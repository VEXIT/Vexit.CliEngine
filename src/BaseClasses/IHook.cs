/************************************************
 *
 * Copyright:		© 2025 VEXIT ®, www.vexit.com, Tomorrow is today...
 * Author:      	Vex Tatarevic
 * Date Created:    2025-11-26 - Lifecycle hooks interface for CliEngine
 * DateUpdated:
 *
 ************************************************/

using Vexit.CliEngine.Models;
using Vexit.Common.Models;

namespace Vexit.CliEngine.BaseClasses;

/// <summary>
/// Interface for lifecycle hooks that execute at specific points in the CLI execution lifecycle.
/// </summary>
public interface IHook
{
    /// <summary>
    /// Execution order - lower numbers execute first (default: 0)
    /// </summary>
    int Order { get; }

    /// <summary>
    /// Executes once per process (before command resolution).
    /// Perfect for update checks, environment validation, or one-time initialization.
    /// Runs even in interactive shells (once when shell starts).
    /// </summary>
    Task OnStartup(CommandContext context);

    /// <summary>
    /// Executes before command execution and can abort by returning a failure Result.
    /// The command waits for blocking hooks to complete before proceeding.
    /// </summary>
    Task<Result> OnBeforeExecuteBlocking(CmdBase command, CommandContext context);

    /// <summary>
    /// Executes before command execution but doesn't block.
    /// The command proceeds immediately while hooks run in the background.
    /// Perfect for cleanup operations that shouldn't delay commands.
    /// </summary>
    Task OnBeforeExecute(CmdBase command, CommandContext context);

    /// <summary>
    /// Executes after command completion (even if command failed).
    /// Always non-blocking and runs in the background.
    /// </summary>
    Task OnAfterExecute(CmdBase command, Result result);
}

