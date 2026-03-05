/************************************************
 *
 * Copyright:		© 2025 VEXIT ®, www.vexit.com, Tomorrow is today...
 * Author:      	Vex Tatarevic
 * Date Created:    2025-11-11 - Initial creation for the command executor interface
 * DateUpdated:		
 *
 ************************************************/

using Vexit.Common.Models;

namespace Vexit.CliEngine.DependencyInjection;

/// <summary>
/// Defines a contract for a service that can execute CLI commands programmatically. <br />
/// This provides a decoupled way for commands (e.g., an interactive shell) <br />
/// to trigger the execution of other commands.
/// </summary>
public interface ICommandExecutor
{
    /// <summary>
    /// Executes a command based on the provided arguments.
    /// </summary>
    /// <param name="args">The command-line arguments.</param>
    /// <returns>The result of the command execution.</returns>
    Task<Result> Execute(string[] args);

    /// <summary>
    /// Parses a raw command-line string and executes it.
    /// </summary>
    /// <param name="commandLine">The raw command-line string (e.g., "init --path C:\temp").</param>
    /// <returns>The result of the command execution.</returns>
    Task<Result> Execute(string commandLine);
}
