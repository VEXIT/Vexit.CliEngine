/************************************************
 *
 * Copyright:		© 2025 VEXIT ®, www.vexit.com, Tomorrow is today...
 * Author:      	Vex Tatarevic
 * Date Created:    2025-09-15 - Initial creation for Parsed Arguments
 * DateUpdated:		2025-10-27	| Vex | Apply parsed values for Argument (positional) and Option (named), strong typing and generic support; support both generic and non-generic attribute forms
 *
 ************************************************/

using System.Reflection;
using Vexit.CliEngine.Attributes;
using Vexit.CliEngine.BaseClasses;

namespace Vexit.CliEngine;

/// <summary>
/// Holds the parsed arguments and their values for a command
/// </summary>
public class ParsedArguments
{
    private readonly Dictionary<string, object?> _argumentValues = new();
    private readonly List<string> _validationErrors = new();

    /// <summary>
    /// Sets the value for an argument
    /// </summary>
    public void SetValue(string argumentName, object? value)
    {
        _argumentValues[argumentName] = value;
    }

    /// <summary>
    /// Gets the value for an argument
    /// </summary>
    public object? GetValue(string argumentName)
    {
        return _argumentValues.TryGetValue(argumentName, out var value) ? value : null;
    }

    /// <summary>
    /// Gets the value for an argument with type conversion
    /// </summary>
    public T? GetValue<T>(string argumentName)
    {
        return (T?)GetValue(argumentName);
    }

    /// <summary>
    /// Checks if an argument has been set
    /// </summary>
    public bool HasValue(string argumentName)
    {
        return _argumentValues.ContainsKey(argumentName);
    }

    /// <summary>
    /// Adds a validation error
    /// </summary>
    public void AddValidationError(string error)
    {
        _validationErrors.Add(error);
    }

    /// <summary>
    /// Gets all validation errors
    /// </summary>
    public IReadOnlyList<string> ValidationErrors => _validationErrors.AsReadOnly();

    /// <summary>
    /// Checks if there are any validation errors
    /// </summary>
    public bool HasValidationErrors => _validationErrors.Count > 0;

    /// <summary>
    /// Applies the parsed values to the command properties via reflection
    /// </summary>
    public void ApplyToCommand(CmdBase command)
    {
        var properties = command.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            // Find ArgumentAttribute (non-generic only, type inferred from property)
            var argAttr = property.GetCustomAttribute<ArgumentAttribute>(inherit: false);
            if (argAttr != null)
            {
                var argumentName = argAttr.Name;
                if (HasValue(argumentName))
                {
                    property.SetValue(command, GetValue(argumentName));
                }
                continue;
            }

            // Find OptionAttribute (non-generic only, type inferred from property)
            var optAttr = property.GetCustomAttribute<OptionAttribute>(inherit: false);
            if (optAttr != null)
            {
                var argumentName = optAttr.LongName;
                if (HasValue(argumentName))
                {
                    property.SetValue(command, GetValue(argumentName));
                }
                continue;
            }
        }
    }
}