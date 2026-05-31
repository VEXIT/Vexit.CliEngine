# Architecture in Vexit.CliEngine

|              |                                                                                                                                  |
| ------------ | -------------------------------------------------------------------------------------------------------------------------------- |
| Copyright    | © VEXIT ® 2025 , www.vexit.com , Tomorrow is today...                                                                            |
| Author       | Vex Tatarevic                                                                                                                    |
| Date Created | 2025-11-03                                                                                                                       |
| Date Updated | 2025-11-09                                                                                                                       |
|              | 2025-11-28 - Vex - UPdated Services DI convention based strategy to include Command-Group folders besides single command folders |
|              | 2025-12-31 - Vex - Added CmdBase and CliBase architecture section explaining inheritance hierarchy and SRP separation |
|              | 2026-01-30 - Vex - Updated to CliService architecture, replaced CliBase inheritance with dependency injection |
|              | 2026-02-17 - Fixed DI precedence order: Service Groups → Command Startup → Command Folder Services → Global Services |
|              | 2026-04-21 - Replaced obsolete `Commands/Encrypt` examples with `Commands/Settings` and shared `ISettingsDiscoveryService` |

---

## Purpose
This document defines the architecture and conventions for organizing commands, services, operations (Ops), and modules in projects built on `Vexit.CliEngine`. The framework supports both traditional horizontal layering and modern vertical slicing by command/feature, enabling true plug-and-play modularity with convention-based dependency injection.

---

## Core Concepts

### 1) Command Hierarchy Groups (non-executable)
- Class inherits `CmdGroupBase`
- Represent folders in command hierarchy (e.g., `vmod new`)
- Provide shared options/aliases for their subtree
- Should not execute business logic
- Location: `Vexit.VxCli/Commands/<Group>/<SubGroup>/...`

### 2) Executable Commands
- Class inherits `CmdBase`
- Decorated with `[Command("name", "description")]`
- Contain business flow orchestration using Ops and/or Services
- Use constructor injection for services including `CliService`
- Support both flat and vertical slicing organization

#### CliService Architecture
For proper separation of concerns, Vexit.CliEngine uses dependency injection for CLI output:

- **`CliService`**: Injectable service that handles all CLI output formatting and margin management
- **`CliConfig`**: Configuration class for CLI colors and styling
- **`CmdBase`**: Manages command execution lifecycle, DI helpers, and result management

This separation follows the Single Responsibility Principle:
- **CliService** manages console output, colors, formatting, and global margins (injected)
- **CmdBase** manages command execution, argument binding, and business logic orchestration

**Key Architectural Benefit:** Commands become pure orchestrators, delegating actual work (including CLI output) to specialized services. This prevents command classes from becoming bloated "do everything" classes and enables proper separation where:
- **Commands**: Parse arguments and orchestrate workflows
- **Services**: Execute business logic with appropriate CLI feedback
- **CliService**: Provides consistent, configurable CLI output anywhere in the application

Components get consistent CLI output by injecting `CliService` instead of inheriting behavior, enabling services to provide rich user feedback without command classes needing to handle presentation logic.

### 3) CliService - Consistent CLI Output

Vexit.CliEngine provides `CliService` for centralized, configurable CLI output across all components:

- **Injection**: `CliService` is registered as a scoped service and injected into any component that needs CLI output
- **Configuration**: `CliConfig` is configured in `Program.cs` via `AddCliEngine()` options
- **Consistency**: All components use the same styling, colors, and margin settings
- **Testability**: CLI output can be mocked for unit testing
- **Flexibility**: Different components can have different CLI behavior by injecting different configurations

**Usage Pattern:**
```csharp
public class MyService
{
    private readonly CliService cli;

    public MyService(CliService cli) => this.cli = cli;

    public void DoWork() {
        cli.WriteLn("Starting...");
        cli.WriteLnSuccess("Done!");
    }
}
```

### 5) Vertical Slicing (Command Folders)
- Vertical slicing is implemented using command folders: commands are organized in command-specific folders with all related layers vertically integrated
- Structure: `Commands/<CommandName>/(Command + _Services + _Models + _Ops + _Constants + _Validators + _Clis)`
- Convention-based DI automatically discovers and registers services in `Commands.<CommandName>._Services.*` (interfaces and concretes) using TryAdd semantics
- Enables true modularity: drop a command folder, get auto-wired services
- Root-level commands (directly under `.Commands`) do not trigger command folder scanning

### 6) Service Groups (Shared Services)
- Decorated with `[AddServiceGroup<ServiceRegistryName>]`, for example: `[AddServiceGroup<ProjectAnalyzerServiceGroup>]`.
- Explicitly opt-in to shared, root-level services
- Share a cached `IServiceProvider` built from `IServiceRegistry`
- Can be combined with convention-based command folder services
- Purpose: load only the services needed by this functional area

### 7) Services (side-effects and integrations)
- External I/O, encryption, key vault, file system, HTTP, etc.
- Can be registered in two ways:
  - **Convention-based**: Place in `Commands/<CommandName>/_Services/` for auto-registration
  - **Explicit**: Register in `IServiceRegistry` and use `[AddServiceGroup<ServiceRegistryName>]` attribute
- Location (shared): `Vexit.VxCli/Services/<Domain>/...`
- Location (command-local): `Vexit.VxCli/Commands/<CommandName>/_Services/...`
- Typical lifetimes: Transient (default). Use Scoped only if per-command state is needed

### Service Group Organization Convention

Service groups are organized based on whether they include local project services:

#### **1. External-Only Service Groups**
When registering only external/third-party library services, place registries directly in `Services/`:

**Examples:**
```
Services/
├── WorkflowServiceGroup.cs     # Registers Vexit.FlowEngine services (external)
├── SshServiceGroup.cs          # Registers Vexit.SSH services (external)
├── LoggingServiceGroup.cs      # Registers Vexit.Logging services (external)
└── CliServiceGroup.cs          # Registers Vexit.CliEngine services (external)
```

#### **2. Local Service Groups**
When including local project services alongside external ones, create a group folder:

**Example:**
```
Services/
├── Security/                         # Group bundle with local services
│   ├── SecurityServiceGroup.cs       # IServiceRegistry for security domain
│   ├── ILocalAuthService.cs          # Local authentication service interface
│   ├── LocalAuthService.cs           # Local authentication implementation
│   ├── IAuditLogger.cs               # Local audit service interface
│   └── ExternalKeyVault.cs           # Local wrapper for external key vault
│
└── WorkflowServiceGroup.cs           # External-only (no group folder needed)
```

**Naming Convention:** Classes implementing `IServiceRegistry` use the "ServiceGroup" suffix (e.g., `SecurityServiceGroup`, `WorkflowServiceGroup`)

### 8) Ops (Operations)
- Stateless, deterministic, no DI, side-effect free when possible
- Single entry point per Op: `Execute(...)` and returns `Result` / `Result<T>`
- Conventions:
  - For 1–2 inputs and simple return → use direct parameters and `Result<T>`
  - For multiple fields → use file-scoped `record struct` DTOs in the same file
  - Avoid nested `Input`/`Output` if it reduces DX; prefer descriptive file-scoped DTO names
- Placement:
  - Shared across commands → `Vexit.VxCli/Ops/`
  - Command folder-specific → `Vexit.VxCli/Commands/<CommandName>/Ops/`

---

## Dependency Injection Architecture

### DI Resolution Flow (Precedence)

For each command execution, services are resolved in this order:

1. **Service Groups (Always first if present)**  
   Apply all `[AddServiceGroup<ServiceRegistryName>]` registries (shared services) to the service collection. Multiple attributes are allowed and are applied in order (use optional `Order` property for fine control). These registrations now *layer with* slice services—ServiceGroups run first so shared infrastructure is available before local services add their own behavior.

2. **Command Startup Override (Optional, per-command)**  
   If a `CmdStartupBase` exists for the command folder (convention: `<CommandName>Startup`), instantiate it and call:
   `Program_AddServices(IServiceCollection services, CommandContext context)`.
   Use runtime context (args, working dir) to explicitly wire concrete implementations (e.g., `DotNetInitService` vs `NextjsInitService`).

3. **Command Folder Conventions (Safety net, TryAdd)**  
   Convention-based registration for `Commands.<CommandName>._Services.*` using `TryAddTransient`. This always runs if the command has a slice namespace, regardless of ServiceGroup usage, so commands can consume both shared registries and slice-local helpers:
   - Register interfaces to concretes when found
   - Register concretes directly when no interface
   TryAdd ensures Startup overrides take precedence. Only a `<CommandName>Startup` can supersede the combination of ServiceGroups + convention services.

4. **Caching**  
   Providers are cached to minimize reflection overhead:
   - Shared registries cache by `RegistryType`
   - Command folder-only cache by `Assembly + NamespacePrefix`
   - Combined cache by `RegistryTypes (ordered) + Assembly + NamespacePrefix`

5. **No DI**  
   Root-level commands without `[AddServiceGroup]` and no command folder → `Activator.CreateInstance` with minimal overhead.

### Convention-Based Service Discovery

**Namespace Pattern:**
```
Commands.<CommandName>._Services.*
```

**Example:**
```
Vexit.VxCli.Commands.Init._Services.DotNetInitService
                      ^^^^          ^^^^^^^^^^^^^^^^^^
                      command folder auto-registered
```

**Rules:**
- Only types under `._Services.` subtree are registered
- Interfaces are registered with their implementations (e.g., `IInitService` → `DotNetInitService`)
- Concrete types without interfaces are registered directly
- Commands, attributes, and abstract types are excluded
- All services registered as Transient by default
- Uses `TryAddTransient` to respect explicit registrations made by `CmdStartupBase`

**Caching:**
- Providers cached by `Assembly + NamespacePrefix` for command folder-only
- Providers cached by `RegistryType + Assembly + NamespacePrefix` for combined
- Thread-safe via `ConcurrentDictionary`

### Command Startup
- Base: `CmdStartupBase`
- Hook: `Program_AddServices(IServiceCollection services, CommandContext context)`
- Context: `CommandContext` provides args and working directory
- Precedence: applied after ServiceGroups and before Command Folder Conventions (which use TryAdd)
- Purpose: context-aware selection of concrete services without bloating `Program.cs`

#### Inspiration from V-Mod Modules
- V-Mod introduced a module concept with a central registration hook to wire services/features.
- Command Startup borrows the idea of a focused composition hook, but scopes it to a single command folder for clarity and performance.
- Differences and rationale:
  - Scope:
    - V-Mod Module: global feature/module registration for an app.
    - Command Startup: per-command folder registration for a CLI.
  - Discovery:
    - V-Mod: attribute-based module discovery.
    - Command Startup: convention-based discovery (`<CommandName>Startup`) alongside the command’s command folder.
  - Precedence:
    - Command Startup runs after ServiceGroups (shared infra) and before command folder TryAdd registrations to allow explicit, context-aware overrides.
  - Ergonomics:
    - Keeps `Program.cs` clean; DI decisions live next to the command that needs them.
  - Performance:
    - Targeted construction: only the active command’s startup is loaded and executed.

Example flow:
1) Engine identifies the command type from args.
2) Applies `[AddServiceGroup<ServiceRegistryName>]` registries (shared services).
3) If `<CommandName>Startup` exists, calls `Program_AddServices(services, context)` for context-aware service registration (e.g., choose `DotNetInitService` vs `NextjsInitService` based on working dir or args).
4) Runs command folder service discovery with TryAdd to register convention-based services.
5) If a command doesn't use any of the above, it can still receive global services.

**Precedence Order**: Service Groups → Command Startup → Command Folder Services → Global Services

## Implementation Notes

### Nested Command Structures
For nested command hierarchies (e.g., `Commands.New.ApiServer.ApiServerCmd`), the convention-based service discovery scans the command slice folder (`Commands.New.ApiServer.Services`) rather than intermediate group folders.

### Startup Class Execution
Command startup classes are executed for **all commands** that have them, regardless of whether the command uses service groups. This ensures context-aware service registration works consistently across all command types.

---

## Service Injection: 4 Strategies (Precedence Order)

Services are resolved in this order (higher numbers can't override lower ones):

### 1. **Service Groups (attribute-based, shared)**
   - What: Bundles of shared services (encryption, env vars, file system) registered via `IServiceRegistry` classes.
   - How to set it up:
     - For external-only services: place registry directly in `Services/`. For group bundles with local services: create a group folder.
     - Create a registry class inside the folder and name it `<ServiceGroupName>ServiceGroup.cs` (e.g. `EncryptionServiceGroup.cs`).
     - In the registry class, implement the `IServiceRegistry`
     - In `RegisterServices(IServiceCollection services)` method, register external and/or local services. Place the registry in   `Services/` (external-only) or in a group folder (with local services).
    - In the command class, decorate class with one or more attributes: `[AddServiceGroup<ServiceRegistryName>]`. Multiple attributes are supported and applied in discovery order. Example: 
    ```csharp
    [AddServiceGroup<ProjectsRegistryServices>]
    [AddServiceGroup<ProjectAnalyzerServiceGroup>]
    public class ExampleCmd : CmdBase { ... }
    ```
   - Examples:
     ```csharp
     // External-only service group
     // In Services/WorkflowServiceGroup.cs
     public sealed class WorkflowServiceGroup : IServiceRegistry
     {
         public void RegisterServices(IServiceCollection services)
         {
             services.AddVexitFlowEngine(options => { /* config */ });
         }
     }

     // Local service group (with local services)
     // In Services/Security/SecurityServiceGroup.cs
     public sealed class SecurityServiceGroup : IServiceRegistry
     {
         public void RegisterServices(IServiceCollection services)
         {
             services.AddTransient<ILocalAuthService, LocalAuthService>();
             // + external services...
         }
     }

    // In Commands/Process/ProcessCmd.cs
    [AddServiceGroup<WorkflowServiceGroup>]        // External FlowEngine services
    [AddServiceGroup<SecurityServiceGroup>]        // Feature bundle with local services
    public class ProcessCmd : CmdBase { ... }
    ```

### 2. **Command Startup (manual, context-aware)**
   - What: Manual service wiring in a `<CommandName>Startup.cs` file inside your command's command folder.
   - How to set it up:
     - Create `<CommandName>Startup.cs` (e.g., `ApiServerStartup.cs` in `Commands/New/ApiServer/`).
     - Inherit from `CmdStartupBase`.
     - Override `Program_AddServices(IServiceCollection services, CommandContext context)`.
     - Use `context` (args, working dir) to choose concretes manually.
   - When used: Runs before convention-based services, allowing explicit override of automatic registrations.
   - Example:
     ```csharp
     // Commands/New/ApiServer/ApiServerStartup.cs
     public class ApiServerStartup : CmdStartupBase
     {
         public override void Program_AddServices(IServiceCollection services, CommandContext context)
         {
             // Register services needed for API server creation
             services.AddTransient<IProjectAnalyzerService, ProjectAnalyzerService>();
             services.AddTransient<InitVModArchFlow>();  // Explicit registration
             services.AddTransient<IInitVXInfraService, InitVXInfraService>();
         }
     }
     ```

### 3. **Command Folder Services (convention-based, command-local)**
   - What: Auto-register all services dropped in the `Services/` folder inside your command's command folder.
   - How it works:
     - If a class implements an interface (e.g., `ICreateCliAppService`), registers `services.AddTransient<ICreateCliAppService, CreateCliAppService>()`.
     - If no interface, registers the concrete type.
     - Uses `TryAddTransient()` so startup registrations take precedence.
     - No attributes needed; engine scans and registers by convention.

   **Example A: Single Command Folder**
   - Command folder is named after the command (minus "Cmd" suffix), e.g., `Commands/CreateCliApp/` for `CreateCliAppCmd`.
   - Drop your services (interfaces + implementations) in `Commands/CreateCliApp/_Services/`.
   - Example:
     ```csharp
     // Commands/CreateCliApp/_Services/ICreateCliAppService.cs
     public interface ICreateCliAppService
     {
         Result<string> CreateProject(string name, string template);
     }

     // Commands/CreateCliApp/_Services/CreateCliAppService.cs
     public class CreateCliAppService : ICreateCliAppService
     {
         public Result<string> CreateProject(string name, string template) { ... }
     }

     // Commands/CreateCliApp/CreateCliAppCmd.cs (no attribute needed)
     public class CreateCliAppCmd : CmdBase
     {
         private readonly ICreateCliAppService _service;  // Auto-injected from Services/
         public CreateCliAppCmd(ICreateCliAppService service) { _service = service; }
     }
     ```

   **Example B: Command Slice Folder**
   - For nested command structures, services are scanned from the command slice folder.
   - Example: `Commands/New/ApiServer/_Services/` for `ApiServerCmd`.
   - Services in slice folders are specific to that command only.
   - Example:
     ```csharp
     // Commands/New/ApiServer/_Services/InitVModArchFlow.cs
     public class InitVModArchFlow : FlowBase<InitVModInput, InitVModOutput>
     {
         // Implementation for API server VMod architecture setup
     }

     // Commands/New/ApiServer/ApiServerCmd.cs (no attribute needed)
     public class ApiServerCmd : NewCmdGroup
     {
         private readonly InitVModArchFlow _flow;  // Auto-injected from Services/
         public ApiServerCmd(InitVModArchFlow flow, IInitVXInfraService infra) { ... }
     }
     ```

   **Example C: Command Group Folder (Shared Services)**
   - Command group folder contains multiple related commands, e.g., `Commands/Settings/` with `EncryptCmd`, `DecryptCmd`, `RestoreCmd`, `SecretsCmd`.
   - Drop shared services in `Commands/Settings/_Services/` - they will be auto-discovered for ALL commands in that group (e.g., `ISettingsSecurityService`, `ISettingsDiscoveryService`).
   - Example:
     ```csharp
     // Commands/Settings/_Services/ISettingsSecurityService.cs
     public interface ISettingsSecurityService
     {
         Result<List<string>> EncryptSecrets(...);
         Result<DecryptResult> DecryptSecrets(...);
     }

     // Commands/Settings/_Services/ISettingsDiscoveryService.cs
     public interface ISettingsDiscoveryService
     {
         IReadOnlyList<string> DiscoverSettingsFiles(string projectPath);
     }

     // Commands/Settings/_Services/SettingsSecurityService.cs
     public class SettingsSecurityService : ISettingsSecurityService { ... }

     // Commands/Settings/_Services/SettingsDiscoveryService.cs
     public class SettingsDiscoveryService : ISettingsDiscoveryService { ... }

     // Commands/Settings/EncryptCmd.cs (no attribute needed for slice services)
     public class EncryptCmd : SettingsCmdGroup
     {
         private readonly ISettingsSecurityService _security;
         private readonly ISettingsDiscoveryService _discovery;
         public EncryptCmd(ISettingsSecurityService security, ISettingsDiscoveryService discovery)
         {
             _security = security;
             _discovery = discovery;
         }
     }

     // Commands/Settings/DecryptCmd.cs — same shared Services/ folder
     public class DecryptCmd : SettingsCmdGroup { ... }
     ```

### 4. **Global Services (fallback for simple commands)**
   - What: Allows commands that don't use any of the above mechanisms to receive globally registered services.
   - How: The engine falls back to the main application's service provider.
   - Use Case: Perfect for simple, root-level commands (like `StartCmd`) that need access to a shared service (like `ICommandExecutor`) without needing a full vertical slice or service group. See `Example 4` under "Command Implementation Examples" for a code sample.

---

## Organization Patterns

### Pattern 1: Horizontal Layering (Traditional)

```
Vexit.VxCli/
├── Commands/
│   ├── InitCmd.cs
│   └── Settings/
│       ├── EncryptCmd.cs
│       └── Services/
│           └── ...
├── Services/
│   ├── Encryption/
│   │   ├── EncryptionServiceGroup.cs  ← IServiceRegistry implementation
│   │   ├── IKeyVaultService.cs
│   │   └── KeyVaultService.cs
│   └── ProjectManagement/
│       ├── ProjectServiceGroup.cs
│       └── ...
├── Ops/
│   ├── ResolveProjectPathOp.cs
│   └── DetectProjectTypeOp.cs
└── Models/
    └── ProjectContextModel.cs
```

**Use with:** `[AddServiceGroup<ProjectsRegistryServices>]` (or another `IServiceRegistry` under `Services/`)

### Pattern 2: Vertical Slicing (Command Folders)

```
Vexit.VxCli/
├── Commands/
│   ├── Init/
│   │   ├── InitCmd.cs                 ← Auto-wired with command folder services
│   │   ├── Services/
│   │   │   ├── IInitService.cs
│   │   │   └── DotNetInitService.cs   ← Auto-registered
│   │   ├── Models/
│   │   │   └── InitResultModel.cs
│   │   └── Constants/
│   │       └── Text.cs (partial)
│   └── Settings/
│       ├── EncryptCmd.cs
│       ├── DecryptCmd.cs
│       ├── RestoreCmd.cs
│       ├── SecretsCmd.cs
│       ├── SettingsCmdGroup.cs
│       └── Services/
│           ├── ISettingsDiscoveryService.cs
│           ├── SettingsDiscoveryService.cs
│           ├── ISettingsSecurityService.cs
│           └── SettingsSecurityService.cs
├── Services/                          ← Shared services (opt-in)
│   └── Common/
│       ├── CommonServiceGroup.cs      ← IServiceRegistry
│       └── IFileSystemService.cs
├── Clis/                              ← Shared CLI utilities
│   └── PromptAppIdCli.cs
├── Validators/                        ← Shared validators
│   └── AppIdValidator.cs
└── Models/                            ← Shared models
    └── ProjectContextModel.cs
```

**Use with:** No attribute needed (auto-DI), or `[AddServiceGroup<CommonServiceGroup>]` to add shared services

### Pattern 3: Hybrid (Recommended)

- Use vertical slicing for complex, self-contained commands (implemented as command folders, e.g., `Init`, `Settings`)
- Use horizontal layering for truly shared infrastructure (e.g., file system, logging)
- Combine both via `[AddServiceGroup<ServiceRegistryName>]` when a command folder needs shared services

---

## Service Registry Pattern

### Explicit Registration (Shared Services)

```csharp
// In Vexit.VxCli/Services/Encryption/EncryptionServiceGroup.cs
public sealed class EncryptionServiceGroup : IServiceRegistry
{
    public void RegisterServices(IServiceCollection services)
    {
        services.AddTransient<IKeyVaultService, KeyVaultService>();
        services.AddTransient<IEnvironmentKeyService, EnvironmentKeyService>();
    }
}
```

### Convention-Based Registration (Command Folder Services)

No registry needed—just place services in the command folder:

```csharp
// In Vexit.VxCli/Commands/Init/_Services/IInitService.cs
public interface IInitService
{
    Task<Result<InitResult>> ExecuteAsync(string projectPath);
}

// In Vexit.VxCli/Commands/Init/_Services/DotNetInitService.cs
public class DotNetInitService : IInitService
{
    // Auto-registered as IInitService → DotNetInitService
}
```

---

## Command Implementation Examples

### Example 1: Command Folder-Only (Convention-Based DI)

```csharp
// In Vexit.VxCli/Commands/Init/InitCmd.cs
[Command("init", T.InitCmd_description)]
public class InitCmd : CmdBase
{
    private readonly IInitService _initService;
    private readonly IProjectAnalyzerService _projectAnalyzer;

    // Services auto-injected from Commands.Init._Services.*
    public InitCmd(IInitService initService, IProjectAnalyzerService projectAnalyzer)
    {
        _initService = initService;
        _projectAnalyzer = projectAnalyzer;
    }

    public override async Task<Result> ExecuteAsync()
    {
        var context = await _projectAnalyzer.AnalyzeAsync(projectPath);
        return await _initService.ExecuteAsync(context);
    }
}
```

### Example 2: Combined (Shared + Command Folder)

```csharp
// In Vexit.VxCli/Commands/Settings/EncryptCmd.cs
[AddServiceGroup<ProjectsRegistryServices>]   // Shared registry (opt-in)
[Command(Cmd.Settings.Encrypt.Name, "...")]
public class EncryptCmd : SettingsCmdGroup
{
    private readonly IProjectsRegistryService _registry;       // From service group
    private readonly ISettingsSecurityService _security;    // From Commands/Settings/_Services (convention)
    private readonly ISettingsDiscoveryService _discovery;   // From Commands/Settings/_Services (convention)

    public EncryptCmd(
        IProjectsRegistryService registry,
        ISettingsSecurityService security,
        ISettingsDiscoveryService discovery)
    {
        _registry = registry;
        _security = security;
        _discovery = discovery;
    }
}
```

### Example 3: Shared-Only (Root Command)

```csharp
// In Vexit.VxCli/Commands/StatusCmd.cs
[AddServiceGroup<CommonServiceGroup>]
[Command("status", "Display tool status")]
public class StatusCmd : CmdBase
{
    private readonly IFileSystemService _fileSystem;

    public StatusCmd(IFileSystemService fileSystem)
    {
        _fileSystem = fileSystem;
    }
}
```

### Example 4: Global Service Injection (Root Command)

```csharp
// In Vexit.VxCli/Commands/StartCmd.cs
// No attributes needed
[Command("start", "The default start page")]
public class StartCmd : CmdBase
{
    private readonly ICommandExecutor _executor;

    // Injected from the global service provider
    public StartCmd(ICommandExecutor executor)
    {
        _executor = executor;
    }
}
```

### Example 5: No DI (Simple Command)

```csharp
// In Vexit.VxCli/Commands/VersionCmd.cs
[Command("version", "Display version information")]
public class VersionCmd : CmdBase
{
    public override async Task<Result> ExecuteAsync()
    {
        Cli.WriteLnInfo($"V-Mod CLI v{Assembly.GetExecutingAssembly().GetName().Version}");
        return Task.FromResult(Result.Ok());
    }
}
```

---

## Assembly Scanning & Discovery

- Engine discovers commands by `[Command]` attribute
- Convention-based DI scans the command’s assembly under the slice namespace for services
- `CmdStartupBase` is discovered by convention within the slice

---

## Placement Summary

- **Commands (flat):** `Commands/<CommandName>Cmd.cs`
- **Commands (vertical slice):** `Commands/<CommandGroupName>/<CommandName>Cmd.cs`
- **Command slice services:** `Commands/<CommandName>/_Services/...`
- **Service groups:** `Services/<ServiceGroupName>ServiceGroup.cs` (external-only) or `Services/<ServiceGroupName>/...` (with local services)
- **Ops:** `Ops/` (no DI, stateless) and `Commands/<CommandName>/Ops/` for slice-local
- **Clis:** `Clis/` (static user interaction utilities)
- **Validators:** `Validators/` (static validation logic)
- **Models (shared):** `Models/`
- **Models (feature-local):** `Commands/<CommandName>/_Models/`
- **Constants (shared):** `Constants/Text.cs`
- **Constants (feature-local):** `Commands/<CommandName>/_Constants/Text.cs` (partial class)

---

## Naming Guidance

- Use `*Cmd` suffix for executable command classes (inherit from `CmdBase`)
- Use `*CmdGroup` suffix for non-executable command group classes (inherit from `CmdGroupBase`)
- Use `*Op` suffix for Ops
- Use `*ServiceGroup` for classes implementing `IServiceRegistry` (e.g., `EncryptionServiceGroup`)
- Use `*Service` for service implementations (e.g., `KeyVaultService`)
- Use `I*Service` for service interfaces (e.g., `IKeyVaultService`)
- Use `*Cli` for CLI interaction utilities (e.g., `PromptAppIdCli`)
- Use `*Validator` for validation utilities (e.g., `AppIdValidator`)


## Proposed Improvements

### 1. Configurable Service Lifetimes
**Current State:** All convention-discovered services are registered as Transient.

**Proposal:** Support lifetime detection via:
- Attributes: `[Transient]`, `[Scoped]`, `[Singleton]`
- Naming convention: `*SingletonService`, `*ScopedService`
- Interface markers: `ITransientService`, `IScopedService`, `ISingletonService`

**Use Case:** Long-lived caches or per-command state management.

### 2. Explicit Opt-Out for Convention Scanning
**Current State:** No way to disable convention scanning for a specific command in a slice.

**Proposal:** Add `[DisableConventionDI]` attribute to skip auto-registration for edge cases.

**Use Case:** Command in a slice that wants manual control or no DI.

### 3. Debug/Logging Mode
**Current State:** No visibility into discovered services or computed namespace prefixes.

**Proposal:** Add `CliEngineOptions.EnableDiagnostics` to log:
- Computed namespace prefix for each command
- List of auto-registered services per slice
- Service resolution path (combined/shared/slice/none)

**Use Case:** Troubleshooting DI issues during development.

### 4. Error Handling & Diagnostics
**Current State:** `assembly.GetTypes()` or `Activator.CreateInstance` failures throw raw exceptions.

**Proposal:** Wrap in try-catch and provide context:
```csharp
try { /* discovery */ }
catch (Exception ex)
{
    throw new CliEngineException($"Failed to discover services in slice '{namespacePrefix}': {ex.Message}", ex);
}
```

**Use Case:** Better developer experience when service registration fails.

### 5. Per-Command Scoped Services
**Current State:** All services are Transient; no scoped lifetime support.

**Proposal:** Create a per-command scope in `CommandController.Execute()`:
```csharp
using var scope = provider.CreateScope();
var command = ActivatorUtilities.CreateInstance(scope.ServiceProvider, commandType);
```

**Use Case:** Services that need per-execution state (e.g., unit of work pattern).

### 6. Convention-Based Model/Validator Discovery
**Current State:** Only services are auto-discovered.

**Proposal:** Extend to Models/Validators if they need DI (rare but possible for validation services).

**Use Case:** Complex validators requiring external data sources.

### 7. Convention-Based Settings Management
**Current State:** No built-in convention for command-specific or global settings binding.

**Proposal:** The engine should automatically discover and register strongly-typed settings. No extra calls in `Program.cs` should be needed.
- **Global settings:** Binds `appsettings.json` to an `IOptions<AppSettings>` injectable class.
- **Command-local settings:** The engine should scan for `Commands/<Feature>/Settings/<Feature>Settings.cs` and a corresponding `appsettings.<Feature>.json` file or JSON section.
- **Auto-registration:** Discovered settings classes are automatically registered for `IOptions<T>` injection.
- **Example:**
  ```csharp
  // Commands/Init/Settings/InitSettings.cs
  public class InitSettings { /* ... */ }
  
  // Commands/Init/InitCmd.cs
  public InitCmd(IOptions<InitSettings> settings) { /* ... */ }
  ```

**Use Case:** Command-specific timeouts, retry policies, default values, feature flags, without boilerplate registration.

### 8. Repository/DbContext Injection
**Current State:** No guidance on data access patterns for CLIs.

**Proposal:** Support optional DbContext/Repository injection for local-first CLIs:
- **Local-first CLIs** (e.g., VModCli, VX CLI): Direct `DbContext` injection for local SQLite/file-based DBs (module cache, user prefs, audit logs, encryption key history)
- **API-first CLIs** (cloud tools): Prefer HTTP clients over direct DB access
- **Hybrid pattern:** Local cache (DbContext) + remote sync (API service)
- **Convention:** Place in `Services/<Feature>/Repositories/` or `Services/<Feature>/Data/`

**Use Case:** Offline-capable tools, local state management, audit trails, caching.

**Recommendation:** Use DbContext only for local, tool-owned databases. For shared/remote databases, use APIs to avoid schema coupling and migration complexity.

### 9. Source Generators for DI
**Current State:** DI registration is reflection-based at runtime.

**Proposal:** Create an optional source generator (`Vexit.CliEngine.SourceGenerator`) that runs at compile time. It would scan for all conventions (`[Command]`, `Services/`, `Settings/`) and generate static, reflection-free registration code.

**Use Case:**
- **Performance:** Eliminates reflection overhead for near-zero startup cost.
- **Reliability:** Turns runtime DI errors into compile-time errors.
- **AOT/Trimming:** Enables full ahead-of-time compilation and assembly trimming for smaller, faster executables.

---

## Future Enhancements

- Per-command scope support for Scoped services
- Source generator for compile-time registration (optional package)
- Hot-reload support for module assemblies
- Telemetry/metrics for command execution and DI resolution

---

*© VEXIT ® 2025 | All rights reserved. | [www.vexit.com](https://www.vexit.com) | Tomorrow is today...®*
