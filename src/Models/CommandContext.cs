/************************************************
 *
 * Copyright:		© 2025 VEXIT ®, www.vexit.com, Tomorrow is today...
 * Author:      	Vex Tatarevic
 * Date Created:    2025-11-08 - Lightweight context for command startup hooks
 * Date Updated:	
 *
 ************************************************/

namespace Vexit.CliEngine.Models;

/// <summary>
/// Lightweight execution context passed to command startup hooks.
/// </summary>
public sealed class CommandContext
{
	/// <summary>
	/// Raw command-line arguments remaining for the target command.
	/// </summary>
	public string[] Args { get; }

	/// <summary>
	/// The current working directory at the time of execution.
	/// </summary>
	public string WorkingDirectory { get; }

	public CommandContext(string[] args, string workingDirectory)
	{
		Args = args;
		WorkingDirectory = workingDirectory;
	}
}


