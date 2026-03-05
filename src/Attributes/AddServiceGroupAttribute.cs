/************************************************
 *
 * Copyright:		© 2025 VEXIT ®, www.vexit.com, Tomorrow is today...
 * Author:      	Vex Tatarevic
 * Date Created:    2025-11-03 - Initial creation for selective DI support
 * DateUpdated:		2026-02-05 - Added Order field so that service groups can register services in their given order
 *
 ************************************************/

using Vexit.CliEngine.DependencyInjection;

namespace Vexit.CliEngine.Attributes;

/// <summary>
/// Marks a command as belonging to a specific service group. <br/>
/// Commands can opt into shared service providers when caching is enabled.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class AddServiceGroupAttribute<T> : Attribute where T : IServiceRegistry
{
	/// <summary>
	/// The type of the service registry that provides services for this command group.
	/// The registry type must implement IServiceRegistry.
	/// </summary>
	public Type ServiceRegistryType { get; }

	/// <summary>
	/// The order in which this service group should be registered relative to other groups. <br/>
	/// Lower values are registered first. Default is 0.
	/// </summary>
	public int Order { get; set; } = 0;

	/// <summary>
	/// Creates a service group attribute for the specified registry type.
	/// </summary>
	public AddServiceGroupAttribute()
	{
		ServiceRegistryType = typeof(T);
	}
}
