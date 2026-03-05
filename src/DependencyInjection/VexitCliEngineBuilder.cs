/************************************************
 *
 * Copyright:		© 2025 VEXIT ®, www.vexit.com, Tomorrow is today...
 * Author:      	Vex Tatarevic
 * Date Created:    2025-11-03 - Initial creation for CLI engine builder
 * DateUpdated:		2025-11-03
 *
 ************************************************/

using Microsoft.Extensions.DependencyInjection;
using static Vexit.CliEngine.CliEngineExtensions;

namespace Vexit.CliEngine.DependencyInjection;

/// <summary>
/// Builder for configuring the Vexit CLI Engine with commands and services. <br/>
/// Used by modules to register themselves during initialization.
/// </summary>
public class VexitCliEngineBuilder : IVexitCliEngineBuilder
{
    private readonly IServiceCollection _services;
    private readonly CliEngineOptions _options;

    internal VexitCliEngineBuilder(IServiceCollection services, CliEngineOptions options)
    {
        _services = services;
        _options = options;
    }

    /// <summary>
    /// Registers a command type with the CLI engine.
    /// </summary>
    /// <typeparam name="TCommand">The command type to register.</typeparam>
    public void AddCommand<TCommand>() where TCommand : class
    {
        // Commands are discovered via reflection, so this is a no-op for now
        // In the future, this could pre-register commands or validate them
    }

    /// <summary>
    /// Registers a service registry type with the CLI engine.
    /// The registry will be used to provide services to commands in groups.
    /// </summary>
    /// <typeparam name="TRegistry">The service registry type to register.</typeparam>
    public void AddServiceRegistry<TRegistry>() where TRegistry : class, IServiceRegistry
    {
        // Service registries are used at runtime via ServiceProviderCache
        // This method can be used for validation or setup if needed
    }
}

