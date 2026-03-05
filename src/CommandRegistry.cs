/************************************************
 *
 * Copyright:		© 2025 VEXIT ®, www.vexit.com, Tomorrow is today...
 * Author:      	Vex Tatarevic
 * Date Created:    2025-10-28 - Initial creation for hierarchical command registry
* Date Updated:    2025-11-10 | Vex | Refactored command discovery to support vertical slicing and explicit command groups
*                  2025-12-01 | Vex | Enforced slice-folder naming convention and auto-generated group nodes when CmdGroup classes are absent
 ************************************************/

using System.Reflection;
using Vexit.CliEngine.Attributes;
using Vexit.CliEngine.BaseClasses;
using Vexit.CliEngine.Utils;

namespace Vexit.CliEngine;

/// <summary>
/// Registry for hierarchical command discovery and resolution
/// </summary>
public class CommandRegistry
{
    private readonly CommandNode _root = new CommandNode { Name = "", IsGroup = true };
    private readonly string _commandsNamespace;

    public CommandRegistry(string commandsNamespace = "Commands")
    {
        _commandsNamespace = commandsNamespace;
    }

    public CommandNode Root => _root;

    /// <summary>
    /// Discovers and registers all commands from the given assemblies
    /// </summary>
    public void Discover(IEnumerable<Assembly> assemblies)
    {
        var commandTypes = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(CmdBase).IsAssignableFrom(t) && !t.IsAbstract)
            // Ensure true group types (non-executable containers) are processed first,
            // so that parent nodes exist before children are registered.
            .OrderByDescending(CmdTypeUtil.IsGroupDefinition)
            .ToList();

        foreach (var type in commandTypes)
        {
            RegisterCommand(type);
        }
    }

    private void RegisterCommand(Type commandType)
    {
        var attr = commandType.GetCustomAttribute<CommandAttribute>();
        if (attr == null)
            return; // Skip types without CommandAttribute

        // Find the best parent for this command in the existing tree.
        // This logic correctly places commands under existing groups or at the root.
        var isGroup = CmdTypeUtil.IsGroupDefinition(commandType);
        var parentNode = FindBestParentGroupNode(commandType, isGroup);

        // Get aliases
        var aliases = commandType.GetCustomAttributes<AliasesAttribute>()
            .Select(a => a.Name)
            .ToList();

        // Create the node for the current command
        var newNode = new CommandNode
        {
            Name = attr.Name, // Use the name from the attribute directly
            ShortName = attr.ShortName,
            Aliases = aliases,
            Description = attr.Description,
            CommandType = commandType,
            IsGroup = isGroup
        };

        parentNode.AddChild(newNode);
    }

    /// <summary>
    /// Finds the parent group node using the CLI naming conventions:<br />
    /// - Groups: <c>Commands/[Group]/[Group]CmdGroup.cs</c> (e.g., <c>Commands/Proj/ProjCmdGroup.cs</c>)<br />
    /// - Simple commands (root): <c>Commands/[Command]Cmd.cs</c> (e.g., <c>Commands/GreetCmd.cs</c>)<br />
    /// - Simple commands (group): <c>Commands/[Group]/[Command]Cmd.cs</c> (e.g., <c>Commands/Proj/ListCmd.cs</c>)<br />
    /// - Complex commands (slice): <c>Commands/[Slice]/[Slice]Cmd.cs</c> (e.g., <c>Commands/Init/InitCmd.cs</c>)<br />
    /// Above examples provide commands that would look like:<br />
    /// <code>
    /// myapp proj
    /// myapp greet
    /// myapp proj list
    /// myapp init
    /// </code>
    /// </summary>
    private CommandNode FindBestParentGroupNode(Type commandType, bool isGroup)
    {
        if (!CmdTypeUtil.TryGetCommandSegments(commandType, _commandsNamespace, out var segments))
            return _root;

        // Groups themselves belong under the path excluding the final segment
        if (isGroup)
        {
            var parentSegments = segments.Length > 0 ? segments[..^1] : Array.Empty<string>();
            return parentSegments.Length == 0 ? _root : EnsureGroupNodePath(parentSegments);
        }

        if (segments.Length == 0)
        {
            return _root;
        }

        var classBaseName = CmdTypeUtil.StripCmdSuffix(commandType.Name);
        var lastSegment = segments[^1];
        var folderMatchesCommand = classBaseName.Equals(lastSegment, StringComparison.OrdinalIgnoreCase);

        // If the folder matches the command name (vertical slice), treat remaining segments as group path.
        // Example: Commands.Init.InitCmd -> root command (no parent segments)
        // Example: Commands.New.ApiServer.ApiServerCmd -> parent is "new"
        if (folderMatchesCommand)
        {
            if (segments.Length == 1)
            {
                return _root;
            }

            var parentSegments = segments[..^1];
            return EnsureGroupNodePath(parentSegments);
        }

        // Otherwise, treat the entire namespace path as command groups and ensure nodes exist.
        return EnsureGroupNodePath(segments);
    }

    /// <summary>
    /// Ensures that each namespace segment has a corresponding group node.
    /// Auto-generates group nodes when no explicit CmdGroup exists.
    /// </summary>
    private CommandNode EnsureGroupNodePath(string[] segments)
    {
        if (segments.Length == 0)
            return _root;

        var currentNode = _root;

        foreach (var segment in segments)
        {
            var normalizedSegment = ToKebabCase(segment);
            var childNode = currentNode.FindChild(normalizedSegment);

            if (childNode == null)
            {
                childNode = new CommandNode
                {
                    Name = normalizedSegment,
                    Description = $"Commands under '{normalizedSegment}'.",
                    IsGroup = true
                };

                currentNode.AddChild(childNode);
            }

            currentNode = childNode;
        }

        return currentNode;
    }

    private string ToKebabCase(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        // Insert hyphen before uppercase letters (except first) and convert to lowercase
        var result = new System.Text.StringBuilder();
        for (int i = 0; i < input.Length; i++)
        {
            if (i > 0 && char.IsUpper(input[i]) && !char.IsUpper(input[i - 1]))
            {
                result.Append('-');
            }
            result.Append(char.ToLowerInvariant(input[i]));
        }
        return result.ToString();
    }

    /// <summary>
    /// Resolves a command path to a command node
    /// </summary>
    public (CommandNode? node, string[] consumedPath, string[] remainingArgs) Resolve(string[] args)
    {
        var consumedPath = new List<string>();
        var currentNode = _root;
        int i = 0;

        while (i < args.Length)
        {
            var arg = args[i];

            // Stop if we hit an option flag
            if (arg.StartsWith("-"))
                break;

            var child = currentNode.FindChild(arg);
            if (child == null)
                break; // No more matching path segments

            consumedPath.Add(arg);
            currentNode = child;
            i++;

            // If we found a leaf command, stop consuming path
            if (child.IsLeaf)
                break;
        }

        var remainingArgs = args.Skip(i).ToArray();
        return (currentNode == _root ? null : currentNode, consumedPath.ToArray(), remainingArgs);
    }

    /// <summary>
    /// Gets all commands at a specific node (for help display)
    /// </summary>
    public IEnumerable<CommandNode> GetChildCommands(CommandNode node)
    {
        return node.Children.Values.OrderBy(c => c.Name);
    }
}

