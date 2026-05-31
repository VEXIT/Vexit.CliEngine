/************************************************
 *
 * Copyright:		© 2025 VEXIT ®, www.vexit.com, Tomorrow is today...
 * Author:      	Vex Tatarevic
 * Date Created:    2025-09-15 - Initial creation for Cli Parser
 * DateUpdated:		2025-10-27	| Vex | Redesign parser to handle positional Argument vs named Option, MultiValue (incl. variadic), key=value, and strong typing
 *                  2026-04-27 | Vex | Parse inline boolean option tri-state (e.g. --opt-in=true, --opt-in=false, --opt-in) and treat supplied bare nullable-boolean flags (e.g. --opt-in) as true.
 *
 ************************************************/

using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using Vexit.CliEngine.Attributes;
using Vexit.CliEngine.BaseClasses;

namespace Vexit.CliEngine;

/// <summary>
/// Parses command line arguments using reflection to read command metadata
/// </summary>
public class CliParser
{
    private readonly CliValidator _validator;

    private class ArgumentDef
    {
        public PropertyInfo Prop { get; }
        public Type ValueType { get; }
        public string Name { get; }
        public bool IsMultiValue { get; }
        public int MinCount { get; }
        public int MaxCount { get; }
        public string? ValidationMessage { get; }

        public ArgumentDef(PropertyInfo prop, Type valueType, string name, bool isMultiValue, int minCount, int maxCount, string? validationMessage)
        {
            Prop = prop;
            ValueType = valueType;
            Name = name;
            IsMultiValue = isMultiValue;
            MinCount = minCount;
            MaxCount = maxCount;
            ValidationMessage = validationMessage;
        }
    }

    private class OptionDef
    {
        public PropertyInfo Prop { get; }
        public Type ValueType { get; }
        public string LongName { get; }
        public string? ShortName { get; }
        public bool IsRequired { get; }
        public bool IsMultiValue { get; }
        public int MinCount { get; }
        public int MaxCount { get; }
        public string? ValidationMessage { get; }

        public OptionDef(PropertyInfo prop, Type valueType, string longName, string? shortName, bool isRequired, bool isMultiValue, int minCount, int maxCount, string? validationMessage)
        {
            Prop = prop;
            ValueType = valueType;
            LongName = longName;
            ShortName = shortName;
            IsRequired = isRequired;
            IsMultiValue = isMultiValue;
            MinCount = minCount;
            MaxCount = maxCount;
            ValidationMessage = validationMessage;
        }
    }

    public CliParser()
    {
        _validator = new CliValidator();
    }

    /// <summary>
    /// Parses command line arguments for the specified command
    /// </summary>
    public ParsedArguments Parse(CmdBase command, string[] args)
    {
        var parsedArgs = new ParsedArguments();
        var defs = GetDefinitions(command);

        // Parse the arguments
        ParseArguments(args, defs, parsedArgs);

        // Validate the parsed arguments
        _validator.Validate(command, parsedArgs);

        return parsedArgs;
    }

    private (List<ArgumentDef> Positionals,
             Dictionary<string, OptionDef> Options,
             Dictionary<string, OptionDef> ShortOptions) GetDefinitions(CmdBase command)
    {
        var positionals = new List<ArgumentDef>();
        var options = new Dictionary<string, OptionDef>(StringComparer.OrdinalIgnoreCase);
        var shortOptions = new Dictionary<string, OptionDef>(StringComparer.OrdinalIgnoreCase);

        var properties = command.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var property in properties)
        {
            // Find ArgumentAttribute (non-generic only, type inferred from property)
            var argAttr = property.GetCustomAttribute<ArgumentAttribute>(inherit: false);
            if (argAttr != null)
            {
                argAttr.OnInitProp(property);
                var valueType = GetValueTypeFromProperty(property);
                positionals.Add(new ArgumentDef(property, valueType, argAttr.Name, argAttr.IsMultiValue, argAttr.MinCount, argAttr.MaxCount, argAttr.ValidationMessage));
                continue;
            }

            // Find OptionAttribute (non-generic only, type inferred from property)
            var optAttr = property.GetCustomAttribute<OptionAttribute>(inherit: false);
            if (optAttr != null)
            {
                optAttr.OnInitProp(property);
                var valueType = GetValueTypeFromProperty(property);
                var def = new OptionDef(property, valueType, optAttr.LongName, optAttr.ShortName, optAttr.IsRequired, optAttr.IsMultiValue, optAttr.MinCount, optAttr.MaxCount, optAttr.ValidationMessage);
                options[optAttr.LongName] = def;
                if (!string.IsNullOrEmpty(optAttr.ShortName))
                    shortOptions[optAttr.ShortName!] = def;
                continue;
            }

            // No attribute – skip
        }

        positionals = positionals.OrderBy(p => p.Prop.MetadataToken).ToList();
        return (positionals, options, shortOptions);
    }

    private void ParseArguments(string[] args, (List<ArgumentDef> Positionals, Dictionary<string, OptionDef> Options, Dictionary<string, OptionDef> ShortOptions) defs, ParsedArguments parsedArgs)
    {
        var positionalArgs = new List<string>();
        var namedArgsStart = -1;

        for (int i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            if (arg.StartsWith("--"))
            {
                namedArgsStart = i;
                break;
            }
            else if (arg.StartsWith("-"))
            {
                // Treat numeric tokens like -5, -10.5 as positional values during positional phase
                if (IsNumericToken(arg))
                {
                    positionalArgs.Add(arg);
                    continue;
                }

                // Short option only if single-letter flag like -n
                if (arg.Length == 2 && char.IsLetter(arg[1]))
                {
                    namedArgsStart = i;
                    break;
                }

                // Any other '-...' here is invalid in positional phase
                parsedArgs.AddValidationError($"'{arg}' is not a valid argument format. Use --{arg.TrimStart('-')} instead.");
                return;
            }
            else if (arg.Contains("="))
            {
                namedArgsStart = i;
                break;
            }
            else
            {
                positionalArgs.Add(arg);
            }
        }

        ParsePositionalArguments(positionalArgs, defs.Positionals, parsedArgs);

        if (namedArgsStart >= 0)
        {
            for (int i = namedArgsStart; i < args.Length; i++)
            {
                var arg = args[i];
                if (arg.StartsWith("--"))
                {
                    var rest = arg[2..];
                    var eq = rest.IndexOf('=');
                    if (eq >= 0)
                    {
                        var optName = rest[..eq];
                        var optVal = rest[(eq + 1)..];
                        ParseOption(optName, defs.Options, defs.ShortOptions, parsedArgs, new[] { optVal }, ref i, isKeyValue: true);
                    }
                    else
                    {
                        ParseOption(rest, defs.Options, defs.ShortOptions, parsedArgs, args, ref i);
                    }
                }
                else if (arg.StartsWith("-") && arg.Length == 2)
                {
                    var argName = arg.Substring(1);
                    ParseOption(argName, defs.Options, defs.ShortOptions, parsedArgs, args, ref i, isShort: true);
                }
                else if (arg.Contains("="))
                {
                    var parts = arg.Split('=', 2);
                    if (parts.Length == 2)
                    {
                        ParseOption(parts[0], defs.Options, defs.ShortOptions, parsedArgs, new[] { parts[1] }, ref i, isKeyValue: true);
                    }
                }
                else
                {
                    parsedArgs.AddValidationError($"Unexpected positional argument '{arg}' after named arguments.");
                }
            }
        }
    }

    private void ParsePositionalArguments(List<string> values, List<ArgumentDef> defs, ParsedArguments parsedArgs)
    {
        if (defs.Count == 0) return;

        var last = defs.LastOrDefault();
        var isVariadic = last != null && last.IsMultiValue && typeof(IEnumerable).IsAssignableFrom(last.Prop.PropertyType) && last.Prop.PropertyType != typeof(string);

        for (int i = 0; i < defs.Count - (isVariadic ? 1 : 0); i++)
        {
            if (i >= values.Count) return;
            var def = defs[i];
            var converted = ConvertValue(values[i], def.ValueType, def.ValidationMessage);
            parsedArgs.SetValue(def.Name, converted);
        }

        if (isVariadic)
        {
            var listType = last!.Prop.PropertyType;
            var itemType = listType.IsGenericType ? listType.GetGenericArguments()[0] : typeof(string);
            var list = (IList)Activator.CreateInstance(listType)!;

            for (int j = defs.Count - 1; j < values.Count; j++)
            {
                list.Add(ConvertValue(values[j], itemType, last.ValidationMessage));
            }
            parsedArgs.SetValue(last.Name, list);
        }
        else if (values.Count > defs.Count)
        {
            parsedArgs.AddValidationError($"Too many positional arguments. Expected {defs.Count}, got {values.Count}.");
        }
    }

    private void ParseOption(string name, Dictionary<string, OptionDef> longMap, Dictionary<string, OptionDef> shortMap, ParsedArguments parsedArgs, string[] args, ref int index, bool isKeyValue = false, bool isShort = false)
    {
        if (!(isShort ? shortMap.TryGetValue(name, out var def) : longMap.TryGetValue(name, out def)))
        {
            parsedArgs.AddValidationError($"Unknown option: {name}");
            return;
        }

        var prop = def.Prop;
        var optType = def.ValueType;
        var fullName = def.LongName;

        // Treat supplied bare nullable-boolean flags (e.g. --opt-in) as true.
        if (!isKeyValue && (optType == typeof(bool) || Nullable.GetUnderlyingType(optType) == typeof(bool)))
        {
            parsedArgs.SetValue(fullName, true);
            return;
        }

        string? valueStr = null;
        if (isKeyValue)
        {
            valueStr = args.Length > 0 ? args[0] : null;
        }
        else if (index + 1 < args.Length)
        {
            var next = args[index + 1];
            if (!next.StartsWith("-") || IsNumericToken(next))
            {
                index++;
                valueStr = args[index];
            }
        }

        if (string.IsNullOrEmpty(valueStr) && def.IsRequired)
        {
            parsedArgs.AddValidationError(def.ValidationMessage ?? $"--{fullName} requires a value.");
            return;
        }

        var value = ConvertValue(valueStr, optType, def.ValidationMessage);

        if (def.IsMultiValue)
        {
            var existing = parsedArgs.GetValue(fullName) as IList;
            if (existing != null)
            {
                existing.Add(value);
                return;
            }

            // Initialize collection if not present
            var list = Activator.CreateInstance(prop.PropertyType) as IList;
            if (list != null)
            {
                list.Add(value);
                parsedArgs.SetValue(fullName, list);
                return;
            }
        }

        parsedArgs.SetValue(fullName, value);
    }

    private object? ConvertValue(string? valueStr, Type targetType, string? validationMessage)
    {
        if (string.IsNullOrEmpty(valueStr)) return null;

        var converter = TypeDescriptor.GetConverter(targetType);
        if (converter.CanConvertFrom(typeof(string)))
        {
            return converter.ConvertFromString(valueStr);
        }
        throw new InvalidOperationException(validationMessage ?? $"Cannot convert '{valueStr}' to {targetType.Name}");
    }

    private static bool IsNumericToken(string s)
    {
        if (string.IsNullOrEmpty(s)) return false;
        // Consider tokens that parse as a number in invariant culture
        // This will return false for '-n' or other non-numeric flags
        if (double.TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out _))
            return true;
        return false;
    }

    private static Type GetValueTypeFromProperty(PropertyInfo prop)
    {
        var t = prop.PropertyType;
        if (t == typeof(string)) return t;
        if (typeof(IEnumerable).IsAssignableFrom(t))
        {
            if (t.IsArray) return t.GetElementType() ?? typeof(string);
            if (t.IsGenericType) return t.GetGenericArguments()[0];
        }
        return t;
    }
}