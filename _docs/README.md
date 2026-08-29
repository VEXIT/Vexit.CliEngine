
|              |                                                       |
| ------------ | ----------------------------------------------------- |
| Copyright    | © VEXIT ® 2025 , www.vexit.com , Tomorrow is today... |
| Author       | Vex Tatarevic                                         |
| Date Created | 2025-09-16                                            |
| Date Updated | 2026-08-23                                            |

# Vexit.CliEngine



## Project Structure

```
Vexit.CliEngine/                    # 📚 Framework Library (CLI building blocks)
├── BaseClasses/                    # Inheritance-based command framework
│   ├── CmdBase.cs                      # Executable command base class
│   ├── CmdGroupBase.cs                 # Non-executable command group base class
│   ├── CmdStartupBase.cs               # Command-specific DI configuration base
│   └── HookBase.cs                     # Lifecycle hook base class
│   └── IHook.cs                        # Lifecycle hook interface
│
├── Attributes/                      # Command decoration and metadata
│   ├── CommandAttribute.cs              # Defines executable commands
│   ├── AddServiceGroupAttribute.cs      # Opts into shared service groups
│   ├── ArgumentAttribute.cs             # Positional command arguments
│   ├── OptionAttribute.cs               # Named command options/flags
│   ├── AliasesAttribute.cs              # Command aliases
│   └── ...
│
├── Components/                      # Reusable CLI output components
│   ├── ProgressMessage.cs               # Progress bars and status messages
│   └── Components.cs                    # Component utilities
│
├── DependencyInjection/             # Advanced DI patterns and caching
│   ├── IServiceRegistry.cs              # Service group registration contract
│   ├── ConventionBasedServiceRegistry.cs # Auto-discovery of services by convention
│   ├── ServiceProviderCache.cs          # Caches compiled service providers
│   ├── ConventionServiceProviderCache.cs # Caches convention-based providers
│   └── ...
│
├── Utils/                           # CLI-specific utilities
│   ├── CliUtil.cs                       # Static CLI helpers (Program.cs / out-of-DI; prefer ICliService in commands)
│   ├── CmdUtil.cs                       # Static command utilities. Methods that decide command type passed on argunents passed
│   ├── CmdTypeUtil.cs                   # Command type analysis
│   ├── ShellUtil.cs                     # Shell execution utilities
│   ├── VersionUtil.cs                   # Entry-assembly version for -v/--version
│   └── ...
│
├── Constants/                       # Framework constants
│   ├── ProcessCliFlags.cs               # -m / --machine token constants
│   ├── RootCliFlags.cs                  # Root -v/--version detection
│   ├── Text.cs                          # User-facing text and messages
│   ├── Hooks.cs                         # Hook identifiers
│   ├── FailureCodes.cs                  # Standardized error codes
│   └── Shells.cs                        # Shell detection constants
│
├── Enums/                          # Type-safe enumerations
│   ├── DataFormatSE.cs                  # Output format options
│   └── ...
│
├── Models/                         # Data transfer objects
│   └── CommandContext.cs                # Command execution context
│
├── ICliService.cs                   # Injectable CLI output contract
├── CliService.cs                    # ICliService implementation (margins + CliConfig)
├── CliConfig.cs                    # CLI output configuration
├── CliParser.cs                    # Argument parsing engine
├── CliValidator.cs                 # Command validation logic
├── CommandController.cs            # Main command execution orchestrator
├── CommandRegistry.cs              # Command discovery and registration
├── CommandNode.cs                  # Command hierarchy nodes
├── HelpGenerator.cs                # Auto-generated help text
├── ParsedArguments.cs              # Argument parsing results
└── CliEngineExtensions.cs          # AddVexitCliEngine / UseCliEngine / AddHook
```

---


## Overview

`Vexit.CliEngine` enables building command-line interfaces using attributes and conventions, minimizing boilerplate and maximizing productivity. It supports simple commands, complex command hierarchies, and advanced dependency injection patterns like vertical slicing and explicit service groups.


## CliService - Consistent CLI Output

Vexit.CliEngine provides `ICliService` / `CliService` for consistent, injectable CLI output across commands and supporting services. Prefer injecting the **interface** (`ICliService`); `CliService` is the registered implementation.

Unlike static `CliUtil`, this path is DI-friendly: styling comes from `CliConfig`, and commands stay testable.


### Registration (wired by CliEngine — you do not register it)

Calling `AddVexitCliEngine` registers core engine services for you, including:

```csharp
services.AddScoped<ICliService, CliService>();
```

The same mapping is also registered by the built-in `CliEngineServices` registry so **every command’s DI scope** gets `ICliService` automatically.

**Consumer apps do not** add `AddScoped<ICliService, CliService>` themselves. Only call `AddVexitCliEngine` (and optionally set `CliConfig`).


### Setup

Configure styling when you add the engine in `Program.cs`:

```csharp
builder.AddVexitCliEngine(options =>
{
    options.CliConfig = new CliConfig
    {
        LabelColor = ConsoleColor.White,
        InputColor = ConsoleColor.Green,
        ProgressMessageColor = ConsoleColor.DarkGray
    };
});
```


### Usage

Inject `ICliService` into commands or other DI-resolved types:

```csharp
public class MyCmd(ICliService cli) : CmdBase
{
    private readonly ICliService _cli = cli;

    public override Result Execute()
    {
        _cli.WriteLn("Starting work...");
        _cli.WriteLnSuccess("Work completed!");
        return Ok();
    }
}
```

**Exception:** code that runs **outside** a command DI scope - specifically `Program.cs` should use static `CliUtil` instead of `ICliService` e.g. `CliUtil.WriteData` instead of `_cli.WriteData`.


### Features

- **Consistent Styling**: Same colors and formatting everywhere via `CliConfig`
- **Dependency Injection**: Registered by CliEngine; inject `ICliService` — no consumer wiring
- **Global Margins**: Automatic indentation support
- **Rich Output**: Colors, progress bars, formatted text
- **Async Support**: Full async/await compatibility (e.g. progress messages)
- **Machine stdout**: `WriteData<T>` for structured output on stdout


## Quick Start

Here's a complete working command example with advanced formatting features:

```csharp
using Vexit.CliEngine;
using Vexit.CliEngine.Attributes;
using Vexit.CliEngine.BaseClasses;
using Vexit.Common.Models;

namespace MyCli.Commands;

[Command("greet", "Greet someone by name")]
public class GreetCmd(ICliService cli) : CmdBase
{
    private readonly ICliService _cli = cli;

    [Argument("name", "The name to greet")]
    public string? Name { get; set; }

    [Option("world", "w", "Greet the world as well")]
    public bool World { get; set; }

    [Option("times", "t", "Number of times to repeat the greeting")]
    public int Times { get; set; } = 1;

    [Option("colours", "c", "Use colored output with XML-like formatting tags")]
    public bool UseColours { get; set; }

    public override Result Execute()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            return Failure("MISSING_ARGUMENT", "Name is required");
        }

        for (int i = 0; i < Times; i++)
        {
            if (UseColours)
            {
                if (World)
                {
                    _cli.WriteLnFormat($"<s>Hello, {Name}!</s> <d>...and </d> <w>hello World</w><d>!</d>");
                }
                else
                {
                    _cli.WriteLnFormat($"<i>Hello, {Name}!</i>");
                }
            }
            else
            {
                var message = $"Hello, {Name}!";

                if (World)
                {
                    message += " ...and hello World!";
                }

                _cli.WriteLn(message);
            }
        }

        return Ok();
    }
}
```

**Gradual Buildup Examples:**

```bash


# Basic greeting

$ mycli greet "Nikola Tesla"
Hello, Nikola Tesla!


# With colored output using XML-like tags

$ mycli greet "Nikola Tesla" -c
Hello, Nikola Tesla!


# With world greeting and colors

$ mycli greet "Nikola Tesla" -w -c
Hello, Nikola Tesla! ...and hello World!


# Repeat multiple times

$ mycli greet "Nikola Tesla" -w -c -t 3
Hello, Nikola Tesla! ...and hello World!
Hello, Nikola Tesla! ...and hello World!
Hello, Nikola Tesla! ...and hello World!
```

**Key Features Demonstrated:**
- **Arguments:** `[Argument("name")]` for required positional parameters
- **Options:** `[Option("world", "w")]` for flags and values
- **ICliService injection:** Inject `ICliService` (registered by CliEngine); call `_cli.WriteLn()` / `_cli.WriteLnFormat()` for styled output with global margins
- **Error Handling:** `Failure(code, message)` / `Ok()` helpers from `CmdBase`


## Formatting

### Mixed Contextual Colors with XML Tags

Vexit.CliEngine supports inline mixed contextual colors using simple XML-like tags. This allows you to apply different colors to different parts of the same line without breaking the output flow.

**Available Tags:**
- `<i>text</i>` - Info (`ConsoleColor.Cyan`)
- `<s>text</s>` - Success (`ConsoleColor.DarkGreen`)
- `<w>text</w>` - Warning (`ConsoleColor.Yellow`)
- `<e>text</e>` - Error (`ConsoleColor.Red`)
- `<d>text</d>` - Dim (`ConsoleColor.DarkGray`)
- `<l>text</l>` - Lite (`ConsoleColor.Gray`)
- `<c>text</c>` - Code (`ConsoleColor.DarkGreen`)

Text outside tags uses the main/default color (`ConsoleColor.White` unless you pass `mainColor`).
Tags are case-insensitive.

**Example Usage:**

```csharp
// Mixed colors in one line
_cli.WriteLnFormat($"<s>Success:</s> <i>Info</i> <w>warning</w> <e>error</e> path <c>/var/app</c>");

// Single context colors (alternative)
_cli.WriteLnSuccess("This entire line is green");
_cli.WriteLnWarning("This entire line is yellow");
```


**Benefits:**
- **Mixed Colors in One Line:** Apply different colors to different parts of the same output line
- **Contextual Meaning:** Each tag represents a semantic meaning (info, success, warning, etc.)
- **Simple Syntax:** XML-like tags are easy to read and understand
- **Automatic Formatting:** The framework handles all the complexity of color application and console management


### Global CLI Output Margins

Vexit.CliEngine supports configurable global top and left margins for all CLI output, allowing you to create visually appealing command layouts and proper indentation across your entire application.

**Environment Variables:**
- `VEXIT_CLI_TOP_MARGIN` - Number of empty lines to add before the first output (default: 0)
- `VEXIT_CLI_LEFT_MARGIN` - Number of spaces to indent all output (default: 0)

**Setup in ~/.bash_profile:**

```bash


# Add visual spacing and indentation to VEXIT CLI output

export VEXIT_CLI_TOP_MARGIN=1
export VEXIT_CLI_LEFT_MARGIN=2
```

**Usage:**

In commands, inject `ICliService` and use its write methods. They add `VEXIT_CLI_LEFT_MARGIN` on top of any per-call `indent`. Avoid calling `CliUtil` / `Cli` from commands — that bypasses the service margin.

```csharp
[Command("example", "Example command with global margins")]
public class ExampleCmd(ICliService cli) : CmdBase
{
    private readonly ICliService _cli = cli;

    public override Result Execute()
    {
        // These automatically respect global margins
        _cli.WriteLn("This text will be indented by VEXIT_CLI_LEFT_MARGIN spaces");
        _cli.WriteLnFormat("<s>Success message</s> with <w>warning highlight</w>");
        _cli.WriteLnSuccess("This success message is automatically indented");

        // DON'T do this from a command - bypasses ICliService margins:
        // CliUtil.WriteLn("This won't get the service left margin");

        return Ok();
    }
}
```

**Example Output with VEXIT_CLI_LEFT_MARGIN=2:**

```bash
  This text will be indented by 2 spaces
  Success message with warning highlight
  ✅ This success message is automatically indented
```

**Benefits:**
- **Consistent Layout:** All commands in your CLI automatically respect the same margin settings
- **Visual Polish:** Create professional-looking CLI output with proper spacing
- **Environment-Based:** Different users can customize margins in their shell profile
- **Automatic Application:** No code changes needed - just set environment variables
- **Per-Command Tracking:** Top margins are applied only once per command execution

---


## Command Hierarchy

The engine builds a command tree at runtime, allowing for natural and complex command structures (e.g., `my-cli new dotnet-web-api`).

- **Command Groups (`CmdGroupBase`):** Optional non-executable classes that define parent nodes in the command tree. They can provide shared options to all child commands. If absent, the engine auto-generates group nodes based on folder names.

- **Executable Commands (`CmdBase`):** Concrete classes decorated with `[Command]` that perform actions.


### Folder & Naming Convention

- Each command lives under `Commands/[Folder]/`.
- The **folder name must match the command name** (minus `Cmd`) for vertical slices, e.g., `Commands/Init/InitCmd.cs`.
- If multiple commands share a folder (e.g., `Commands/New/DotnetWebApi/DotnetWebApiCmd.cs` and `Commands/New/Nextjs/NextjsCmd.cs`), the folder represents a command group.
- Explicit `[GroupName]CmdGroup` classes are optional. When omitted, the engine auto-creates the group node using the folder name (kebab-cased) and default metadata.


### Examples

- Root command `greet`: `Commands/GreetCmd.cs` → provides `my-cli greet`.
- Vertical slice `init`: `Commands/Init/InitCmd.cs` → provides `my-cli init` (no group class needed).
- Grouped commands under `new`:
  - `Commands/New/DotnetWebApi/DotnetWebApiCmd.cs` → provides `my-cli new dotnet-web-api`.
  - `Commands/New/Nextjs/NextjsCmd.cs` → provides `my-cli new nextjs`.
  - Optional `Commands/New/NewCmdGroup.cs` can add shared options for all `my-cli new *` commands.


## Command - Complex (Vertical Slice)

- Same rules as a simple command, but it has its own folder named `[CommandName]`.
- Example: root command `init` lives in `Commands/Init/InitCmd.cs` → provides `my-cli init` command.
NOTE: the `Init` folder is a slice container, not a group, so there is no `InitCmdGroup`. Command container can contain all the layers of logic that are used just by that command. It can contain folders like Models, Services, Ops, Constants, Validators, Clis, etc.


## Vertical Slicing & Convention-Based DI

Organize your commands into self-contained feature folders. The engine automatically discovers and wires up dependencies based on namespace conventions, making features truly modular.
- **Structure:** `Commands/<FeatureName>/`
- **Convention:** Services placed in `Commands/<FeatureName>/_Services/` are automatically registered for injection into the command in that folder.
- **Benefit:** Drop a new command folder into your project, and its local dependencies are instantly available with zero manual registration.


## Service Groups

For shared, cross-cutting concerns (e.g., encryption, file system access), you can define explicit bundles of services. Commands can then opt-in to these shared services using a simple attribute.

- **How:** Define an `IServiceRegistry`, then decorate a command with `[AddServiceGroup(YourRegistryName)]`, for example: `[AddServiceGroup<SecurityServiceGroup>]`. Optional `Order` property available for fine control of registration sequence.
- **Modular Organization:** Service registries and their related service implementations are co-located in the same folder for portability. The `IServiceRegistry` class serves as the bundle manifest for a cohesive set of services.
- **Benefit:** Keeps commands decoupled from global service locators and ensures they only load the shared dependencies they actually need.
- **Composable:** ServiceGroups now *layer* on top of slice services. The engine applies ServiceGroup registries first and then registers `Commands/<Slice>/_Services/*` by convention. This means a complex command can consume both shared infrastructure and slice-local helpers without extra wiring. Only a `<CommandName>Startup` class can override these registrations.


### Service Group Organization Convention

Service groups can be organized in two ways depending on whether they include local project services:


#### **1. External-Only Service Groups**

When a service group only registers external/third-party library services, place the registry directly in `Services/`:

**Example:**
```
Services/
├── WorkflowServiceGroup.cs     # Registers FlowEngine services (external library)
├── SshServiceGroup.cs          # Registers SSH services (external library)
├── LoggingServiceGroup.cs      # Registers Logging services (external library)
└── CliServiceGroup.cs          # Registers CliEngine services (external library)
```


#### **2. Local Services Groups**

When a service group includes local project services, create a feature folder containing both the registry and local services:

**Example:**
```
Services/
├── Encryption/                       # Feature bundle with local services
│   ├── EncryptionServiceGroup.cs    # IServiceRegistry + registration logic
│   ├── ILocalKeyService.cs          # Local service interface
│   ├── LocalKeyService.cs            # Local service implementation
│   ├── IEncryptionProvider.cs        # Another local service interface
│   └── ExternalEncryptor.cs         # Local wrapper for external service
│
└── WorkflowServiceGroup.cs          # External-only service group (no feature folder)
```

**Usage:**
```csharp
[AddServiceGroup<WorkflowServiceGroup>]    // External services only
[AddServiceGroup<EncryptionServiceGroup>]  // Includes local project services
public class SecureCmd : CmdBase { ... }
```


## Command Startup Hook

For commands that require complex, context-aware dependency resolution (e.g., choosing a service based on the current directory or command-line arguments), you can use a `CmdStartupBase` implementation. This is different from [Lifecycle Hooks](#lifecycle-hooks), which are for global cross-cutting behavior across all commands.

> **Command Startup Hook** - is a per-command DI configuration that runs when a specific command is being instantiated, allowing dynamic service registration based on runtime context.

- **Automatic Discovery:** No manual wiring required. The framework automatically discovers and executes startup hooks using naming convention.
- **DI Override:** When a startup hook exists, it creates a **new ServiceProvider** for that command, effectively overriding global DI registrations. The new provider contains:
  1. ServiceGroups (if command has `[AddServiceGroup]` attributes)
  2. Startup hook registrations (your custom DI logic)
  3. Convention-based services from the command's slice namespace
- **Naming Convention:** `[CommandName]Startup` where `[CommandName]` is the command class name without the "Cmd" suffix.
  - Command: `InitCmd` → Startup: `InitStartup`
  - Command: `ApiServerCmd` → Startup: `ApiServerStartup`
- **Location:** Must be in the **same folder/namespace** as the command file, inside the command's container folder (not group folder).
  - ✅ Correct: `Commands/Init/InitCmd.cs` and `Commands/Init/InitStartup.cs` (same folder)
  - ✅ Correct: `Commands/New/ApiServer/ApiServerCmd.cs` and `Commands/New/ApiServer/ApiServerStartup.cs` (same folder)
  - ❌ Wrong: `Commands/New/NewStartup.cs` (group folder, not command folder)
- **When:** Executes during command instance creation, before the command's `Execute()` method is called.
- **Use Case:** Choose service implementations dynamically (e.g., detect project type and register `DotNetInitService` vs `NextjsInitService`).
- **Benefit:** Allows for dynamic, runtime service registration that keeps your `Program.cs` clean and places DI logic right next to the command that uses it.
- **Difference from Lifecycle Hooks:** Command Startup Hooks are per-command DI configuration, while Lifecycle Hooks are global cross-cutting behavior that runs for all commands.

- **Example Implementation:**
  ```csharp
  // Commands/Init/InitStartup.cs
  namespace Vexit.VxCli.Commands.Init;
  
  /// <summary>
  /// Startup for the Init command that selects the appropriate service 
  /// implementation based on project type.
  /// </summary>
  public class InitStartup : CmdStartupBase
  {
      public override void Program_AddServices(IServiceCollection services, CommandContext context)
      {
          // Resolve project path using working directory context
          var pathResult = ResolveProjectPathOp.Execute(context.WorkingDirectory);
          if (pathResult.IsFailure)
          {
              services.AddTransient<IInitService>(_ => 
                  new UnsupportedInitService(pathResult.Message ?? "Invalid project path."));
              return;
          }
          var projectPath = pathResult.Data!.ProjectPath;
  
          // Detect project type without throwing; fallback to unsupported service if unknown
          var typeResult = DetectProjectTypeOp.Execute(projectPath);
          if (typeResult.IsFailure)
          {
              services.AddTransient<IInitService>(_ => 
                  new UnsupportedInitService(typeResult.Message ?? "Unsupported or undetected project type."));
              return;
          }
          var projectType = typeResult.Data!;
  
          // Register cross-cutting services required by InitCmdService
          services.AddTransient<IProjectAnalyzerService, ProjectAnalyzerService>();
          services.AddTransient<IKeyVaultService, KeyVaultService>();
          services.AddTransient<IEnvironmentKeyService, EnvironmentKeyService>();
          services.AddTransient<IProjectsRegistryService, ProjectsRegistryService>();
  
          // Register InitVXInfraService (orchestrates the full init workflow)
          services.AddTransient<IInitVXInfraService, InitVXInfraService>();
  
          // Register the appropriate IInitService implementation based on the project type
          switch (projectType.Framework)
          {
              case ProjectFrameworkEnum.DotNet:
                  services.AddTransient<IInitService, DotNetInitService>();
                  break;
              case ProjectFrameworkEnum.Nextjs:
                  services.AddTransient<IInitService, NextjsInitService>();
                  break;
              default:
                  services.AddTransient<IInitService>(_ => 
                      new UnsupportedInitService($"Project framework '{projectType.Framework}' is not supported."));
                  break;
          }
      }
  }
  ```
  
  This example shows how `InitStartup` dynamically selects the appropriate `IInitService` implementation based on the detected project framework, demonstrating context-aware dependency resolution.


## Root Version Flag

CliEngine handles **`-v`** and **`--version`** at the root level, the same way it handles **`--help`** / **`-h`**: before command resolution, print and exit successfully.

```bash
mycli -v
mycli --version


# => 1.0.0

```

- **Root only:** `mycli git -v` is **not** intercepted; the `git` command receives its own arguments.
- **Version source:** the **entry assembly** (your CLI executable), not CliEngine. Set `<Version>` in your app `.csproj` so releases report the correct value.
- **Format:** `major.minor.build` (e.g. `1.0.0`). When the assembly has no version, prints `unknown`.
- **No extra wiring:** works for every consumer automatically; no `VersionCmd` class required.


## Start Page

You can provide a command class named `StartCmd` inside `Commands/` folder. CLI engine will automatically execute it when the user runs your application with no arguments.

For example if your CLI tool is called `mycli`, and you create `StartCmd` class in the `Commands` folder and decorate it with the `[Command("start")]` attribute.

Then when the user types in command `mycli` without any arguments, the CLI engine will automatically execute the `StartCmd` command and display what ever you programmed into it.

- **Implementation:** Create a class `public class StartCmd : CmdBase`.
- **User Experience:** Provides a welcoming, interactive start page or dashboard instead of just showing a help screen.


## Interactive Shell

You can use **programmatic command execution** with **ICommandExecutor** service to implement an interactive shell functionality or just to chain commands programmatically in some other advanced scenarios.

**Note:** `ICommandExecutor` is **automatically registered** by the CliEngine framework and available for injection in any command without requiring manual service registration.

To achieve programmatic command execution, inject the `ICommandExecutor` service and call the `Execute` method of the service like this:

  ```csharp
  public class MyInteractiveCmd(ICommandExecutor executor, ICliService cli) : CmdBase
  {
      private readonly ICommandExecutor _executor = executor;
      private readonly ICliService _cli = cli;

      public override async Task<Result> ExecuteAsync()
      {
          // Single command execution (args already bound to [Option]/[Argument] properties)
          var userInput = _cli.ReadInput(); // e.g., "init --force"
          return await _executor.Execute(userInput);
      }
  }
  ```

- **Interactive Shell Example:** Use ICommandExecutor inside Start Page
  ```csharp
  [Command("start", "Interactive dashboard and command shell")]
  public class StartCmd(ICommandExecutor executor, ICliService cli) : CmdBase
  {
      private readonly ICommandExecutor _executor = executor;
      private readonly ICliService _cli = cli;

      public override async Task<Result> ExecuteAsync()
      {
          DisplayDashboard(); // Show welcome screen once

          while (true)
          {
              _cli.Write("Enter command or hit Enter to exit: ");
              var input = _cli.ReadInput();

              if (string.IsNullOrWhiteSpace(input))
                  return Ok(); // Exit shell

              // Execute command and continue loop
              var result = await _executor.Execute(input.Trim());
              if (result.IsFailure) _cli.WriteLn(); // Add spacing
          }
      }
  }
  ```

- **Benefits:**
  - **Interactive CLI Experiences:** Create shells where one command can host others
  - **Command Chaining:** Execute multiple commands programmatically
  - **Debugging Convenience:** Set breakpoints in your command code, run in debug mode, then interactively execute commands to test specific scenarios
  - **Proper DI Integration:** Commands executed this way still receive full dependency injection and respect the CLI execution pipeline


## Run from File

For batch command execution or when you need to run commands with sensitive data (like passwords) without typing them in the terminal, you can create a command that reads and executes commands from a file using `ICommandExecutor`.

**Key Features:**
- **Security First:** Command arguments are masked in output to protect sensitive data
- **Smart Continue Logic:** Only prompts to continue when there are actually more commands remaining
- **Error Handling:** Comprehensive error handling with user-friendly prompts
- **Comment Support:** Supports comments (#) and empty lines in command files

**Example Implementation:**
```csharp
[Command("run", "Execute commands from a file")]
public class RunCmd(ICommandExecutor commandExecutor, ICliService cli) : CmdBase
{
    private readonly ICommandExecutor _commandExecutor = commandExecutor;
    private readonly ICliService _cli = cli;

    [Option("file", "f", "Path to file containing commands to execute", isRequired: true)]
    public string? FilePath { get; set; }

    public override async Task<Result> ExecuteAsync()
    {
        if (string.IsNullOrWhiteSpace(FilePath) || !File.Exists(FilePath))
            return FailWithMessage("Valid file path required");

        var lines = await File.ReadAllLinesAsync(FilePath);

        // Count valid commands for smart continue logic
        var validCommands = lines.Count(line =>
        {
            var trimmed = line.Trim();
            return !string.IsNullOrWhiteSpace(trimmed) &&
                   !trimmed.StartsWith("#") &&
                   trimmed.StartsWith("yourcli ", StringComparison.OrdinalIgnoreCase);
        });

        var executedCount = 0;
        var failedCount = 0;
        var processedCount = 0;

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            if (string.IsNullOrWhiteSpace(trimmedLine) || trimmedLine.StartsWith("#"))
                continue;

            if (!trimmedLine.StartsWith("yourcli ", StringComparison.OrdinalIgnoreCase))
            {
                _cli.WriteLnWarning($"Skipping non-yourcli command: {trimmedLine}");
                continue;
            }

            processedCount++;

            // Security: Mask command display (stop at first dash, add ellipsis)
            var commandToExecute = trimmedLine.Substring(8).Trim();
            var dashIndex = commandToExecute.IndexOf(" -");
            var commandDisplay = dashIndex >= 0 ?
                commandToExecute.Substring(0, dashIndex) : commandToExecute;
            var commandParts = commandDisplay.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var display = $"yourcli {commandParts[0]}";
            if (commandParts.Length > 1) display += $" {commandParts[1]}";
            if (commandParts.Length > 2 || dashIndex >= 0) display += " ...";

            _cli.Write($"$ {display}");
            _cli.WriteLn();

            try
            {
                var result = await _commandExecutor.Execute(commandToExecute);

                if (result.IsFailure)
                {
                    failedCount++;
                    _cli.WriteLnError($"Command failed: {result.Message}");

                    // Smart continue: only ask if more commands exist
                    var remainingCommands = validCommands - processedCount;
                    if (remainingCommands > 0)
                    {
                        var continueResult = _cli.PromptYesNo($"Continue with remaining {remainingCommands} commands?", true);
                        if (!continueResult)
                        {
                            _cli.WriteLnDim("Execution stopped by user.");
                            break;
                        }
                    }
                }
                else
                {
                    executedCount++;
                    _cli.WriteLnSuccess("Command completed successfully");
                }
            }
            catch (Exception ex)
            {
                failedCount++;
                _cli.WriteLnError($"Command execution failed: {ex.Message}");

                // Smart continue logic for exceptions too
                var remainingCommands = validCommands - processedCount;
                if (remainingCommands > 0)
                {
                    var continueResult = _cli.PromptYesNo($"Continue with remaining {remainingCommands} commands?", true);
                    if (!continueResult)
                    {
                        _cli.WriteLnDim("Execution stopped by user.");
                        break;
                    }
                }
            }
        }

        _cli.WriteLnDim($"Execution summary: {executedCount} successful, {failedCount} failed");
        return failedCount > 0 ? FailWithMessage("Some commands failed") : Ok();
    }
}
```

**Example Command File:**
```bash


# This file contains batch commands


# Comments starting with # are ignored


# Basic commands

yourcli greet John


# Commands with sensitive data (passwords don't appear in command history)

yourcli server setup myhost --ip 192.168.1.100 --initial-user root --initial-password mypassword


# Interactive commands

yourcli interactive-command
```

**Usage:**
```bash
yourcli run --file commands.txt
```

**Example Output (with security masking):**
```bash
$ yourcli greet ...
Command completed successfully

$ yourcli server setup ...
Command completed successfully
```

**Benefits:**
- **Batch Processing:** Execute multiple commands from a single file
- **Security First:** Sensitive data stays in files, command arguments are masked in output
- **Smart Error Handling:** Only prompts to continue when more commands actually remain
- **Reproducibility:** Version control your command sequences
- **Automation:** Perfect for deployment scripts and CI/CD pipelines


## Lifecycle Hooks

For cross-cutting concerns that need to run at various points in the CLI lifecycle (e.g., security cleanup, update checks, logging, validation), you can implement lifecycle hooks that execute automatically without modifying individual commands or the framework itself.

> **Lifecycle Hooks** - are services that run at specific points in the CLI execution lifecycle, allowing you to add behavior without modifying individual commands or the framework itself. They are different from [Command Startup Hook](#command-startup-hook), which is a per-command DI configuration.

- **Hook Types:**
  - **Startup Hooks**: Execute once per process (before command resolution). Perfect for update checks, environment validation, or one-time initialization. Runs even in interactive shells (once when shell starts).
  - **Blocking Hooks**: Execute before command and can abort execution by returning a failure `Result`. The command waits for blocking hooks to complete before proceeding.
  - **Non-Blocking Hooks**: Execute before command but don't block execution. The command proceeds immediately while hooks run in the background. Perfect for cleanup operations that shouldn't delay commands.
  - **After Hooks**: Execute after command completion (even if command failed). Always non-blocking and run in the background.

- **Interface:**
  ```csharp
  public interface IHook
  {
      int Order { get; } // Lower = earlier execution (default: 0)
      
      // Startup: Runs once per process (before command resolution)
      Task OnStartup(Context context);
      
      // Blocking: Command waits, can abort via Result
      Task<Result> OnBeforeExecuteBlocking(CmdBase command, Context context);
      
      // Non-blocking: Fire-and-forget, command proceeds immediately
      Task OnBeforeExecute(CmdBase command, Context context);
      
      // After: Always non-blocking, runs even if command failed
      Task OnAfterExecute(CmdBase command, Result result);
  }
  ```

- **Base Class:**
  ```csharp
  public abstract class HookBase : IHook
  {
      public virtual int Order => 0;
      public virtual Task OnStartup(Context context) => Task.CompletedTask;
      public virtual Task<Result> OnBeforeExecuteBlocking(CmdBase command, Context context) 
          => Task.FromResult(Result.Ok());
      public virtual Task OnBeforeExecute(CmdBase command, Context context) 
          => Task.CompletedTask;
      public virtual Task OnAfterExecute(CmdBase command, Result result) 
          => Task.CompletedTask;
  }
  ```
  
  Inherit from `HookBase` and override only the methods you need. All methods have default no-op implementations, so you only implement what's necessary.

- **Context:**
  ```csharp
  public class Context
  {
      public string[] Args { get; init; }
      public Dictionary<string, string> NamedArgs { get; init; }
      public string WorkingDirectory { get; init; }
      public DateTime ExecutionTime { get; init; }
  }
  ```

- **Registration:**
  ```csharp
  // In Program.cs
  var builder = Host.CreateApplicationBuilder(args);
  builder.AddVexitCliEngine();
  builder.AddHook<TempFolderCleanup_OnBeforeExecute>();
  builder.AddHook<CheckUpdates_OnStartup>();
  ```
  
  > `AddHook<T>()` is an extension on `IHostApplicationBuilder` (not `IServiceCollection`). Hooks are resolved from DI and run by the framework.

- **Naming Convention:**
  For single-purpose hooks (most common), use the pattern `[Action]_[When]`:
  - `CheckUpdates_OnStartup` - Checks for updates on startup
  - `TempFolderCleanup_OnBeforeExecute` - Cleans up temp folders before command execution
  - `AuthCheck_OnBeforeExecuteBlocking` - Validates authentication before command (blocking)
  - `PerformanceMonitor_OnAfterExecute` - Monitors performance after command execution
  
  This naming convention makes hooks self-documenting and reads naturally: `AddHook<CheckUpdates_OnStartup>()` reads like "Add hook: Check Updates On Startup".
  
  For multi-purpose hooks that implement multiple lifecycle methods, use traditional PascalCase names without underscores (e.g., `LoggingHook`).

- **Folder Convention:**
  Keep all hook implementations in a `Hooks/` folder at the root of your CLI project. The folder location makes the "Hook" suffix unnecessary in class names:
  ```
  YourCli/
  ├── Commands/
  ├── Hooks/
  │   ├── CheckUpdates_OnStartup.cs
  │   ├── TempFolderCleanup_OnBeforeExecute.cs
  │   └── LoggingHook.cs  // Multi-purpose hook
  └── Program.cs
  ```

- **Execution Location:**
  Hooks are executed by `CommandController` at specific points:
  - **Startup hooks**: Execute in `CommandController.Execute()` before command resolution (once per process)
  - **Before hooks**: Execute in `CommandController.ExecuteCommand()` before command execution
  - **After hooks**: Execute in `CommandController.ExecuteCommand()` after command execution

- **Example Implementation (Before Hook):**
  ```csharp
  public class TempFolderCleanup_OnBeforeExecute : HookBase
  {
      public override int Order => 0; // Run first

      // Only override what you need - other methods have default no-op implementations
      public override async Task OnBeforeExecute(CmdBase command, Context context)
      {
          try
          {
              var projectsDir = Path.Combine(
                  Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), 
                  ".vexit", "projects");
              
              if (!Directory.Exists(projectsDir)) return;

              var projectDirs = Directory.GetDirectories(projectsDir);
              foreach (var projectDir in projectDirs)
              {
                  var tempPath = Path.Combine(projectDir, "_temp");
                  if (Directory.Exists(tempPath))
                  {
                      try
                      {
                          Directory.Delete(tempPath, recursive: true);
                      }
                      catch (Exception ex)
                      {
                          // Print warning but don't block command
                          Cli.WriteLnWarning(
                              $"⚠️  Could not delete _temp folder: {tempPath}\n" +
                              $"   Error: {ex.Message}\n" +
                              $"   Please close any open files and delete manually.");
                      }
                  }
              }
          }
          catch
          {
              // Silently fail - don't break commands if cleanup fails
          }
      }

      // No need to override other methods - base class provides no-op defaults
  }
  ```

- **Example Implementation (Startup Hook with Caching):**
  ```csharp
  public class CheckUpdates_OnStartup : HookBase
  {
      public override int Order => 100; // Run last (low priority)

      // Only override startup - other methods have default no-op implementations
      public override async Task OnStartup(Context context)
      {
          var config = LoadCliConfig(); // Read from ~/.vexit/cli-config.json
          var lastCheck = config.LastUpdateCheck;
          var frequencyHours = config.UpdateCheckFrequencyHours ?? 24;
          
          var hoursSinceCheck = (DateTime.UtcNow - lastCheck).TotalHours;
          
          if (hoursSinceCheck < frequencyHours)
          {
              return; // Skip check - too soon
          }
          
          // Non-blocking update check
          _ = Task.Run(async () =>
          {
              try
              {
                  var updateAvailable = await CheckForUpdatesAsync();
                  if (updateAvailable)
                  {
                      Cli.WriteLnWarning("⚠️  A new version is available! Run 'vx update' to upgrade.");
                  }
                  
                  // Update config with new check time
                  config.LastUpdateCheck = DateTime.UtcNow;
                  SaveCliConfig(config);
              }
              catch
              {
                  // Silently fail - don't annoy users with network errors
              }
          });
      }
      
      // No need to override other methods - base class provides no-op defaults
  }
  ```

- **Execution Order:**
  1. **Startup hooks** execute first (once per process, before command resolution)
     - Hooks sorted by `Order` property (lower numbers first)
     - All startup hooks are non-blocking (fire-and-forget)
     - Executed in `CommandController.Execute()` before command resolution
  2. Command is resolved from arguments
  3. **Blocking hooks** execute (in order), command waits for completion
     - If any blocking hook returns failure, command execution is aborted
  4. **Non-blocking hooks** execute in parallel (fire-and-forget), command proceeds immediately
  5. Command executes
  6. **After hooks** execute in parallel after command completes (regardless of success/failure)

- **Single vs Multi-Purpose Hooks:**
  Most hooks implement only one lifecycle method (single-purpose). For these, use the `[Action]_[When]` naming convention:
  ```csharp
  public class CheckUpdates_OnStartup : HookBase
  {
      public override async Task OnStartup(Context context) { /* only this */ }
  }
  ```
  
  Rarely, a hook may implement multiple methods (multi-purpose). For these, use traditional PascalCase names:
  ```csharp
  public class LoggingHook : HookBase
  {
      public override async Task OnStartup(Context context) { /* startup logging */ }
      public override async Task OnAfterExecute(CmdBase command, Result result) { /* after logging */ }
  }
  ```

- **Exception Handling:**
  - Blocking hooks: Exceptions are caught and converted to `Result.Failure()` to abort command
  - Non-blocking hooks: Exceptions are caught and logged as warnings, don't affect command execution
  - After hooks: Exceptions are caught and logged, don't affect command result

- **Benefits:**
  - **Separation of Concerns**: Framework stays generic, business logic in hooks
  - **No Framework Modifications**: Add behavior without changing CliEngine code
  - **Flexible Execution**: Blocking for validation, non-blocking for cleanup
  - **Works with Sync/Async**: Hooks are async but work transparently with sync commands
  - **Multiple Hooks**: Register multiple hooks, execution order controlled via `Order` property

- **Use Cases:**
  - **Startup hooks**: Update checks (with config-based caching), environment validation, one-time initialization
  - **Before hooks**: Security cleanup (delete temporary folders), authentication/authorization checks
  - **After hooks**: Logging and audit trails, performance monitoring, cleanup operations

---


## Architecture

The library uses a modular design with fluent extensions for easy integration:

- **AddVexitCliEngine**: Extension on `IHostApplicationBuilder` / `IServiceCollection` — registers core services (`ICliService`, `ICommandExecutor`, `CliConfig`, options) and configures assembly scanning.
- **UseCliEngine**: Extension on `IHost` to execute commands.
- **`AddHook<T>`**: Registers lifecycle hooks on the host builder.
- **Command Discovery**: Automatic scanning of specified assemblies for `[Command]`-attributed classes deriving from `CmdBase`.
- **DI Integration**: Uses Microsoft.Extensions.DependencyInjection; per-command scopes get CliEngine services via `CliEngineServices`.
- **Argument Parsing**: Supports **positional** arguments, **named** options (long/short), and key=value formats with validation.
- **Convention-Based Command Discovery**: Commands are organized by folder structure; hierarchy is inferred from namespace.
- **StartCmd Default Execution**: When no arguments are provided, automatically executes a `"start"` command if available, otherwise shows help.
- **Root Version Flag**: When invoked with only `-v` or `--version` at the root, prints the entry assembly version and exits (set `<Version>` in the consumer `.csproj`).
- **ProcessCliFlags / CmdUtil**: Every command inherits `-m` / `--machine` on `CmdBase`; `CmdUtil.IsMachineRequest(args)` in `Program.cs` for failure codes on stdout.
- **Separate Output Streams**: Human-facing on stderr (`ICliService` / `CliUtil`); machine data on stdout (`WriteData`).


### Constructor Injection in Commands

Commands can declare constructors with dependencies registered in the host container. The framework will use ActivatorUtilities to resolve them at runtime:

```csharp
using Vexit.CliEngine;

public interface ILogger { void Log(string msg); }
public class ConsoleLogger : ILogger
{
    // Prefer CliUtil here if this type is a singleton; ICliService is scoped
    public void Log(string msg) => CliUtil.WriteLn(msg);
}

[Command("hello", "Say hello with DI")]
public class HelloCmd(ILogger logger) : CmdBase
{
    private readonly ILogger _logger = logger;

    [Argument("name", "Name to greet", isRequired: false)]
    public string Name { get; set; } = "World";

    public override Result Execute()
    {
        _logger.Log($"Hello, {Name}!");
        return Ok();
    }
}
```

In your Program.cs:

```csharp
var isMachineMode = CmdUtil.IsMachineRequest(args);

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddSingleton<ILogger, ConsoleLogger>();
builder.AddVexitCliEngine();
var app = builder.Build();

var result = await app.UseCliEngine(args);

if (result.IsFailure)
{
    if (result.HasMessage)
    {
        CliUtil.WriteLn();
        CliUtil.WriteLnError(result.Message!, CliUtil.GlobalLeftMargin);
    }

    if (isMachineMode && !string.IsNullOrWhiteSpace(result.FailureCode))
        CliUtil.WriteTextData(result.FailureCode);

    Environment.Exit(1);
}
Environment.Exit(0);
```


## Result and Command Execution

Commands return structured `Result` objects for two key reasons:


### **Explicit Exit Codes for Automation**

- Success: Exit code 0 (silent success)
- Failure: Exit code 1+ (programmatic failure detection)
- Enables reliable CI/CD pipelines, scripts, and tool integration where commands communicate status without parsing output.


### **Centralized Error Handling (Optional)**

- Commands can handle their own error printing for immediate user feedback
- `Program.cs` can optionally print errors centrally for consistent formatting
- Framework bubbles up `Result` objects for uniform processing across the application

**Result Properties**:
- `IsSuccess`: Operation succeeded
- `IsFailure`: Any failure (logic or system error)
- `IsError`: System error with exception (subset of `IsFailure`)
- `HasMessage`: Whether the result has message to print. If we need to print message in custom way we can do it in command handlers and just not pass message to result which then results in HasMessage false and failure message will not printed in Program.cs
- `Exception`: The underlying exception (if `IsError`)

**Usage Patterns** (using CmdBase helper methods):
- **Success**: `return Ok();`
- **Logic Failure**: `return Failure("ERROR_CODE", "Error message");` — code first, then message
- **Message only**: `return FailWithMessage("Error message");`
- **Code only**: `return FailWithCode("ERROR_CODE");`
- **System Error**: `return Result.Error(ex);` — sets `IsError`
- **Proc Result Chaining**: `return Failure(procResult);` — converts failed `Result` / `Result<T>` to `Result`
- **Centralized Handling**: In `Program.cs`, check `result.HasMessage` to print errors centrally; use `CmdUtil.IsMachineRequest(args)` for stdout failure codes on any failure (including unknown command).


## Output Management with CliUtil

CliEngine separates **human** output (stderr) from **machine** data (stdout). **Never use `Console.WriteLine` directly.**

**In commands / DI-resolved types:** inject `ICliService` and call `_cli.WriteLn*`, `_cli.WriteData`, etc.

**Outside a command scope** (especially `Program.cs`): use static `CliUtil`, or the common alias:

```csharp
global using Cli = Vexit.CliEngine.Utils.CliUtil;
```


### **Setup: Global Using Alias**

Create a `GlobalUsings.cs` file in your CLI project root:

```csharp
global using Cli = Vexit.CliEngine.Utils.CliUtil;
```

This makes `Cli` available for `Program.cs` and other non-injected call sites.


### **Human-Facing Output (stderr)**

- Prefer `_cli.WriteLn()`, `_cli.WriteLnError()`, `_cli.WriteLnWarning()`, `_cli.WriteLnSuccess()`, `_cli.WriteLnFormat()`, etc. from injected `ICliService`.
- Static equivalents: `Cli.WriteLn*`, `CliUtil.WriteLn*` (same streams; no DI margin unless you pass `indent` / `GlobalLeftMargin` yourself).
- All of these write to `Console.Error` (stderr) so stdout stays clean for `WriteData`.
- Example: `_cli.WriteLnFormat("<i>Deploying...</i>");`


### Returning Failure Code to Machine

Every command inherits `-m` / `--machine` on `CmdBase` as `MachineMode`. Agents pass it on any subcommand; the flag is hidden from per-command help.

On **failure**, consumer `Program.cs` emits `FailureCode` on stdout when machine mode was requested — works even when the command never ran (e.g. unknown command).

On **success**, each command opts in: when `MachineMode` is true, call `_cli.WriteJsonData(...)` / `_cli.WriteTextData(...)`. Add machine output command-by-command; not required day one.

Example:

```bash
vxs domain info example.com --host-alias myhost -m
```

If the command fails:

```
Domain example.com is not set up on the host myhost.
DOMAIN_NOT_SETUP
```

- Exit code 1
- stderr: human message
- stdout: `DOMAIN_NOT_SETUP`

**Program.cs wiring** (pass `args` through unchanged; do not strip `-m`):

```csharp
var isMachineMode = CmdUtil.IsMachineRequest(args);

var builder = Host.CreateApplicationBuilder(args);

// Add Vexit CliEngine with custom name and CLI styling
builder.AddVexitCliEngine(options =>
{
    options.CliName = $"{AppInfo.Org} {AppInfo.DisplayName}";
    options.CliConfig = new CliConfig
    {
        LabelColor = ConsoleColor.White,
        InputColor = ConsoleColor.Green,
        ProgressMessageColor = ConsoleColor.DarkGray
    };
});

var app = builder.Build();

var result = await app.UseCliEngine(args);

if (result.IsFailure)
{
    if (result.HasMessage)
    {
        CliUtil.WriteLn();
        CliUtil.WriteLnError(result.Message!, CliUtil.GlobalLeftMargin);
    }

    if (isMachineMode && !string.IsNullOrWhiteSpace(result.FailureCode))
        CliUtil.WriteTextData(result.FailureCode);

    Environment.Exit(1);
}
Environment.Exit(0);
```

**In commands with machine success output:**

```csharp
public override async Task<Result> ExecuteAsync()
{
    if (MachineMode)
    {
        _cli.WriteJsonData(payload);
        return Ok();
    }

    _cli.WriteLn("Human table...");
    return Ok();
}
```


### Returning Structured Data to Machine

You can return structured data to machine by using `ICliService.WriteData` (stdout).

In this mock-up example of deploy command we are returning result of deployment as JSON object.

```csharp
using Vexit.CliEngine;
using Vexit.CliEngine.Attributes;
using Vexit.CliEngine.BaseClasses;
using Vexit.CliEngine.Enums;
using Vexit.Common.Models;

[Command("deploy", "Deploy the application")]
public class DeployCmd(ICliService cli) : CmdBase
{
    private readonly ICliService _cli = cli;

    public override async Task<Result> ExecuteAsync()
    {
        _cli.WriteLnDim("Starting deployment...");

        // Simulate deployment work
        await Task.Delay(1000);

        _cli.WriteLnSuccess("Deployment complete.");

        // Machine payload on stdout
        var payload = new { Status = "success", Version = "1.0.0" };
        _cli.WriteData(payload, DataFormatEnum.Json);

        return Ok();
    }
}
```


### **Machine-Readable Data (stdout) — Summary**

- Prefer `_cli.WriteJsonData` / `_cli.WriteTextData` (or `WriteData` with format) from injected `ICliService` in commands.
- Use `CliUtil.WriteTextData` / `WriteJsonData` in `Program.cs` when there is no injected service (e.g. `-m` failure codes).
- Writes to `Console.Out` (stdout) for easy piping in scripts/CI.
- Example: `_cli.WriteJsonData(deploymentResult)` outputs indented JSON.


### **Stream Separation Benefits**

- Scripts can pipe stdout safely: `mycli command | jq .field`
- Human messages don't pollute data streams
- Aligns with industry standards (kubectl, git, etc.)


## Failure Code Constants

Vexit.CliEngine provides centralized failure code constants for consistent error handling:

```csharp
using Vexit.CliEngine.Constants;

// In your commands (using CmdBase helper methods)
return Failure("Command not found", CommonFailureCodes.COMMAND_NOT_FOUND);
```

Available constants:
- `VALIDATION_ERROR`: For argument validation failures
- `UNKNOWN_COMMAND`: When command doesn't exist
- `COMMAND_NOT_FOUND`: When command type not found

This ensures consistent error codes across applications.


## Dual Execute Methods: Synchronous vs Asynchronous Commands

Vexit.CliEngine supports both synchronous and asynchronous commands through a dual-method system:


### **Synchronous Commands (95% of use cases)**

```csharp
[Command("sync-example", "A synchronous command")]
public class SyncCmd(ICliService cli) : CmdBase
{
    private readonly ICliService _cli = cli;

    public override Result Execute()
    {
        _cli.WriteLn("Processing synchronously...");
        // Your synchronous logic here
        return Ok();
    }
}
```


### **Asynchronous Commands (when you need await)**

```csharp
[Command("async-example", "An asynchronous command")]
public class AsyncCmd(ICliService cli) : CmdBase
{
    private readonly ICliService _cli = cli;

    public override async Task<Result> ExecuteAsync()
    {
        _cli.WriteLn("Starting async operation...");
        await Task.Delay(1000); // Async work
        _cli.WriteLn("Async operation complete!");
        return Ok();
    }
}
```


### **How It Works**

- **Framework Entry Point**: `ExecuteAsync()` - Always called by the framework
- **Default Behavior**: `ExecuteAsync()` calls `Execute()` for backward compatibility
- **Arguments**: Command arguments are handled declaratively via `[Option]` and `[Argument]` attributes on properties. The framework automatically parses command-line arguments and binds them to these properties before calling `Execute()`.
- **Override Strategy**:
  - Override `Execute()` for simple synchronous commands
  - Override `ExecuteAsync()` for commands needing `await`
- **Performance**: Synchronous commands avoid unnecessary `Task` allocation


## UTF-8 Encoding for Emojis

CLI tools often use emojis for better UX, but they require proper UTF-8 encoding to display correctly on Windows.

**Issue**: Emojis work under `dotnet run` (UTF-8 by default) but show `?` in compiled executables (inherit Windows OEM encoding).

**Solution**: Set UTF-8 encoding in your application's entry point:

```csharp
// In Program.cs, after using statements
Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;
```

This forces .NET to encode console output as UTF-8, ensuring emojis display consistently in development (`dotnet run`) and production (compiled exe) modes.


## Attribute System

- `CommandAttribute` - Marks command classes and defines command name, description, and optional short name
- `ArgumentAttribute` - Defines positional arguments
- `OptionAttribute` - Defines named options (flags and values)
- `AliasesAttribute` - Defines additional aliases for commands (repeatable)


## Current Type System

```csharp
// Positional argument (type inferred from property)
[Argument("name", "The name to greet", isRequired: true)]
public string Name { get; set; }

// Named option with short name
[Option("times", "t", "Number of times to repeat")]
public int Times { get; set; } = 1;

// Boolean flag (no value required)
[Option("verbose", "v", "Enable verbose output")]
public bool Verbose { get; set; }

// List argument (variadic - consumes remaining args)
[Argument("files", "Files to process")]
public List<string> Files { get; set; } = new();
```


## Argument Types

- **Arguments**: Positional parameters (no `--` or `-` prefix)
- **Options**: Named parameters with `--long` or `-short` syntax
- List/array properties consume remaining positional values (must be last argument)


## Standard CLI Conventions

- `--name` for long options
- `-n` for short options
- Quoted strings for multi-word values: `--name "John Doe"`
- `-name` is **invalid** (must be `--name` or `-n`)
- Boolean options don't require values: `--verbose` sets to `true`


## Positional Arguments

- **Left-to-right parsing**: `[command] [positional-args] [--named-args]`
- **Positional args first**, then named args (no mixing)
- **Property order defines position**: First property = Position 1, Second = Position 2, etc.
- **Named args override positional** if both provided for same parameter


### Examples:

```bash


# Positional only

mycli greet John                    # Position 1 → Name


# Named only

mycli greet --name John             # Named → Name


# Mixed (positional + named)

mycli greet John --times 3          # Position 1 → Name, Named → Times


# Named overrides positional

mycli greet positional --name named # Named wins → Name = "named"


# Invalid mixing (named before positional)

mycli greet --name John positional  # ❌ Not supported
```


### Property Order Mapping:

```csharp
[Command("greet")]
public class GreetCommand : CmdBase
{
    [Argument<string>("name")]     // Position 1 (first property)
    public string Name { get; set; }

    [Argument<int>("times")]       // Position 2 (second property)
    public int Times { get; set; }

    [Argument("world")]            // Named-only (no position)
    public bool World { get; set; }
}
```


## Validation System

- Required parameters validation
- Type validation (cannot pass string to bool flag)
- Custom validation messages via `ValidationMessage` property


## Command Examples

### Valid Usage

```bash


# Single name

mycli greet --name John


# Result: Hello, John


# Multiple names (concatenated)

mycli greet --name John --name Doe


# Result: Hello, John Doe


# Boolean flag only

mycli greet --world


# Result: Hello, World


# Mixed arguments

mycli greet --name John --world


# Result: Hello, John + Hello, World


# Repeat greeting multiple times

mycli greet --name John --times 3


# Result: Hello, John (printed 3 times)


# Combined with boolean flag

mycli greet --name John --world --times 2


# Result: Hello, John + Hello, World (each printed 2 times)

```


### Invalid Usage

```bash


# Invalid: missing required parameter

mycli greet


# Error: Name is required


# Invalid: wrong type for flag

mycli greet --world John


# Error: World is a boolean flag, cannot pass string value


# Invalid: wrong type for times parameter

mycli greet --name John --times notanumber


# Error: Times parameter must be a valid integer


# Invalid: wrong argument format

mycli greet -name John


# Error: -name is not a valid argument format

```


### Example Command Implementation

Here's the `GreetCmd` implementation that supports all the above usage patterns:

```csharp
using Vexit.CliEngine;
using Vexit.CliEngine.Attributes;
using Vexit.CliEngine.BaseClasses;
using Vexit.Common.Models;

namespace MyCli.Commands;

[Command("greet", "Greet someone with various options")]
public class GreetCmd(ICliService cli) : CmdBase
{
    private readonly ICliService _cli = cli;

    // Multi-value is inferred from List<string> (do not set IsMultiValue on the attribute — it is internal)
    [Option("name", "n", "Name(s) to greet")]
    public List<string> Names { get; set; } = new();

    [Option("world", "w", "Include world greeting")]
    public bool World { get; set; }

    [Option("times", "t", "Number of times to repeat each greeting")]
    public int Times { get; set; } = 1;

    public override Result Execute()
    {
        string fullName = string.Join(" ", Names);

        if (string.IsNullOrWhiteSpace(fullName) && !World)
        {
            return Failure("MISSING_GREETING_TARGET", "Either provide name(s) with --name or use --world flag");
        }

        if (!string.IsNullOrWhiteSpace(fullName))
        {
            for (int i = 0; i < Times; i++)
            {
                _cli.WriteLn($"Hello, {fullName}!");
            }
        }

        if (World)
        {
            for (int i = 0; i < Times; i++)
            {
                _cli.WriteLn("Hello, World!");
            }
        }

        return Ok();
    }
}
```

This command demonstrates:
- **Multi-value options**: `List<string>` property → framework sets multi-value collection semantics
- **Boolean flags**: `World` flag for additional greeting
- **Integer options**: `Times` for repetition
- **Validation**: Ensures at least one greeting target
- **ICliService**: `_cli.WriteLn()` for human stderr output


## Architecture Components

### 1. ArgumentAttribute (Positional)

```csharp
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class ArgumentAttribute : Attribute
{
    public string Name { get; }
    public string Description { get; }
    public bool IsRequired { get; set; }
    // IsMultiValue is internal — set automatically when property is IEnumerable (not string)
    public int MinCount { get; set; }
    public int MaxCount { get; set; }
    public string? ValidationMessage { get; set; }

    public ArgumentAttribute(string name, string description = "", bool isRequired = false, int minCount = 0, int maxCount = 0, string? validationMessage = null)
}
```


### 2. OptionAttribute (Named)

```csharp
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class OptionAttribute : Attribute
{
    public string LongName { get; }
    public string? ShortName { get; }
    public string Description { get; }
    public bool IsRequired { get; set; }
    // IsMultiValue is internal — set automatically when property is IEnumerable (not string)
    public int MinCount { get; set; }
    public int MaxCount { get; set; }
    public string? ValidationMessage { get; set; }

    public OptionAttribute(string longName, string? shortName = null, string description = "", bool isRequired = false, int minCount = 0, int maxCount = 0, string? validationMessage = null)
}
```


### 3. Parser Class

- Uses reflection to read command attributes
- Maps `--name` and `-n` to same property
- Handles multi-value logic
- Performs type conversion


### 4. Validator Class

- Validates required parameters are provided
- Validates argument types
- Provides custom error messages
- Stops processing on validation errors


### 5. ParsedArguments Class

- Holds parsed values and metadata
- Provides type-safe access to arguments
- Tracks validation errors


## Implementation Flow

```
Command Line Args → Parser → Validation → Property Assignment → Command Execution
```

1. **Parse**: Read command attributes, parse arguments, perform type conversion
2. **Validate**: Check required parameters, validate types, collect errors
3. **Assign**: Set command properties via reflection
4. **Execute**: Run command with populated properties


## Error Handling

### Validation Errors

- Missing required parameters
- Type mismatches
- Invalid argument formats
- Custom validation failures


### Error Messages

- Use `ValidationMessage` property when provided
- Generate descriptive default messages
- Stop processing on first validation error


## Feature Examples

### Type Inference

```csharp
[Option("count", "c", "Number of items")]
public int Count { get; set; } = 1;
```

```bash


# CLI usage

yourapp cmd --count 5


# Result: Count = 5 (auto-converted from string)

```
- Parser auto-converts string to int based on property type; validator errors on non-int.


### MultiValue Options

Multi-value is **inferred** from the property type (`IEnumerable` other than `string`). Do not set `IsMultiValue` on the attribute (it is internal).

```csharp
[Option("tag", "t", "Add a tag")]
public List<string> Tags { get; set; } = new();
```

```bash


# CLI usage

yourapp cmd --tag foo --tag bar


# Result: Tags = ["foo", "bar"]

```
- Can specify MinCount/MaxCount for validation.


### Variadic Positional Arguments

```csharp
[Argument("files", "Files to process")]
public List<string> Files { get; set; } = new();
```

```bash


# CLI usage

yourapp add foo.txt bar.txt baz.txt


# Result: Files = ["foo.txt", "bar.txt", "baz.txt"]

```
- **Important**: Variadic arguments must be the last positional argument.


### Custom ValidationMessage

```csharp
[Option("name", "n", "User name", ValidationMessage = "Name must be alphanumeric.")]
public string Name { get; set; } = string.Empty;
```

```bash


# CLI usage

yourapp cmd --name "invalid@name"


# Error: Name must be alphanumeric.

```
- Uses custom error on invalid input.


## Subcommands

Vexit.CliEngine supports hierarchical subcommands using a **convention-based** approach. Commands are organized by folder structure, and the library automatically infers the command hierarchy from your namespace.


### Convention-Based Command Discovery

The command path is automatically inferred from your namespace structure:

- **Namespace**: `YourApp.Commands.Secrets.InitCmd`
- **Command Path**: `yourapp secrets init`
- **With Aliases**: `yourapp s i` (using short names)


### Folder Structure Example

```
YourApp/
└── Commands/
    ├── GreetCmd.cs              # Root command: yourapp greet
    ├── New/
    │   ├── NewCmd.cs            # Group (defines aliases): yourapp new (n)
    │   └── ServerCmd.cs         # Subcommand: yourapp new server (s, srv)
    └── Secrets/
        ├── SecretsCmd.cs        # Group (defines aliases): yourapp secrets (s, sec)
        ├── InitCmd.cs           # Subcommand: yourapp secrets init (i)
        └── SetupCmd.cs          # Subcommand: yourapp secrets setup
```


### Creating Subcommands with Aliases

**Step 1: Create the group class (optional - for aliases and shared options)**

```csharp
using Vexit.CliEngine;

namespace YourApp.Commands.Secrets;

[Command("secrets", "Manage application secrets", shortName: "s")]
[Aliases("sec")]  // Additional aliases
public abstract class SecretsCmd : CmdGroupBase
{
    // Shared options for ALL secrets commands
    [Option("server-path", "p", "Path to the Server project root")]
    public string ServerPath { get; set; } = "MyApp/Server";
}
```

**Step 2: Create the command class**

```csharp
using Vexit.CliEngine;
using Vexit.CliEngine.Attributes;
using Vexit.CliEngine.BaseClasses;
using Vexit.Common.Models;

namespace YourApp.Commands.Secrets;

[Command("init", "Initialize secrets directory", shortName: "i")]
public class InitCmd(ICliService cli) : SecretsCmd  // Inherits ServerPath option
{
    private readonly ICliService _cli = cli;

    [Option("dir", "d", "Secrets directory name")]
    public string SecretsDir { get; set; } = "secrets";

    public override Result Execute()
    {
        _cli.WriteLn($"Initializing secrets at: {ServerPath}/{SecretsDir}");

        // Your implementation here

        _cli.WriteLn("Secrets initialized!");
        return Ok();
    }
}
```

**Step 3: Usage**

```bash


# Show available commands (with aliases)

yourapp --help


# Show secrets subcommands

yourapp secrets --help
yourapp s --help          # Using short name
yourapp sec --help        # Using alias


# Run the init command - all these work:

yourapp secrets init
yourapp s i                    # Short names
yourapp sec i                  # Mixed aliases
yourapp secrets i              # Mixed full/short


# With options (inherited ServerPath works!)

yourapp secrets init --server-path ./MyApp/Server --dir secrets
yourapp s i -p ./MyApp/Server -d secrets
```


### How It Works

1. **Namespace Parsing**: CliEngine extracts the path after `Commands.` in your namespace
   - `Commands.Secrets.InitCmd` → `["secrets", "init"]`

2. **Name Normalization**: Class names are converted to kebab-case and "Cmd" suffix is removed
   - `InitCmd` → `init`
   - `SetupCmd` → `setup`
   - `ManifestBuildCmd` → `manifest-build`

3. **Hierarchy Building**: Folders become command groups automatically
   - `Commands/Secrets/` → group
   - `Commands/Secrets/InitCmd.cs` → leaf command


### Command Aliases

**ShortName**: Single-letter alias (most common)
```csharp
[Command("secrets", "Manage secrets", shortName: "s")]
```

**Aliases**: Multiple additional aliases
```csharp
[Command("server", "Create server", shortName: "s")]
[Aliases("srv")]
[Aliases("backend")]  // Can have multiple Aliases attributes
```

**Group Aliases**: Define aliases for command groups
```csharp
[Command("secrets", "Manage secrets", shortName: "s")]
[Aliases("sec")]
public abstract class SecretsCmd : CmdGroupBase { }
```


### CmdGroupBase vs CmdBase

**CmdGroupBase**: For non-executable command groups
- Provides default Execute implementation (shows error message)
- CommandController automatically shows help instead of executing
- Perfect for organizing related commands and sharing options
- Leaf commands can inherit from group classes to get shared options

**CmdBase**: For executable leaf commands
- Must override Execute method with actual logic
- Arguments are automatically bound to properties via [Option] and [Argument] attributes
- Can be invoked directly
- Use when command performs an action (not just grouping)

**Example:**
```csharp
// Group - provides shared options, shows help when invoked
[Command("secrets", shortName: "s")]
public abstract class SecretsCmd : CmdGroupBase 
{
    // Shared options for ALL secrets commands
    [Option("server-path", "p", "Path to server")]
    public string ServerPath { get; set; } = "MyApp/Server";
    
    // No need to implement Execute - CmdGroupBase provides default
}

// Leaf - inherits shared options and implements actual logic
[Command("init", shortName: "i")]
public class InitCmd(ICliService cli) : SecretsCmd  // Inherits ServerPath option
{
    private readonly ICliService _cli = cli;

    [Option("dir", "d", "Secrets directory name")]
    public string SecretsDir { get; set; } = "secrets";
    
    public override Result Execute()
    {
        // ServerPath is available here from base class!
        _cli.WriteLn($"Initializing at: {ServerPath}/{SecretsDir}");
        return Ok();
    }
}
```

**How it works:**
1. `yourapp secrets` → CommandController detects it's a group → shows help (Execute never called)
2. `yourapp secrets init` → CommandController detects it's a leaf → creates InitCmd → calls Execute
3. `yourapp s i` → Same as above, but with aliases


### Help System

The help system automatically displays aliases and adapts to your command hierarchy:

```bash


# Root help - shows all top-level commands with aliases

yourapp --help


# Output shows: secrets (s, sec), new (n), greet


# Group help - shows subcommands with aliases

yourapp secrets --help
yourapp s --help        # Same help via alias


# Output shows: init (i), setup


# Command help - shows arguments and options

yourapp secrets init --help
yourapp s i --help      # Same help via aliases
```

**Help Output Example:**
```
Available commands:

  [group]   secrets (s, sec)      Manage application secrets
  [command] init (i)              Initialize secrets directory
  [command] setup                 Setup sovereign secrets
```


### Best Practices

1. **Keep hierarchy shallow** - 2-3 levels maximum (root → group → command)
2. **Use descriptive names** - Command names should be clear and intuitive
3. **Group related commands** - Put related functionality under the same folder
4. **Share common options** - Use base classes for options used across multiple commands
5. **Provide good descriptions** - Help users understand what each command does
6. **Use aliases** - Provide short names for frequently used commands
7. **Use Simple Folder-only Grouping** where possible. You don't need to create CmdGroup class if you don't need to share options across multiple commands. CliEngine will automatically create a group node for you based on the folder name.


### Flat vs Hierarchical Commands

You can mix both approaches in the same application:

- **Flat**: `Commands/GreetCmd.cs` → `yourapp greet` (no subcommands)
- **Hierarchical**: `Commands/Secrets/InitCmd.cs` → `yourapp secrets init` (subcommands)

The framework handles both seamlessly!

---


## StartCmd Default Command

Vexit.CliEngine supports a **default start command** that automatically executes when users run your CLI tool without any arguments. This provides a much more engaging first-time experience than showing help.


### How It Works

1. **Command Discovery**: When no arguments are provided (`args.Length == 0`), the framework checks for a command named `"start"`
2. **Automatic Execution**: If a `StartCmd` exists, it executes immediately
3. **Fallback**: If no start command exists, it shows the standard help menu (backward compatibility)


### Creating a StartCmd

Create a root-level command class named `StartCmd` in your `Commands` folder:

```csharp
using Vexit.CliEngine;
using Vexit.CliEngine.Attributes;
using Vexit.CliEngine.BaseClasses;
using Vexit.Common.Models;

namespace YourApp.Commands;

[Command("start", "Welcome dashboard and quick actions")]
public class StartCmd(ICliService cli) : CmdBase
{
    private readonly ICliService _cli = cli;

    public override Result Execute()
    {
        _cli.WriteLn("╔══════════════════════════════════════════════╗");
        _cli.WriteLn("║            Welcome to YourApp!              ║");
        _cli.WriteLn("╚══════════════════════════════════════════════╝");
        _cli.WriteLn();
        _cli.WriteLn("Quick Actions:");
        _cli.WriteLn("  yourapp init     - Initialize new project");
        _cli.WriteLn("  yourapp build    - Build application");
        _cli.WriteLn("  yourapp deploy   - Deploy to production");
        _cli.WriteLn();
        _cli.WriteLn("Use 'yourapp --help' for detailed help");

        return Ok();
    }
}
```


### User Experience

**Before**: `yourapp` → Shows dry help menu
**After**: `yourapp` → Shows engaging welcome dashboard


### Benefits

- **Better First Impression**: Users see a welcoming interface instead of raw help
- **Guided Experience**: Provides context and next steps
- **Flexible**: Can show status, available commands, or interactive prompts
- **Backward Compatible**: Falls back to help if no StartCmd exists


### Implementation Details

- **Location**: `Commands/StartCmd.cs` (root level, not in a subfolder)
- **Naming**: Must be named `StartCmd` and have `[Command("start")]` attribute
- **Execution**: Happens before argument parsing or validation
- **Help Access**: Users can still run `yourapp --help` to see detailed help
- **Version Access**: Users can run `yourapp -v` or `yourapp --version` to print the version from your `.csproj`

---


## Naming Guidance

- Use `*Cmd` suffix for executable command classes
- Use `*CmdGroup` suffix for non-executable command group classes (inherit from `CmdGroupBase`)
- Use `*Op` suffix for Ops
- Use `*ServiceGroup` for classes implementing `IServiceRegistry` (e.g., `EncryptionServiceGroup`)
- Use `*Service` for service implementations (e.g., `KeyVaultService`)
- Use `I*Service` for service interfaces (e.g., `IKeyVaultService`)
- Use `*Cli` for CLI interaction utilities (e.g., `PromptAppIdCli`)
- Use `*Validator` for validation utilities (e.g., `AppIdValidator`)

---


## Implementation Checklist: Command Lifecycle Hooks

Hooks are implemented in CliEngine (`IHook` / `HookBase`, `builder.AddHook<T>()`, executed by `CommandController`). See **Lifecycle Hooks** above.

Remaining optional polish:

- [ ] Add more hook examples alongside command examples (if needed)
- [ ] Expand unit tests for hook order / failure / non-blocking behavior


## Future Extensions

### Planned Features

- Custom type converters
- Auto-completion support
- Configuration file integration


### Compatibility

- Maintains backward compatibility with flat command structures
- Support both flat and hierarchical commands in the same application
- Gradual migration path for existing codebases


## History

| Date:      | Author | Description                                                                                                  |
| ---------- | ------ | ------------------------------------------------------------------------------------------------------------ |
| 2026-08-24 | Vex    | Machine mode on `CmdBase`; `CmdUtil.IsMachineRequest`; `WriteJsonData`/`WriteTextData`; removed argv strip |
| 2026-08-23 | Vex    | Doc pass: `ICliService` injection, `AddVexitCliEngine`, `Failure` arg order, multi-value inference, hooks APIs |
| 2026-08-23 | Vex    | Added `ICliService.WriteData` for injectable machine stdout                                                   |
| 2026-08-22 | Vex    | Documented `ProcessCliFlags` (`-m` / `--machine`), failure code on stdout, and `Program.cs` strip pattern    |
| 2026-08-17 | Vex    | Added built-in root `-v` / `--version` (consumer CLI version from `.csproj` `<Version>`)                     |
| 2026-01-30 | Vex    | Added CliService for consistent CLI output; replaced CliBase with injectable CliService architecture         |
| 2025-12-31 | Vex    | Added Formatting section (mixed-color XML tags, global margins); updated quick start to use CliBase wrappers |
| 2025-11-26 | Vex    | Added Lifecycle Hooks section (hooks, CommandContext, execution order)                                       |
| 2025-11-11 | Vex    | Added Overview/Features sections; documented Start page and ICommandExecutor                                 |
| 2025-11-11 | Vex    | Added StartCmd default execution when no arguments provided                                                  |
| 2025-11-03 | Vex    | Added dual Execute methods documentation; clarified CmdBase helper methods                                   |
| 2025-11-01 | Vex    | CliUtil global using alias `Cli`; UI vs machine-readable output split                                        |
| 2025-10-31 | Vex    | Updated for `Result<T>` returns, centralized error handling, and exit codes                                  |
| 2025-09-16 | Vex    | Initial README                                                                                               |










---

*© VEXIT ® 2025 | All rights reserved. | [www.vexit.com](https://www.vexit.com) | Tomorrow is today...®*

