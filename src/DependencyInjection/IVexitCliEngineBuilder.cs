/************************************************
 *
 * Copyright:		© 2025 VEXIT ®, www.vexit.com, Tomorrow is today...
 * Author:      	Vex Tatarevic
 * Date Created:    2025-11-03 - Initial creation for selective DI support
 * DateUpdated:		2025-11-03
 *
 ************************************************/

namespace Vexit.CliEngine.DependencyInjection;

/// <summary>
/// Builder interface for configuring the Vexit CLI Engine with modules and services. <br/>
/// Used by modules to register themselves during initialization.
/// </summary>
public interface IVexitCliEngineBuilder
{
    /// <summary>
    /// Registers a command type with the CLI engine.
    /// </summary>
    /// <typeparam name="TCommand">The command type to register.</typeparam>
    void AddCommand<TCommand>() where TCommand : class;

    /// <summary>
    /// Registers a service registry type with the CLI engine.
    /// The registry will be used to provide services to commands in groups.
    /// </summary>
    /// <typeparam name="TRegistry">The service registry type to register.</typeparam>
    void AddServiceRegistry<TRegistry>() where TRegistry : class, IServiceRegistry;
}
