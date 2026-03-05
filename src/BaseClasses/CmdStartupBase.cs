/************************************************
 *
 * Copyright:		© 2025 VEXIT ®, www.vexit.com, Tomorrow is today...
 * Author:      	Vex Tatarevic
 * Date Created:    2025-11-08 - Base class for per-command startup hooks
 * Date Updated:
 *
 ************************************************/

using Microsoft.Extensions.DependencyInjection;
using Vexit.CliEngine.Models;

namespace Vexit.CliEngine.BaseClasses;

/// <summary>
/// Base class for command-scoped startup. <br />
/// Startups can override lifecycle hooks to customize behavior for a specific command slice.
/// </summary>
public abstract class CmdStartupBase
{
	/// <summary>
	/// Optional DI configuration hook. <br />
	/// Override in a command startup to explicitly wire services based on runtime context.
	/// </summary>
	/// <param name="services">Service collection to register dependencies.</param>
	/// <param name="context">Lightweight command execution context (args, working directory).</param>
	public virtual void Program_AddServices(IServiceCollection services, CommandContext context)
	{
		// Default: no-op. Convention-based DI will act as a safety net.
	}
}


