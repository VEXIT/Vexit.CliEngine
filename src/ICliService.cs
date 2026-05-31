/************************************************
 *
 * Copyright:		© 2025 VEXIT ®, www.vexit.com
 * Author:      	Vex Tatarevic
 * Date Created:    2026-03-12 - Interface for testability and abstraction
 *
 ************************************************/

using Vexit.CliEngine.Components;
using Vexit.Common.Models;

namespace Vexit.CliEngine;

public interface ICliService
{
    /// <inheritdoc cref="Cli.WriteLn"/>
    void WriteLn(string text, int indent = 0);
    /// <inheritdoc cref="Cli.WriteLnFormat"/>
    void WriteLnFormat(string text, int indent = 0, ConsoleColor? mainColor = null);
    /// <inheritdoc cref="Cli.WriteLn"/>
    void WriteLn();
    /// <inheritdoc cref="Cli.WriteLnSuccess"/>
    void WriteLnSuccess(string message, int indent = 0);
    /// <inheritdoc cref="Cli.WriteLnError"/>
    void WriteLnError(string message, int indent = 0);
    /// <inheritdoc cref="Cli.WriteLnWarning"/>
    void WriteLnWarning(string message, int indent = 0);
    /// <inheritdoc cref="Cli.WriteLnInfo"/>
    void WriteLnInfo(string message, int indent = 0);
    /// <inheritdoc cref="Cli.WriteLnDim"/>
    void WriteLnDim(string message, int indent = 0);
    /// <inheritdoc cref="Cli.WriteLnLite"/>
    void WriteLnLite(string message, int indent = 0);
    /// <inheritdoc cref="Cli.WriteLnCode"/>
    void WriteLnCode(string message, int indent = 0);
    
    /// <inheritdoc cref="Cli.WriteFormat"/>
    void WriteFormat(string text, int indent = 0, ConsoleColor? mainColor = null);
    /// <inheritdoc cref="Cli.Write"/>
    void Write(string text, int indent = 0);
    /// <inheritdoc cref="Cli.WriteDim"/>
    void WriteDim(string text, int indent = 0);
    /// <inheritdoc cref="Cli.WriteLabel"/>
    void WriteLabel(string text, int indent = 0);
    /// <inheritdoc cref="Cli.ReadInput"/>
    string ReadInput(bool masked = false);
    /// <inheritdoc cref="Cli.Prompt"/>
    string Prompt(string message, bool masked = false, ConsoleColor? promptColor = null, ConsoleColor? inputColor = null);
    /// <inheritdoc cref="Cli.PromptYesNo"/>
    bool PromptYesNo(string message, bool defaultValue = false, ConsoleColor? promptColor = null);
    
    /// <inheritdoc cref="Cli.PromptOptions"/>
    Result<T> PromptOptions<T>(string prompt, IReadOnlyList<T> options, Func<T, string> displaySelector, ConsoleColor? promptColor = null, ConsoleColor? optionsColor = null, ConsoleColor? inputColor = null);
    /// <inheritdoc cref="Cli.PromptOptions"/>
    Result<string> PromptOptions(string prompt, IReadOnlyList<string> options, ConsoleColor? promptColor = null, ConsoleColor? optionsColor = null, ConsoleColor? inputColor = null);
    Task<Result> WriteProgressMessageAsync(string progressMessage, Func<Task<Result>> work, string? successMessage = null, string? errorMessage = null, ConsoleColor? successColor = null, ConsoleColor? errorColor = null, ProgressMessage.Animation animation = ProgressMessage.Animation.SpinnerPipe, int? totalSteps = null, int? currentStep = null, CancellationToken token = default);
}
