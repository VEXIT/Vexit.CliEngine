/************************************************
 *
 * Copyright:		© 2025 VEXIT ®, www.vexit.com, Tomorrow is today...
 * Author:      	Vex Tatarevic
 * Date Created:    2025-11-03 - Initial creation for CLI engine factory
 * DateUpdated:		2025-11-03
 *
 ************************************************/

using Microsoft.Extensions.DependencyInjection;
using static Vexit.CliEngine.CliEngineExtensions;

namespace Vexit.CliEngine.DependencyInjection;

/// <summary>
/// Factory for creating CLI engine builders.
/// </summary>
public static class VexitCliEngine
{
    /// <summary>
    /// Creates a new CLI engine builder.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="options">The CLI engine options.</param>
    /// <returns>A configured CLI engine builder.</returns>
    public static IVexitCliEngineBuilder CreateBuilder(IServiceCollection services, CliEngineOptions options)
    {
        return new VexitCliEngineBuilder(services, options);
    }
}
