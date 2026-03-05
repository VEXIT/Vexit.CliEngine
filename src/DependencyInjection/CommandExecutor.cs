/************************************************
 *
 * Copyright:		© 2025 VEXIT ®, www.vexit.com, Tomorrow is today...
 * Author:      	Vex Tatarevic
 * Date Created:    2025-11-11 - Initial creation for the command executor implementation
 * Date Updated:		
 *
 ************************************************/

using Vexit.CliEngine.Models;
using Vexit.CliEngine.Utils;
using Vexit.Common.Models;

namespace Vexit.CliEngine.DependencyInjection;

/// <summary>
/// A concrete implementation of <see cref="ICommandExecutor"/> that acts as a wrapper <br />
/// around the static <see cref="CommandController"/> to allow for dependency injection.
/// </summary>
public class CommandExecutor : ICommandExecutor
{
    /// <inheritdoc />
    public Task<Result> Execute(string[] args)
    {
        // This is a bridge to the static execution entry point of the CLI engine.
        return CommandController.Execute(args);
    }

    /// <inheritdoc />
    public Task<Result> Execute(string commandLine)
    {
        var args = CliUtil.ParseCommandLine(commandLine);
        return Execute(args);
    }
}
