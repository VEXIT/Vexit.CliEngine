/************************************************
 *
 * Copyright:		© 2025 VEXIT ®, www.vexit.com, Tomorrow is today...
 * Author:      	Vex Tatarevic
 * Date Created:    2025-10-28 - Initial creation for hierarchical command support
 * DateUpdated:		
 *
 ************************************************/

using Vexit.CliEngine.BaseClasses;

namespace Vexit.CliEngine;

/// <summary>
/// Represents a node in the command hierarchy tree
/// </summary>
public class CommandNode
{
    public string Name { get; set; } = string.Empty;
    public string? ShortName { get; set; }
    public List<string> Aliases { get; set; } = new();
    public string Description { get; set; } = string.Empty;
    public Type? CommandType { get; set; }
    
    public bool IsGroup { get; init; }
    public bool IsLeaf => !IsGroup && Children.Count == 0;
    public Dictionary<string, CommandNode> Children { get; } = new(StringComparer.OrdinalIgnoreCase);
    public CommandNode? Parent { get; set; }
    public string FullPath => GetFullPath();
    
    /// <summary>
    /// Gets all names (primary + short + aliases) for this node
    /// </summary>
    public IEnumerable<string> AllNames
    {
        get
        {
            yield return Name;
            if (!string.IsNullOrWhiteSpace(ShortName))
                yield return ShortName;
            foreach (var alias in Aliases)
                yield return alias;
        }
    }

    private string GetFullPath()
    {
        var parts = new List<string>();
        var current = this;
        while (current != null && !string.IsNullOrEmpty(current.Name))
        {
            parts.Insert(0, current.Name);
            current = current.Parent;
        }
        return string.Join(" ", parts);
    }

    public void AddChild(CommandNode child)
    {
        child.Parent = this;
        Children[child.Name] = child;
    }

    public CommandNode? FindChild(string name)
    {
        // Try direct lookup first
        if (Children.TryGetValue(name, out var child))
            return child;
        
        // Try matching against all names (including aliases)
        return Children.Values.FirstOrDefault(c => c.AllNames.Any(n => n.Equals(name, StringComparison.OrdinalIgnoreCase)));
    }

    public CommandNode? Resolve(string[] path, int startIndex = 0)
    {
        if (startIndex >= path.Length)
            return this;

        var segment = path[startIndex];
        var child = FindChild(segment);

        if (child == null)
            return this; // Return current node if path can't continue

        return child.Resolve(path, startIndex + 1);
    }
}

