/************************************************
 *
 * Copyright:		© 2025 VEXIT ®, www.vexit.com, Tomorrow is today...
 * Author:      	Vex Tatarevic
 * Date Created:    2025-11-06 - Initial creation for convention-based DI support
 * Date Updated:	2025-11-07	| Vex | Register concrete service types alongside interfaces for flexible injection
 *                  2025-11-30	| Vex | Slice-local convention registration under the command namespace prefix
 *                  2026-04-27	| Vex | Convention root is `._Services` only (avoids colliding with a hypothetical `services` command segment).
 ************************************************/

using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Vexit.CliEngine.Attributes;

namespace Vexit.CliEngine.DependencyInjection;

/// <summary>
/// Convention-based service registry that automatically discovers and registers services <br />
/// under <c>{commandNamespacePrefix}._Services</c> (and nested namespaces beneath it).
/// </summary>
public class ConventionBasedServiceRegistry : IServiceRegistry
{
    private readonly string _namespacePrefix;
    private readonly Assembly _assembly;

    /// <summary>
    /// Creates a registry that scans the specified namespace prefix for services.
    /// </summary>
    /// <param name="namespacePrefix">The namespace prefix to scan (e.g., "Vexit.VxCli.Commands.Init").</param>
    /// <param name="assembly">The assembly to scan for types.</param>
    public ConventionBasedServiceRegistry(string namespacePrefix, Assembly assembly)
    {
        _namespacePrefix = namespacePrefix;
        _assembly = assembly;
    }

    /// <summary>
    /// Registers all discovered services within the <c>._Services</c> subtree. <br />
    /// Services are registered as transient by default.
    /// </summary>
    /// <param name="services">The service collection to register services with.</param>
    public void RegisterServices(IServiceCollection services)
    {
        var servicesNamespaceRoot = $"{_namespacePrefix}._Services";

        var serviceTypes = _assembly.GetTypes()
            .Where(t => t.Namespace != null &&
                       (t.Namespace.Equals(servicesNamespaceRoot, StringComparison.Ordinal) ||
                        t.Namespace.StartsWith($"{servicesNamespaceRoot}.", StringComparison.Ordinal)) &&
                       !t.IsInterface &&
                       !t.IsAbstract &&
                       !typeof(Attribute).IsAssignableFrom(t) &&
                       t.GetCustomAttribute<CommandAttribute>() == null)
            .GroupBy(t => t.FullName!, StringComparer.Ordinal)
            .Select(g => g.First())
            .ToList();

        foreach (var serviceType in serviceTypes)
        {
            var interfaces = serviceType.GetInterfaces()
                .Where(i => i != typeof(IServiceRegistry))
                .ToList();

            services.TryAddTransient(serviceType);

            if (interfaces.Any())
            {
                foreach (var @interface in interfaces)
                {
                    services.TryAddTransient(@interface, provider => provider.GetRequiredService(serviceType));
                }
            }
        }
    }
}
