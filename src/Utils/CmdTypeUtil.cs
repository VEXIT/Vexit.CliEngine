/************************************************
 *
 * Copyright:		© 2025 VEXIT ®, www.vexit.com, Tomorrow is today...
 * Author:      	Vex Tatarevic
 * Date Created:    2025-11-17 - Consolidated command type inspection helpers
 *
 ************************************************/

using Vexit.CliEngine.BaseClasses;

namespace Vexit.CliEngine.Utils;

/// <summary>
/// Helpers for command discovery that assume the documented naming conventions:<br />
/// - Groups: <c>Commands/[Group]/[Group]CmdGroup.cs</c> (e.g., <c>Commands/Proj/ProjCmdGroup.cs</c>)<br />
/// - Simple commands (root): <c>Commands/[Command]Cmd.cs</c> (e.g., <c>Commands/GreetCmd.cs</c>)<br />
/// - Simple commands (group): <c>Commands/[Group]/[Command]Cmd.cs</c> (e.g., <c>Commands/Proj/ListCmd.cs</c>)<br />
/// - Complex commands (slice): <c>Commands/[Slice]/[Slice]Cmd.cs</c> (e.g., <c>Commands/Init/InitCmd.cs</c>)
/// Above examples provide commands that would look like:<br />
/// <code>
/// myapp proj
/// myapp greet
/// myapp proj list
/// myapp init
/// </code>
/// </summary>
internal static class CmdTypeUtil
{
    private const string CommandSuffix = "Cmd";

    /// <summary>
    /// Determines whether the provided type is a concrete command group definition.
    /// Groups are the classes that inherit directly from <see cref="CmdGroupBase"/>.
    /// </summary>
    public static bool IsGroupDefinition(Type type)
    {
        if (type == null)
            return false;

        return type.BaseType == typeof(CmdGroupBase);
    }

    /// <summary>
    /// Attempts to extract the namespace segments that appear after the configured commands namespace.
    /// Example: Vexit.VxCli.Commands.Proj.IdCmd -> ["Proj"]
    /// </summary>
    public static bool TryGetCommandSegments(Type commandType, string commandsNamespace, out string[] segments)
    {
        segments = Array.Empty<string>();

        if (commandType == null || string.IsNullOrWhiteSpace(commandsNamespace))
            return false;

        var fullNamespace = commandType.Namespace ?? string.Empty;
        var searchString = $".{commandsNamespace}.";
        var commandsIndex = fullNamespace.IndexOf(searchString, StringComparison.OrdinalIgnoreCase);

        if (commandsIndex == -1)
            return false;

        var relevantNamespace = fullNamespace.Substring(commandsIndex + searchString.Length);
        if (string.IsNullOrWhiteSpace(relevantNamespace))
            return false;

        segments = relevantNamespace.Split('.', StringSplitOptions.RemoveEmptyEntries);
        return segments.Length > 0;
    }

    /// <summary>
    /// Removes the conventional "Cmd" suffix from a command class name if present.
    /// </summary>
    public static string StripCmdSuffix(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return name;

        return name.EndsWith(CommandSuffix, StringComparison.OrdinalIgnoreCase)
            ? name[..^CommandSuffix.Length]
            : name;
    }
}

