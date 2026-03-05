/************************************************
 *
 * Copyright:		© 2025 VEXIT ®, www.vexit.com, Tomorrow is today...
 * Author:      	Vex Tatarevic
 * Date Created:    2025-11-03 - Initial creation for selective DI support
 * DateUpdated:		2026-02-04 | Vex | Added HostServices to the ServiceProviderCache for baseline services DI
 *                  2026-02-05 | Vex | Updated GetOrCreateMany to use ORDERED prefix in cache keys for explicit ordering
 *
 ************************************************/

using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;

namespace Vexit.CliEngine.DependencyInjection;

/// <summary>
/// Caches service providers for each service registry type.
/// This ensures that complex service graphs are only built once per command group.
/// </summary>
public static class ServiceProviderCache
{
    private static readonly ConcurrentDictionary<Type, IServiceProvider> _providers = new();
    private static readonly ConcurrentDictionary<string, IServiceProvider> _providersMany = new();

    /// <summary>
    /// Gets or creates a service provider for the specified registry type. <br/>
    /// The provider is cached for performance - subsequent calls return the same instance.
    /// </summary>
    /// <param name="registryType">The type of the service registry (must implement IServiceRegistry).</param>
    /// <returns>A configured service provider with all services registered by the registry.</returns>
    public static IServiceProvider GetOrCreate(Type registryType)
    {
        return _providers.GetOrAdd(registryType, static t =>
        {
            var registry = (IServiceRegistry)Activator.CreateInstance(t)!;
            var services = new ServiceCollection();
            registry.RegisterServices(services);
            return services.BuildServiceProvider();
        });
    }

    /// <summary>
    /// Gets or creates a service provider for a set of registry types. <br/>
    /// The provider is cached based on the ordered list of registry type names.
    /// </summary>
    public static IServiceProvider GetOrCreateMany(Type[] orderedRegistryTypes)
    {
        var key = $"ORDERED|{string.Join("+", orderedRegistryTypes.Select(t => t.FullName))}";
        return _providersMany.GetOrAdd(key, _ =>
        {
            var services = new ServiceCollection();
            foreach (var t in orderedRegistryTypes)
            {
                var registry = (IServiceRegistry)Activator.CreateInstance(t)!;
                registry.RegisterServices(services);
            }
            return services.BuildServiceProvider();
        });
    }
}
