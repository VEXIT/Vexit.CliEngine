# Proposed Features: Theme Service & CLI Service Architecture

|              |                                                       |
| ------------ | ----------------------------------------------------- |
| Copyright    | © VEXIT ® 2025 , www.vexit.com , Tomorrow is today... |
| Author       | Vex Tatarevic                                         |
| Date Created | 2025-01-15                                            |
| Date Updated | 2025-01-15                                            |

---

## Overview
This document outlines the proposed theming system for Vexit.CliEngine, enabling consumer applications to define and apply global CLI output themes consistently across all commands.

## Core Interfaces & Classes

### ICliTheme Interface
```csharp
public interface ICliTheme
{
    ConsoleColor InfoColor { get; }
    ConsoleColor WarningColor { get; }
    ConsoleColor ErrorColor { get; }
    ConsoleColor SuccessColor { get; }
    ConsoleColor DimColor { get; }
    ConsoleColor LiteColor { get; }
    ConsoleColor InputColor { get; }
    ConsoleColor LabelColor { get; }
    ConsoleColor ProgressMessageColor { get; }
}
```

### CliTheme Implementation
```csharp
public class CliTheme : ICliTheme
{
    public CliTheme() { /* Default theme */ }
    public CliTheme(string themePath, string themeName) { /* Load from file */ }

    // Implement all ICliTheme properties
    public ConsoleColor InfoColor { get; set; } = ConsoleColor.Cyan;
    public ConsoleColor WarningColor { get; set; } = ConsoleColor.Yellow;
    // ... other properties
}
```

### ICli Interface
```csharp
public interface ICli
{
    // All CliUtil wrapper methods
    void WriteLn(string message, int indent = 0);
    void WriteLnSuccess(string message, int indent = 0);
    void WriteLnError(string message, int indent = 0);
    void WriteLnWarning(string message, int indent = 0);
    void WriteLnInfo(string message, int indent = 0);
    void WriteLnDim(string message, int indent = 0);
    void WriteLnLite(string message, int indent = 0);
    void WriteLnFormat(string format, int indent = 0);
    // ... all other output methods
}
```

### CliBase Abstract Class
```csharp
public abstract class CliBase : ICli
{
    protected readonly ICliTheme _theme;

    protected CliBase(ICliTheme theme)
    {
        _theme = theme;
    }

    // Implement all ICli methods using CliUtil with theme colors
    public void WriteLnSuccess(string message, int indent = 0)
    {
        CliUtil.WriteLnSuccess(message, indent, _theme.SuccessColor);
    }

    // ... implement all other methods
}
```

### DefaultCli Implementation
```csharp
public class DefaultCli : CliBase
{
    public DefaultCli(ICliTheme theme) : base(theme) { }

    // All methods inherited from CliBase
}
```

## Configuration Architecture

### CliEngineOptions Extension
```csharp
public class CliEngineOptions
{
    // Existing properties...
    public string CliName { get; set; }

    // New: Theme configuration
    public ICliTheme? Theme { get; set; }
}
```

### AddCliService Extension Method
```csharp
public static class CliServiceExtensions
{
    public static IHostApplicationBuilder AddCliService<TCli>(
        this IHostApplicationBuilder builder,
        Func<IServiceProvider, TCli> factory)
        where TCli : class, ICli
    {
        builder.Services.AddSingleton<ICli>(factory);
        return builder;
    }
}
```

### CliEngine Auto-Registration
```csharp
public static IHostApplicationBuilder AddCliEngine(
    this IHostApplicationBuilder builder,
    Action<CliEngineOptions> configure)
{
    var options = new CliEngineOptions();
    configure(options);

    // Register options as service
    builder.Services.AddSingleton(options);

    // Register theme if provided
    if (options.Theme != null)
    {
        builder.Services.AddSingleton(options.Theme);
    }

    // Auto-register default Cli if theme provided and no custom Cli registered
    builder.Services.TryAddSingleton<ICli>(sp => {
        var theme = sp.GetRequiredService<CliEngineOptions>().Theme;
        return theme != null ? new DefaultCli(theme) : null;
    });

    return builder;
}
```

## Usage Patterns

### Simple Usage (Default Theme)
```csharp
builder.AddCliEngine(options => {
    options.CliName = $"{AppInfo.Org} {AppInfo.CliName}";
    options.Theme = new CliTheme(); // Use default theme
});
// ICli automatically registered with DefaultCli
```

### Custom Theme from File
```csharp
builder.AddCliEngine(options => {
    options.CliName = $"{AppInfo.Org} {AppInfo.CliName}";
    options.Theme = new CliTheme("~/.vexit/themes", "dark");
});
// ICli automatically registered with DefaultCli using custom theme
```

### Custom CLI Implementation
```csharp
public class CustomCli : CliBase
{
    public CustomCli(ICliTheme theme, IOtherDependency dep)
        : base(theme) { /* custom logic */ }
}

builder.AddCliEngine(options => {
    options.Theme = new CliTheme();
})
.AddCliService(sp => {
    var theme = sp.GetRequiredService<CliEngineOptions>().Theme;
    var dep = sp.GetRequiredService<IOtherDependency>();
    return new CustomCli(theme, dep);
});
```

## Command Implementation

### Themed Command (Opt-in)
```csharp
public class ThemedCmd : CmdBase
{
    private readonly ICli _cli;

    public ThemedCmd(ICli cli)
    {
        _cli = cli;
    }

    public override async Task<Result> ExecuteAsync()
    {
        _cli.WriteLnSuccess("Operation completed!");
        _cli.WriteLnError("Something went wrong");
        return Result.Success();
    }
}
```

### Plain Command (Opt-out)
```csharp
public class PlainCmd : CmdBase
{
    public override async Task<Result> ExecuteAsync()
    {
        Console.WriteLine("Plain output - no theming");
        return Result.Success();
    }
}
```

## Benefits

1. **Flexible**: Commands opt-in to theming by injecting ICli
2. **Clean**: Clear separation between themed and plain output
3. **Extensible**: Consumer apps can provide custom Cli implementations
4. **Backwards Compatible**: Existing commands continue to work
5. **Auto-Registration**: Sensible defaults when theme is provided
6. **Dependency Injection**: Full DI support for custom implementations

## Migration Path

1. Add ICliTheme, CliTheme, ICli, CliBase, DefaultCli to CliEngine
2. Extend CliEngineOptions with Theme property
3. Add AddCliService extension method
4. Update AddCliEngine to auto-register ICli
5. Update existing commands to inject ICli (optional)
6. Consumer apps can configure themes as needed

---

*© VEXIT ® 2025 | All rights reserved. | [www.vexit.com](https://www.vexit.com) | Tomorrow is today...®*