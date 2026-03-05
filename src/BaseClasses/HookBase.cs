/************************************************
 *
 * Copyright:		© 2025 VEXIT ®, www.vexit.com, Tomorrow is today...
 * Author:      	Vex Tatarevic
 * Date Created:    2025-11-26 - Base class for lifecycle hooks with default implementations
 * DateUpdated:
 *
 ************************************************/

using Vexit.CliEngine.Models;
using Vexit.Common.Models;

namespace Vexit.CliEngine.BaseClasses;

/// <summary>
/// Base class for lifecycle hooks with default no-op implementations. <br />
/// Override only the methods you need - all methods have default implementations.
/// </summary>
public abstract class HookBase : IHook
{
    /// <summary>
    /// Execution order - lower numbers execute first (default: 0)
    /// </summary>
    public virtual int Order => 0;

    /// <summary>
    /// Executes once per process (before command resolution).
    /// Default: no-op - override if you need startup logic.
    /// </summary>
    public virtual Task OnStartup(CommandContext context)
        => Task.CompletedTask;

    /// <summary>
    /// Executes before command execution and can abort by returning a failure Result.
    /// Default: allow execution - override if you need validation that can block.
    /// </summary>
    public virtual Task<Result> OnBeforeExecuteBlocking(CmdBase command, CommandContext context)
        => Task.FromResult(Result.Ok());

    /// <summary>
    /// Executes before command execution but doesn't block.
    /// Default: no-op - override if you need non-blocking pre-execution logic.
    /// </summary>
    public virtual Task OnBeforeExecute(CmdBase command, CommandContext context)
        => Task.CompletedTask;

    /// <summary>
    /// Executes after command completion (even if command failed).
    /// Default: no-op - override if you need post-execution logic.
    /// </summary>
    public virtual Task OnAfterExecute(CmdBase command, Result result)
        => Task.CompletedTask;
}

