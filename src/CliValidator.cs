/************************************************
 *
 * Copyright:		© 2025 VEXIT ®, www.vexit.com, Tomorrow is today...
 * Author:      	Vex Tatarevic
 * Date Created:    2025-09-15 - Initial creation for Cli Validator
 * DateUpdated:		2025-10-27	| Vex | Validation split for positional Argument and named Option, MultiValue validation (incl. Min/MaxCount), and custom messages
 *
 ************************************************/

using System.Collections;
using System.ComponentModel;
using System.Reflection;
using Vexit.CliEngine.Attributes;
using Vexit.CliEngine.BaseClasses;

namespace Vexit.CliEngine;

/// <summary>
/// Validates parsed arguments against command metadata
/// </summary>
public class CliValidator
{
    /// <summary>
    /// Validates the parsed arguments against the command's requirements
    /// </summary>
    public bool Validate(CmdBase command, ParsedArguments parsedArgs)
    {
        var properties = command.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

        // Positional collections must be last
        var positionalProps = properties
            .Select(p => new { Prop = p, Attr = p.GetCustomAttribute<ArgumentAttribute>(false) })
            .Where(x => x.Attr != null)
            .OrderBy(x => x.Prop.MetadataToken)
            .ToList();

        if (positionalProps.Count > 0)
        {
            var lastIndex = positionalProps.Count - 1;
            for (int i = 0; i < lastIndex; i++)
            {
                var attr = positionalProps[i].Attr!;
                if (attr.IsMultiValue)
                {
                    parsedArgs.AddValidationError($"Positional collection '{attr.Name}' must be the last positional argument.");
                }
            }
        }

        foreach (var property in properties)
        {
            var argAttr = property.GetCustomAttribute<ArgumentAttribute>(false);
            if (argAttr != null)
            {
                // Forbid non-generic IEnumerable (no element type) and object
                if (IsInvalidPropertyType(property))
                {
                    parsedArgs.AddValidationError($"Property '{property.Name}' has unsupported type '{property.PropertyType.Name}'. Use array or generic IEnumerable/IList<T> and avoid object.");
                }
                ValidateArgument(property, argAttr, parsedArgs);
                continue;
            }

            var optAttr = property.GetCustomAttribute<OptionAttribute>(false);
            if (optAttr != null)
            {
                if (IsInvalidPropertyType(property))
                {
                    parsedArgs.AddValidationError($"Property '{property.Name}' has unsupported type '{property.PropertyType.Name}'. Use array or generic IEnumerable/IList<T> and avoid object.");
                }
                ValidateOption(property, optAttr, parsedArgs);
                continue;
            }
        }

        return !parsedArgs.HasValidationErrors;
    }

    private void ValidateArgument(PropertyInfo property, dynamic attribute, ParsedArguments parsedArgs)
    {
        var argumentName = attribute.Name;

        if (attribute.IsRequired && !parsedArgs.HasValue(argumentName))
        {
            parsedArgs.AddValidationError(attribute.ValidationMessage ?? $"{argumentName} is required.");
            return;
        }

        if (parsedArgs.HasValue(argumentName))
        {
            var value = parsedArgs.GetValue(argumentName);
            ValidateValue(property, value, parsedArgs, argumentName, attribute.ValidationMessage, attribute.MinCount, attribute.MaxCount);
        }
    }

    private void ValidateOption(PropertyInfo property, dynamic attribute, ParsedArguments parsedArgs)
    {
        var argumentName = attribute.LongName;

        if (attribute.IsRequired && !parsedArgs.HasValue(argumentName))
        {
            parsedArgs.AddValidationError(attribute.ValidationMessage ?? $"--{argumentName} is required.");
            return;
        }

        if (parsedArgs.HasValue(argumentName))
        {
            var value = parsedArgs.GetValue(argumentName);
            ValidateValue(property, value, parsedArgs, argumentName, attribute.ValidationMessage, attribute.MinCount, attribute.MaxCount);
        }
    }

    private void ValidateValue(PropertyInfo property, object? value, ParsedArguments parsedArgs, string argumentName, string? validationMessage, int minCount, int maxCount)
    {
        var expectedType = property.PropertyType;

        if (value == null)
        {
            if (minCount > 0)
            {
                parsedArgs.AddValidationError(validationMessage ?? $"{argumentName} requires at least {minCount} value(s).");
            }
            return;
        }

        if (value is IList list)
        {
            if (list.Count < minCount)
            {
                parsedArgs.AddValidationError(validationMessage ?? $"{argumentName} requires at least {minCount} value(s).");
            }
            if (maxCount > 0 && list.Count > maxCount)
            {
                parsedArgs.AddValidationError(validationMessage ?? $"{argumentName} allows at most {maxCount} value(s).");
            }
            return;
        }

        // Non-list value - treat as single
        if (minCount > 1)
        {
            parsedArgs.AddValidationError(validationMessage ?? $"{argumentName} requires at least {minCount} value(s).");
        }

        // Type validation/conversion (for single value)
        if (value is string stringValue)
        {
            try
            {
                var converter = TypeDescriptor.GetConverter(expectedType);
                if (converter.CanConvertFrom(typeof(string)))
                {
                    var converted = converter.ConvertFromString(stringValue);
                    parsedArgs.SetValue(argumentName, converted);
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }
            catch
            {
                parsedArgs.AddValidationError(validationMessage ?? $"{argumentName} must be a valid {expectedType.Name}.");
            }
        }
    }

    private static bool IsInvalidPropertyType(PropertyInfo property)
    {
        var t = property.PropertyType;
        if (t == typeof(object)) return true;
        if (t == typeof(string)) return false;
        if (typeof(System.Collections.IEnumerable).IsAssignableFrom(t))
        {
            if (t.IsArray) return false;
            if (t.IsGenericType) return t.GetGenericArguments().Length != 1; // invalid when no element type
            return true; // non-generic IEnumerable
        }
        return false;
    }
}