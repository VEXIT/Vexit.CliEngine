/************************************************
 *
 * Copyright:		© 2025 VEXIT ®, www.vexit.com, Tomorrow is today...
 * Author:		Vex Tatarevic
 * Date Created:	2025-11-07 - Convention provider caches for modular commands
 * DateUpdated:		2026-02-05 | Vex | Updated GetOrCreateCombinedMany to use ORDERED prefix in cache keys for explicit ordering
 *
 ************************************************/

using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Vexit.CliEngine.DependencyInjection;

/// <summary>
/// Caches service providers for convention-based DI scopes. <br />
/// Avoids rebuilding service graphs for the same command slice and/or registry.
/// </summary>
public static class ConventionServiceProviderCache
{
	private static readonly ConcurrentDictionary<string, IServiceProvider> _namespaceProviders = new();
	private static readonly ConcurrentDictionary<string, IServiceProvider> _combinedProviders = new();

	/// <summary>
	/// Gets or creates a provider for a namespace slice using convention-based registration only. <br />
	/// Keyed by Assembly + NamespacePrefix.
	/// </summary>
	public static IServiceProvider GetOrCreateForNamespace(Assembly assembly, string namespacePrefix)
	{
		var key = $"NS|{assembly.FullName}|{namespacePrefix}";
		return _namespaceProviders.GetOrAdd(key, _ =>
		{
			var services = new ServiceCollection();
			new ConventionBasedServiceRegistry(namespacePrefix, assembly).RegisterServices(services);
			return services.BuildServiceProvider();
		});
	}

	/// <summary>
	/// Gets or creates a provider combining a service registry with a namespace slice. <br />
	/// Keyed by RegistryType + Assembly + NamespacePrefix.
	/// </summary>
	public static IServiceProvider GetOrCreateCombined(Type registryType, Assembly assembly, string namespacePrefix)
	{
		var key = $"COMBO|{registryType.FullName}|{assembly.FullName}|{namespacePrefix}";
		return _combinedProviders.GetOrAdd(key, _ =>
		{
			var registry = (IServiceRegistry)Activator.CreateInstance(registryType)!;
			var services = new ServiceCollection();
			registry.RegisterServices(services);
			new ConventionBasedServiceRegistry(namespacePrefix, assembly).RegisterServices(services);
			return services.BuildServiceProvider();
		});
	}

	/// <summary>
	/// Gets or creates a provider combining multiple service registries with a namespace slice. <br />
	/// Keyed by RegistryTypes (ordered) + Assembly + NamespacePrefix.
	/// </summary>
	public static IServiceProvider GetOrCreateCombinedMany(Type[] orderedRegistryTypes, Assembly assembly, string namespacePrefix)
	{
		var registriesKey = $"ORDERED|{string.Join("+", orderedRegistryTypes.Select(t => t.FullName))}";
		var key = $"COMBO_MANY|{registriesKey}|{assembly.FullName}|{namespacePrefix}";
		return _combinedProviders.GetOrAdd(key, _ =>
		{
			var services = new ServiceCollection();
			foreach (var rt in orderedRegistryTypes)
			{
				var reg = (IServiceRegistry)Activator.CreateInstance(rt)!;
				reg.RegisterServices(services);
			}
			new ConventionBasedServiceRegistry(namespacePrefix, assembly).RegisterServices(services);
			return services.BuildServiceProvider();
		});
	}
}
