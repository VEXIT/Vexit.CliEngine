/************************************************
 *
 * Copyright:		© 2025 VEXIT ®, www.vexit.com, Tomorrow is today...
 * Author:      	Vex Tatarevic
 * Date Created:    2025-09-15 - Initial creation for Command Controller
 * DateUpdated:		2025-10-27	| Vex | Register command ShortName; integrate new parser
 *                  2025-10-28	| Vex | Added hierarchical command support with CommandRegistry
 *                  2025-11-01	| Vex | Integrated CliUtil for consistent console output. Integrated FailureCodes for consistent error handling
 *                  2025-11-03	| Vex | Integrated selective DI support with CommandGroupAttribute and ServiceProviderCache
 *                  2025-11-07	| Vex | Added convention-based DI for vertical slicing with combined ServiceGroup support
 *                  2025-11-08	| Vex | Added per-command startup (CmdStartupBase) with precedence: ServiceGroups → Startup override → Slice conventions
 *                  2025-11-11	| Vex | Added StartCmd default execution when no arguments provided
 *                  2025-11-11	| Vex | Added ICommandExecutor support for interactive commands
 *                  2025-11-30	| Vex | Edited to allow combining DI via attributes (ServiceRegistry pattern) with DI via convention based (inside vertical slice Services folder)
 *                  2026-02-02	| Vex | Added built-in CliEngineServices registry for automatic core service inclusion in all command DI containers
 *                  2026-02-05	| Vex | Reordered command instantiation flow: build command-scoped provider with ordered service groups BEFORE command instantiation for proper constructor injection
 *
 ************************************************/

using Microsoft.Extensions.DependencyInjection;
using Vexit.CliEngine.Models;
using Vexit.Common.Models;
using Vexit.CliEngine.Constants;
using Vexit.CliEngine.BaseClasses;
using Vexit.CliEngine.Attributes;
using Vexit.CliEngine.DependencyInjection;
using System.Reflection;
using System.IO;
using static Vexit.CliEngine.CliEngineExtensions;

namespace Vexit.CliEngine;

/// <summary>
/// Controller for the CLI application.
/// </summary>
public class CommandController
{
    private static IServiceProvider? _serviceProvider;
    private static CliEngineExtensions.CliEngineOptions? _options;
    private static CommandRegistry? _registry;
    private static HelpGenerator? _helpGenerator;

    internal static void Initialize(IServiceProvider? serviceProvider, CliEngineExtensions.CliEngineOptions options)
    {
        _serviceProvider = serviceProvider;
        _options = options;

        // Initialize hierarchical command registry
        _registry = new CommandRegistry();
        _registry.Discover(options.AssembliesToScan);

        // Initialize help generator
        _helpGenerator = new HelpGenerator(_registry, options.CliName);
    }

    public static async Task<Result> Execute(string[] args)
    {
        if (_registry == null || _helpGenerator == null)
        {
            throw new InvalidOperationException(T.CommandController_not_initialized);
        }

        // Execute startup hooks once per process (before command resolution)
        var startupContext = new CommandContext(args, Directory.GetCurrentDirectory());
        await ExecuteStartupHooks(startupContext);

        // Handle empty args - check for start command, otherwise show root help
        if (args.Length == 0)
        {
            // Check if a "start" command exists and execute it as the default
            var startCommand = _registry.Root.Children.Values
                .FirstOrDefault(c => c.Name == "start" || c.AllNames.Contains("start"));

            if (startCommand != null)
            {
                // Execute the start command
                return await ExecuteCommand(startCommand, Array.Empty<string>());
            }
            else
            {
                // Fall back to showing help
                Cli.WriteLn(_helpGenerator.Generate(_registry.Root));
                return Result.Ok();
            }
        }

        // Check for help flag
        if (args.Any(a => a == "--help" || a == "-h"))
        {
            // Resolve command path without the help flag
            var argsWithoutHelp = args.Where(a => a != "--help" && a != "-h").ToArray();
            var (node, _, _) = _registry.Resolve(argsWithoutHelp);

            if (node == null)
            {
                Cli.WriteLn(_helpGenerator.Generate(_registry.Root));
            }
            else
            {
                Cli.WriteLn(_helpGenerator.Generate(node));
            }
            return Result.Ok();
        }

        // Resolve command path
        var (targetNode, consumedPath, remainingArgs) = _registry.Resolve(args);

        // If no command found, show root help
        if (targetNode == null)
        {
            var unknownCommandMessage = $"{T.Unknown_command}: {args[0]}";
            Cli.WriteLnError(unknownCommandMessage);
            Cli.WriteLn();
            Cli.WriteLn(_helpGenerator.Generate(_registry.Root));
            return Result.Failure(unknownCommandMessage, FC.UNKNOWN_COMMAND);
        }

        // If it's a group (not a leaf), show group help
        if (!targetNode.IsLeaf)
        {
            Cli.WriteLn(_helpGenerator.Generate(targetNode));
            return Result.Ok();
        }

        // Execute leaf command
        var result = await ExecuteCommand(targetNode, remainingArgs);

        return result;
    }

    private static async Task<Result> ExecuteCommand(CommandNode node, string[] args)
    {
        if (node.CommandType == null)
        {
            var commandNotFoundMessage = $"{T.Command_type_not_found}: {node.FullPath}";
            Cli.WriteLnError(commandNotFoundMessage);
            return Result.Failure(commandNotFoundMessage, FC.COMMAND_NOT_FOUND);
        }

        // Create command instance with tiered DI approach
        var instance = CreateCommandInstance(node.CommandType, args);

        // Parse arguments
        var parser = new CliParser();
        var parsedArgs = parser.Parse(instance, args);

        // Check for validation errors
        if (parsedArgs.HasValidationErrors)
        {
            foreach (var error in parsedArgs.ValidationErrors)
            {
                Cli.WriteLnError(error);
            }
            // Don't include message since we already printed above - avoid duplication in centralized printing
            return Result.Failure(null, FailureCodes.VALIDATION_ERROR);
        }

        // Apply parsed arguments to command properties
        parsedArgs.ApplyToCommand(instance);

        // Execute before hooks (blocking and non-blocking)
        var hookContext = new CommandContext(args, Directory.GetCurrentDirectory());
        var beforeResult = await ExecuteBeforeHooks(instance, hookContext);
        if (beforeResult.IsFailure)
        {
            return beforeResult; // Blocking hook aborted execution
        }

        // Execute the command
        // Arguments are already bound to command properties via [Option] and [Argument] attributes
        var commandResult = await instance.ExecuteAsync();

        // Execute after hooks (non-blocking, fire-and-forget)
        _ = ExecuteAfterHooks(instance, commandResult);

        return commandResult;
    }

    /// <summary>
    /// Creates a command instance using the appropriate DI tier with correct precedence order:
    /// 1. Service Groups (highest precedence)
    /// 2. Command Startup (context-aware manual wiring)
    /// 3. Command Folder Services (convention-based automation)
    /// 4. Global Services (fallback)
    /// </summary>
    /// <param name="commandType">The type of command to create.</param>
    /// <returns>A configured command instance.</returns>
    private static CmdBase CreateCommandInstance(Type commandType, string[] args)
    {
        // Try convention-based namespace prefix (vertical slice)
        var namespacePrefix = GetCommandNamespacePrefix(commandType);
        // Console.WriteLine($"[DEBUG CommandController] Command: {commandType.FullName}, Namespace prefix: {namespacePrefix ?? "NULL"}");

        // Service Groups: shared services explicitly opted in (always apply if present)
        var groupAttrs = commandType.GetCustomAttributes()
            .Where(attr => attr.GetType().IsGenericType &&
                           attr.GetType().GetGenericTypeDefinition() == typeof(AddServiceGroupAttribute<>))
            .Cast<dynamic>()
            .OrderBy(ga => ga.Order) // .ThenBy(ga => ga.ServiceRegistryType.FullName) // Stable tiebreaker for deterministic ordering
            .ToList();

        // Startup discovery: convention-only {SliceName}Startup within the slice namespace
        var startupType = DiscoverStartupType(commandType, namespacePrefix);

        // Build service collection with correct precedence order
        var services = new ServiceCollection();

        // Always include core CliEngine services first
        if (_options != null)
        {
            var engineServices = new CliEngineServices(_options);
            engineServices.RegisterServices(services);
        }

        // 1. Service Groups (highest precedence - explicit shared services)
        foreach (var ga in groupAttrs)
        {
            var registry = (IServiceRegistry)Activator.CreateInstance(ga.ServiceRegistryType)!;
            registry.RegisterServices(services);
        }

        // 2. Command Startup (context-aware manual wiring - runs if startup exists)
        if (startupType != null)
        {
            var context = new CommandContext(args, Directory.GetCurrentDirectory());
            var startup = (CmdStartupBase)Activator.CreateInstance(startupType)!;
            startup.Program_AddServices(services, context);
        }

        // 3. Command Folder Services (convention-based automation - lowest precedence in command scope)
        if (!string.IsNullOrEmpty(namespacePrefix))
        {
            new ConventionBasedServiceRegistry(namespacePrefix!, commandType.Assembly).RegisterServices(services);
        }

        // Build the provider and create instance
        var commandProvider = services.BuildServiceProvider();

        // DEBUG: Check if ServerSetupService is in command provider
        var testType = Type.GetType("Vexit.VxServerCli.Commands.Server.Setup.Services.ServerSetupService, Vexit.VxServerCli");
        // Console.WriteLine($"[DEBUG CommandController] Type resolved: {testType != null}");
        if (testType != null)
        {
            var testResolve = commandProvider.GetService(testType);
            // Console.WriteLine($"[DEBUG CommandController] ServerSetupService in command provider: {testResolve != null}");
        }

        // Use the command-scoped provider for instantiation
        return (CmdBase)ActivatorUtilities.CreateInstance(commandProvider, commandType)!;
    }


    /// <summary>
    /// Extracts the namespace prefix for convention-based DI if the command is in a Commands sub-namespace. <br />
    /// For nested command structures, finds the deepest namespace containing command classes.
    /// </summary>
    /// <param name="commandType">The command type.</param>
    /// <returns>The namespace prefix (e.g., "Vexit.VxCli.Commands.New.ApiServer") or null if not applicable.</returns>
    private static string? GetCommandNamespacePrefix(Type commandType)
    {
        var fullNamespace = commandType.Namespace ?? "";
        const string commandsSegment = ".Commands.";

        var commandsIndex = fullNamespace.IndexOf(commandsSegment, StringComparison.OrdinalIgnoreCase);
        if (commandsIndex < 0)
        {
            // If it ends with .Commands, treat as root and disable slice scanning
            if (fullNamespace.EndsWith(".Commands", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }
            return null;
        }

        // Extract after "Commands." to get the command path
        var afterCommands = fullNamespace.Substring(commandsIndex + commandsSegment.Length);
        var segments = afterCommands.Split('.', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length == 0)
            return null;

        // For command classes, the namespace is already the slice namespace.
        // Example: Vexit.VxCli.Commands.New.ApiServer (namespace) + ApiServerCmd (class)
        // The slice namespace is: Vexit.VxCli.Commands.New.ApiServer
        return fullNamespace;
    }

    /// <summary>
    /// Attempts to discover a convention-named command startup type within the slice. <br />
    /// Naming convention: {SliceNamespace}.{SliceName}Startup (e.g., Commands.Init.InitStartup)
    /// </summary>
    private static Type? DiscoverStartupType(Type commandType, string? namespacePrefix)
    {
        if (string.IsNullOrEmpty(namespacePrefix))
            return null;

        try
        {
            // SliceName is the last segment of the prefix
            var lastDot = namespacePrefix!.LastIndexOf('.');
            var sliceName = lastDot >= 0 ? namespacePrefix.Substring(lastDot + 1) : namespacePrefix;
            var startupFullName = $"{namespacePrefix}.{sliceName}Startup";
            var t = commandType.Assembly.GetType(startupFullName, throwOnError: false, ignoreCase: false);

            if (t == null)
                return null;
            if (!typeof(CmdStartupBase).IsAssignableFrom(t))
                return null;
            if (!t.IsPublic || t.IsAbstract)
                return null;
            return t;
        }
        catch
        {
            return null;
        }
    }


    /// <summary>
    /// Executes all registered startup hooks once per process.
    /// Startup hooks are non-blocking and run in parallel.
    /// </summary>
    private static async Task ExecuteStartupHooks(CommandContext context)
    {
        if (_serviceProvider == null) return;

        try
        {
            var hooks = _serviceProvider.GetServices<IHook>()
                .OrderBy(h => h.Order)
                .ToArray();

            if (!hooks.Any()) return;

            // Execute all startup hooks in parallel (fire-and-forget)
            var startupTasks = hooks.Select(hook => ExecuteHookSafely(
                () => hook.OnStartup(context),
                hook,
                Hooks.OnStartup
            ));

            await Task.WhenAll(startupTasks);
        }
        catch
        {
            // Silently fail - startup hooks shouldn't break CLI startup
        }
    }

    /// <summary>
    /// Executes all registered before hooks.
    /// Blocking hooks are executed first (in order), then non-blocking hooks run in parallel.
    /// Returns failure if any blocking hook fails.
    /// </summary>
    private static async Task<Result> ExecuteBeforeHooks(CmdBase command, CommandContext context)
    {
        if (_serviceProvider == null) return Result.Ok();

        try
        {
            var hooks = _serviceProvider.GetServices<IHook>()
                .OrderBy(h => h.Order)
                .ToArray();

            if (!hooks.Any()) return Result.Ok();

            // Execute blocking hooks first (in order, waiting for each)
            foreach (var hook in hooks)
            {
                var result = await ExecuteHookSafelyBlocking(
                    () => hook.OnBeforeExecuteBlocking(command, context),
                    hook,
                    Hooks.OnBeforeExecuteBlocking
                );

                if (result.IsFailure)
                {
                    return result; // Blocking hook aborted execution
                }
            }

            // Execute non-blocking hooks in parallel (fire-and-forget)
            var nonBlockingTasks = hooks.Select(hook => ExecuteHookSafely(
                () => hook.OnBeforeExecute(command, context),
                hook,
                Hooks.OnBeforeExecute
            ));

            // Don't await - let them run in background while command executes
            _ = Task.WhenAll(nonBlockingTasks);

            return Result.Ok();
        }
        catch
        {
            // If hook execution fails unexpectedly, continue
            return Result.Ok(); // Don't abort command for hook failures
        }
    }

    /// <summary>
    /// Executes all registered after hooks (fire-and-forget).
    /// After hooks always run in parallel and never block command completion.
    /// </summary>
    private static async Task ExecuteAfterHooks(CmdBase command, Result result)
    {
        if (_serviceProvider == null) return;

        try
        {
            var hooks = _serviceProvider.GetServices<IHook>()
                .OrderBy(h => h.Order)
                .ToArray();

            if (!hooks.Any()) return;

            // Execute all after hooks in parallel (fire-and-forget)
            var afterTasks = hooks.Select(hook => ExecuteHookSafely(
                () => hook.OnAfterExecute(command, result),
                hook,
                Hooks.OnAfterExecute
            ));

            await Task.WhenAll(afterTasks);
        }
        catch
        {
            // Silently fail - after hooks shouldn't affect command result
        }
    }

    /// <summary>
    /// Executes a blocking hook method safely with error handling.
    /// Exceptions are converted to Result.Failure to abort command execution.
    /// </summary>
    private static async Task<Result> ExecuteHookSafelyBlocking(Func<Task<Result>> hookAction, IHook hook, string methodName)
    {
        try
        {
            return await hookAction();
        }
        catch (Exception ex)
        {
            var hookType = hook.GetType().Name;
            var message = string.Format(T.Hook_failed, $"{hookType}.{methodName}") + $": {ex.Message}";
            Cli.WriteLnWarning(message);
            var failureMessage = string.Format(T.Hook_failed, hookType) + $": {ex.Message}";
            return Result.Failure(failureMessage, FC.HOOK_EXECUTION_FAILED);
        }
    }

    /// <summary>
    /// Executes a non-blocking hook method safely (for void-returning hooks).
    /// Exceptions are logged as warnings but don't affect execution.
    /// </summary>
    private static async Task ExecuteHookSafely(Func<Task> hookAction, IHook hook, string methodName)
    {
        try
        {
            await hookAction();
        }
        catch (Exception ex)
        {
            var hookType = hook.GetType().Name;
            var message = string.Format(T.Hook_failed, $"{hookType}.{methodName}") + $": {ex.Message}";
            Cli.WriteLnWarning(message);
        }
    }
}