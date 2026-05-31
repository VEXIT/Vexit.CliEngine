
/************************************************
 *
 * Copyright:		© 2025 VEXIT ®, www.vexit.com, Tomorrow is today...
 * Author:      	Vex Tatarevic
 * Date Created:    2025-10-26 - Initial creation for Cli Engine extensions
 * Date Updated:	2025-11-11 | Vex | Added ICommandExecutor service registration for interactive commands
 *                  2025-11-26 | Vex | Added AddHook extension method for lifecycle hooks
 *                  2026-01-30 | Vex | Added CliConfig to the CliEngineOptions and CliService registration
 *                  2026-02-02 | Vex | Added built-in CliEngineServices registry for automatic core service inclusion
 ************************************************/

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using Vexit.Common.Models;
using Vexit.CliEngine.BaseClasses;
using Vexit.CliEngine.DependencyInjection;

namespace Vexit.CliEngine;

/// <summary>
/// Extension class for the Cli Engine. <br/>
/// Provides extension methods for the IHostApplicationBuilder and IHost.
/// </summary>
public static class CliEngineExtensions
{
    /// <summary>
    /// Adds Vexit Cli Engine services to a service collection (for CLI registries).
    /// </summary>
    public static IServiceCollection AddVexitCliEngine(
        this IServiceCollection services,
        Action<CliEngineOptions>? configure = null)
    {
        var options = new CliEngineOptions();
        configure?.Invoke(options);

        if (options.IncludeEntryAssembly && Assembly.GetEntryAssembly() != null)
        {
            options.AssembliesToScan.Add(Assembly.GetEntryAssembly()!);
        }

        RegisterCliEngineServices(services, options);
        return services;
    }

    /// <summary>
    /// Adds Vexit Cli Engine framework with optional configuration.
    /// Defaults: Scans entry assembly for commands, uses app services for DI.
    /// </summary>
    public static IHostApplicationBuilder AddVexitCliEngine(
        this IHostApplicationBuilder builder,
        Action<CliEngineOptions>? configure = null)
    {
        var options = new CliEngineOptions();
        configure?.Invoke(options);

        if (options.IncludeEntryAssembly && Assembly.GetEntryAssembly() != null)
        {
            options.AssembliesToScan.Add(Assembly.GetEntryAssembly()!);
        }

        RegisterCliEngineServices(builder.Services, options);

        return builder;
    }

    private static void RegisterCliEngineServices(IServiceCollection services, CliEngineOptions options)
    {
        // Register options for later initialization at runtime (when IServiceProvider is available)
        services.AddSingleton(options);
        services.AddSingleton(options.CliConfig);

        // Register the command executor for interactive command scenarios
        services.AddSingleton<ICommandExecutor, CommandExecutor>();

        // Register CLI service for consistent output across all components
        services.AddScoped<ICliService, CliService>();
    }

    /// <summary>
    /// Executes Cli Engine commands using the configured framework.
    /// </summary>
    public static async Task<Result> UseCliEngine(this IHost app, string[] args)
    {
        // Initialize using runtime service provider and configured options
        var options = app.Services.GetService<CliEngineOptions>() ?? new CliEngineOptions();
        if (options.IncludeEntryAssembly && options.AssembliesToScan.Count == 0 && Assembly.GetEntryAssembly() != null)
        {
            options.AssembliesToScan.Add(Assembly.GetEntryAssembly()!);
        }
        CommandController.Initialize(app.Services, options);
        return await CommandController.Execute(args);
    }

    /// <summary>
    /// Registers a lifecycle hook that executes at specific points in the CLI lifecycle.
    /// Hooks are resolved from DI and executed automatically by the framework.
    /// </summary>
    /// <typeparam name="THook">The hook type to register (must implement IHook)</typeparam>
    public static IHostApplicationBuilder AddHook<THook>(this IHostApplicationBuilder builder)
        where THook : class, IHook
    {
        builder.Services.AddTransient<IHook, THook>();
        return builder;
    }

    // Options class (public so consumers can configure it)
    public sealed class CliEngineOptions
    {
        public IList<Assembly> AssembliesToScan { get; } = new List<Assembly>();
        public bool IncludeEntryAssembly { get; set; } = true;
        public string CliName { get; set; } = "CLI Application";

        /// <summary>
        /// Configuration for CLI output styling and behavior.
        /// </summary>
        public CliConfig CliConfig { get; set; } = new CliConfig();
    }

    /// <summary>
    /// Built-in service registry containing core CliEngine services.
    /// This is automatically included for all commands.
    /// </summary>
    public sealed class CliEngineServices : IServiceRegistry
    {
        private readonly CliEngineOptions _options;

        public CliEngineServices(CliEngineOptions options)
        {
            _options = options;
        }

        public void RegisterServices(IServiceCollection services)
        {
            // Register the command executor for interactive command scenarios
            services.AddSingleton<ICommandExecutor, CommandExecutor>();

            services.AddSingleton(_options.CliConfig);
            services.AddScoped<ICliService, CliService>();
        }
    }
}
