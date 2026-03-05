/************************************************
 *
 * Copyright:		© 2025 VEXIT ®, www.vexit.com, Tomorrow is today...
 * Author:      	Vex Tatarevic
 * Date Created:    2025-10-28 - Initial creation for hierarchical help system
 * DateUpdated:		
 *
 ************************************************/

using System.Reflection;
using System.Text;
using Vexit.CliEngine.Attributes;



namespace Vexit.CliEngine;

/// <summary>
/// Generates help documentation for commands
/// </summary>
public class HelpGenerator
{

    //--------------------------------------------------
    //  Fields
    //--------------------------------------------------
    private readonly CommandRegistry _registry;

    /// <summary>
    /// Passed in from MetaCliExtensions.MetaCliOptions <br/>
    /// Options are set in the Program.cs file of the consuming application like: <br />
    /// <code>
    /// builder.AddMetaCli(options =>
    /// {
    ///     options.CliName = "VMod CLI";
    /// });
    /// </code>
    /// </summary>
    private readonly string _cliName;
    private readonly string _pageName = "Help";
    private string PageTitle => $"{_cliName} - {_pageName}";


    //--------------------------------------------------
    //  Constructor
    //--------------------------------------------------

    public HelpGenerator(CommandRegistry registry, string cliName)
    {
        _registry = registry;
        _cliName = cliName;
    }

    //--------------------------------------------------
    //  Methods
    //--------------------------------------------------

    /// <summary>
    /// Generate help for a command node
    /// </summary>
    public string Generate(CommandNode node)
    {
        if (node.IsLeaf)
        {
            return GenerateLeafHelp(node);
        }
        else
        {
            return GenerateGroupHelp(node);
        }
    }

    private string GenerateGroupHelp(CommandNode node)
    {
        var sb = new StringBuilder();

        sb.AppendLine(Com.Title(PageTitle));

        if (!string.IsNullOrEmpty(node.FullPath))
        {
            sb.AppendLine($"Command: {node.FullPath}");
            sb.AppendLine($"Description: {node.Description}");
            sb.AppendLine();
        }

        sb.AppendLine("Available commands:");
        sb.AppendLine();

        var children = _registry.GetChildCommands(node);
        foreach (var child in children)
        {
            var type = child.IsLeaf ? "[command]" : "[group]  ";
            var nameWithAliases = FormatNameWithAliases(child);
            sb.AppendLine($"  {type} {nameWithAliases,-25} {child.Description}");
        }

        sb.AppendLine();
        sb.AppendLine("Usage:");
        if (string.IsNullOrEmpty(node.FullPath))
        {
            sb.AppendLine("  <command> [arguments] [options]");
        }
        else
        {
            sb.AppendLine($"  {node.FullPath} <subcommand> [arguments] [options]");
        }

        sb.AppendLine();
        sb.AppendLine("For more information on a specific command, use:");
        if (string.IsNullOrEmpty(node.FullPath))
        {
            sb.AppendLine("  <command> --help");
        }
        else
        {
            sb.AppendLine($"  {node.FullPath} <subcommand> --help");
        }

        return sb.ToString();
    }

    private string GenerateLeafHelp(CommandNode node)
    {
        var sb = new StringBuilder();
        
        sb.AppendLine(Com.Title($"{PageTitle}"));

        sb.AppendLine($"Command: {node.FullPath}");
        sb.AppendLine($"Description: {node.Description}");
        sb.AppendLine();

        if (node.CommandType == null)
            return sb.ToString();

        // Get arguments and options from the command type
        var properties = node.CommandType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        var arguments = properties
            .Select(p => new { Prop = p, Attr = p.GetCustomAttribute<ArgumentAttribute>() })
            .Where(x => x.Attr != null)
            .ToList();

        var options = properties
            .Select(p => new { Prop = p, Attr = p.GetCustomAttribute<OptionAttribute>() })
            .Where(x => x.Attr != null)
            .ToList();

        // Show arguments
        if (arguments.Any())
        {
            sb.AppendLine("Arguments:");
            foreach (var arg in arguments)
            {
                var required = arg.Attr!.IsRequired ? "[required]" : "[optional]";
                var typeName = GetTypeName(arg.Prop.PropertyType);
                sb.AppendLine($"  <{arg.Attr.Name}> {required,-12} {typeName,-15} {arg.Attr.Description}");
            }
            sb.AppendLine();
        }

        // Show options
        if (options.Any())
        {
            sb.AppendLine("Options:");
            foreach (var opt in options)
            {
                var shortName = !string.IsNullOrEmpty(opt.Attr!.ShortName) ? $"-{opt.Attr.ShortName}, " : "    ";
                var required = opt.Attr.IsRequired ? "[required]" : "[optional]";
                var typeName = GetTypeName(opt.Prop.PropertyType);
                sb.AppendLine($"  {shortName}--{opt.Attr.LongName,-15} {required,-12} {typeName,-15} {opt.Attr.Description}");
            }
            sb.AppendLine();
        }

        // Generate usage example
        sb.AppendLine("Usage:");
        var usage = new StringBuilder($"  {node.FullPath}");

        foreach (var arg in arguments)
        {
            if (arg.Attr!.IsRequired)
                usage.Append($" <{arg.Attr.Name}>");
            else
                usage.Append($" [<{arg.Attr.Name}>]");
        }

        foreach (var opt in options)
        {
            var optName = !string.IsNullOrEmpty(opt.Attr!.ShortName) ? $"-{opt.Attr.ShortName}" : $"--{opt.Attr.LongName}";
            usage.Append($" [{optName}");
            if (opt.Prop.PropertyType != typeof(bool))
                usage.Append($" <{opt.Attr.LongName}>");
            usage.Append("]");
        }

        sb.AppendLine(usage.ToString());
        sb.AppendLine();

        // Generate example
        sb.AppendLine("Example:");
        var example = new StringBuilder($"  {node.FullPath}");

        foreach (var arg in arguments.Where(a => a.Attr!.IsRequired))
        {
            example.Append($" {GetExampleValue(arg.Prop.PropertyType)}");
        }

        if (options.Any())
        {
            var firstOpt = options.First();
            var optName = !string.IsNullOrEmpty(firstOpt.Attr!.ShortName) ? $"-{firstOpt.Attr.ShortName}" : $"--{firstOpt.Attr.LongName}";
            example.Append($" {optName}");
            if (firstOpt.Prop.PropertyType != typeof(bool))
                example.Append($" {GetExampleValue(firstOpt.Prop.PropertyType)}");
        }

        sb.AppendLine(example.ToString());

        return sb.ToString();
    }

    private string GetTypeName(Type type)
    {
        if (type == typeof(string)) return "string";
        if (type == typeof(int)) return "int";
        if (type == typeof(bool)) return "bool";
        if (type == typeof(double) || type == typeof(float)) return "number";

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
        {
            var itemType = type.GetGenericArguments()[0];
            return $"List<{GetTypeName(itemType)}>";
        }

        return type.Name;
    }

    private string GetExampleValue(Type type)
    {
        if (type == typeof(string)) return "value";
        if (type == typeof(int)) return "1";
        if (type == typeof(bool)) return "";
        if (type == typeof(double) || type == typeof(float)) return "1.5";

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
        {
            var itemType = type.GetGenericArguments()[0];
            return $"{GetExampleValue(itemType)}1 {GetExampleValue(itemType)}2";
        }

        return "value";
    }

    private string FormatNameWithAliases(CommandNode node)
    {
        var aliases = new List<string>();

        if (!string.IsNullOrWhiteSpace(node.ShortName))
            aliases.Add(node.ShortName);

        aliases.AddRange(node.Aliases);

        if (aliases.Any())
        {
            return $"{node.Name} ({string.Join(", ", aliases)})";
        }

        return node.Name;
    }
}

