/************************************************
 *
 * Copyright:		© 2025 VEXIT ®, www.vexit.com, Tomorrow is today...
 * Author:      	Vex Tatarevic
 * Date Created:    2025-09-15 - Initial creation inside time.git repository (Time app) - experimental version
 * DateUpdated:		2025-10-27	| Vex | Updated to Vexit.MetaCli namespace and moved to Vexit.MetaCli project. Added ShortName property.
 *
 ************************************************/

namespace Vexit.CliEngine.Attributes;

/// <summary>
/// Marks a class as a command. <br/>
/// Commands are the main entry points for the CLI application.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class CommandAttribute : Attribute
{
    public string Name { get; }
    public string? ShortName { get; }
    public string Description { get; }

    public CommandAttribute(string name, string description = "", string? shortName = null)
    {
        Name = name;
        Description = description;
        ShortName = shortName;
    }
}