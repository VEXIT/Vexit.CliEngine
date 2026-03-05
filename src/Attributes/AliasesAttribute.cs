/************************************************
 *
 * Copyright:		© 2025 VEXIT ®, www.vexit.com, Tomorrow is today...
 * Author:      	Vex Tatarevic
 * Date Created:    2025-10-28 - Initial creation for additional command aliases
 * DateUpdated:		2025-10-28
 *
 ************************************************/

namespace Vexit.CliEngine.Attributes;

/// <summary>
/// Defines additional aliases for a command (beyond the single ShortName). <br/>
/// Can be applied multiple times for multiple aliases
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class AliasesAttribute : Attribute
{
    public string Name { get; }

    public AliasesAttribute(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Alias name cannot be empty", nameof(name));
        
        Name = name;
    }
}

