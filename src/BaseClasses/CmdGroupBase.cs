/************************************************
 *
 * Copyright:		© 2025 VEXIT ®, www.vexit.com, Tomorrow is today...
 * Author:      	Vex Tatarevic
 * Date Created:    2025-10-28 - Initial creation for non-executable group commands
 * DateUpdated:		
 *
 ************************************************/

using Vexit.Common.Models;

namespace Vexit.CliEngine.BaseClasses;

/// <summary>
/// Base class for command groups (non-executable containers for subcommands)
/// Groups can define shared options and provide aliases for the group itself
/// Leaf commands inherit from group classes to get shared options
/// </summary>
public abstract class CmdGroupBase : CmdBase
{
    /// <summary>
    /// Default implementation for groups - should never be called
    /// CommandController shows help for groups instead of executing them
    /// Leaf commands override this with their actual logic
    /// </summary>
    public override Result Execute()
    {
        // Safety net - this should never be called due to CommandController checks
        // But if somehow reached, show a helpful message
        Cli.WriteLnError("This is a command group and cannot be executed directly.");
        Cli.WriteLn("   Use --help to see available subcommands.");
        return Result.Failure("Command groups cannot be executed directly.", "GROUP_EXECUTION_NOT_ALLOWED");
    }
}

