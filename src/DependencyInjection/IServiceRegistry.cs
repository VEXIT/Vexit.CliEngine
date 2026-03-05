/************************************************
 *
 * Copyright:		© 2025 VEXIT ®, www.vexit.com, Tomorrow is today...
 * Author:      	Vex Tatarevic
 * Date Created:    2025-11-03 - Initial creation for selective DI support
 * DateUpdated:		2025-11-03
 *
 ************************************************/

using Microsoft.Extensions.DependencyInjection;

namespace Vexit.CliEngine.DependencyInjection;

/// <summary>
/// Interface for service registries that register a group of related services that can be used by a command.
/// </summary>
public interface IServiceRegistry
{
    /// <summary>
    /// Registers group of related services that can be used by a command.
    /// </summary>
    /// <param name="services">The service collection to register services with.</param>
    void RegisterServices(IServiceCollection services);
}
