/************************************************
 *
 * Copyright:		© 2025 VEXIT ®, www.vexit.com, Tomorrow is today...
 * Author:      	Vex Tatarevic
 * Date Created:    2026-01-30 - Injectable CLI service for consistent output
 * Date Updated:    2026-02-26 | Vex | Added PromptOptions and PromptOptions<T> methods for prompting the user to select one option from a list of string or object options.
 *                  2026-07-20 | Vex | Added PromptOptionsMulti and PromptOptionsMulti<T> wrapper methods.
 *                  2026-07-29 | Vex | Prompt defaultValue for editable input prefill.
 *                  2026-08-01 | Vex | Added PromptLabel wrapper - appends the ": " so callers and text constants don't have to.
 *                  2026-08-23 | Vex | Added WriteData wrapper for machine stdout
 *
 ************************************************/

using Vexit.CliEngine.Components;
using Vexit.CliEngine.Enums;
using Vexit.CliEngine.Utils;
using Vexit.Common.Models;

namespace Vexit.CliEngine;

/// <summary>
/// Injectable CLI service for consistent output formatting and user interaction. <br />
/// Provides the same functionality as CliBase but can be injected anywhere.
/// </summary>
public class CliService : ICliService
{
    private readonly CliConfig _config;

    // Cached global margin values (read once per instance)
    private readonly int _globalTopMargin = Cli.GlobalTopMargin;
    private readonly int _globalLeftMargin = Cli.GlobalLeftMargin;

    // Per-instance state for output tracking
    private bool _hasOutputStarted = false;

    /// <summary>
    /// Creates a new CLI service with the specified configuration.
    /// </summary>
    public CliService(CliConfig config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    /// <summary>
    /// Applies top margin before the first output of this instance
    /// </summary>
    private void ApplyTopMarginIfNeeded()
    {
        if (!_hasOutputStarted && _globalTopMargin > 0)
        {
            for (int i = 0; i < _globalTopMargin; i++)
            {
                // STDERR: human-facing margin, keep STDOUT clean for machine data
                Console.Error.WriteLine();
            }
            _hasOutputStarted = true;
        }
        else if (!_hasOutputStarted)
        {
            // Mark as started even with no top margin
            _hasOutputStarted = true;
        }
    }

    /// <summary>
    /// Writes a line with automatic top/left margin handling
    /// </summary>
    public void WriteLn(string text, int indent = 0)
    {
        ApplyTopMarginIfNeeded();
        Cli.WriteLn(text, indent + _globalLeftMargin);
    }

    /// <summary>
    /// <inheritdoc cref="Cli.WriteLnFormat"/>
    /// </summary>
    public void WriteLnFormat(string text, int indent = 0, ConsoleColor? mainColor = null)
    {
        ApplyTopMarginIfNeeded();
        Cli.WriteLnFormat(text, indent + _globalLeftMargin, mainColor);
    }

    /// <summary>
    /// Writes an empty line with automatic top/left margin handling
    /// </summary>
    public void WriteLn()
    {
        ApplyTopMarginIfNeeded();
        Cli.WriteLn("", 0 + _globalLeftMargin);
    }

    /// <summary>
    /// Writes a success message with automatic top/left margin handling
    /// </summary>
    public void WriteLnSuccess(string message, int indent = 0)
    {
        ApplyTopMarginIfNeeded();
        Cli.WriteLnSuccess(message, indent + _globalLeftMargin);
    }

    /// <summary>
    /// Writes an error message with automatic top/left margin handling
    /// </summary>
    public void WriteLnError(string message, int indent = 0)
    {
        ApplyTopMarginIfNeeded();
        Cli.WriteLnError(message, indent + _globalLeftMargin);
    }

    /// <summary>
    /// Writes a warning message with automatic top/left margin handling
    /// </summary>
    public void WriteLnWarning(string message, int indent = 0)
    {
        ApplyTopMarginIfNeeded();
        Cli.WriteLnWarning(message, indent + _globalLeftMargin);
    }

    /// <summary>
    /// Writes an info message with automatic top/left margin handling
    /// </summary>
    public void WriteLnInfo(string message, int indent = 0)
    {
        ApplyTopMarginIfNeeded();
        Cli.WriteLnInfo(message, indent + _globalLeftMargin);
    }

    /// <summary>
    /// Writes dimmed text with automatic top/left margin handling
    /// </summary>
    public void WriteLnDim(string message, int indent = 0)
    {
        ApplyTopMarginIfNeeded();
        Cli.WriteLnDim(message, indent + _globalLeftMargin);
    }

    /// <summary>
    /// Writes lite text with automatic top/left margin handling
    /// </summary>
    public void WriteLnLite(string message, int indent = 0)
    {
        ApplyTopMarginIfNeeded();
        Cli.WriteLnLite(message, indent + _globalLeftMargin);
    }

    
    public void WriteLnCode(string message, int indent = 0)
    {
        ApplyTopMarginIfNeeded();
        Cli.WriteLnCode(message, indent + _globalLeftMargin);
    }

    public void WriteFormat(string text, int indent = 0, ConsoleColor? mainColor = null)
    {
        ApplyTopMarginIfNeeded();
        Cli.WriteFormat(text, indent + _globalLeftMargin, mainColor);
    }

    /// <summary>
    /// Used for writing to stdout stream read by machine processes like AI agents or automation scripts. <br />
    /// It can write structured data objects as json (useful for returning success payloads) or plain text (useful for returning error codes). <br />
    /// No left margin — payload must stay clean for piping and agents.
    /// </summary>
    public void WriteData<T>(T data, DataFormatEnum format = DataFormatEnum.Json)
    {
        Cli.WriteData(data, format);
    }

    public void WriteJsonData<T>(T data) => Cli.WriteJsonData(data);

    public void WriteTextData<T>(T data) => Cli.WriteTextData(data);

    public void Write(string text, int indent = 0)
    {
        Cli.Write(text, null, false, indent + _globalLeftMargin);
    }

    public void WriteDim(string text, int indent = 0)
    {
        Cli.WriteDim(text, indent + _globalLeftMargin);
    }

    public void WriteLabel(string text, int indent = 0)
    {
        ApplyTopMarginIfNeeded();
        Cli.WriteLabel(text, indent + _globalLeftMargin, _config.LabelColor);
    }

    /// <summary>
    /// Reads user input from console
    /// </summary>
    public string ReadInput(bool masked = false)
    {
        return Cli.ReadInput(_config.InputColor, masked);
    }

    /// <summary>
    /// Writes the prompt text and reads a line of input from the user.
    /// </summary>
    /// <param name="message">The prompt to display (e.g. "Enter domain name (e.g. mysite.com): ").</param>
    /// <param name="masked">If true, masks input (e.g. for passwords).</param>
    /// <param name="promptColor">Color for the prompt text; defaults to LabelColor when null.</param>
    /// <param name="inputColor">Color for the user's input as they type; defaults to InputColor when null.</param>
    /// <param name="defaultValue">Optional editable prefill in the input buffer (ignored when masked).</param>
    /// <returns>The trimmed user input.</returns>
    public string Prompt(
        string message,
        bool masked = false,
        ConsoleColor? promptColor = null,
        ConsoleColor? inputColor = null,
        string? defaultValue = null)
    {
        ApplyTopMarginIfNeeded();
        return Cli.Prompt(message, promptColor ?? _config.LabelColor, inputColor ?? _config.InputColor, masked, _globalLeftMargin, defaultValue);
    }

    /// <summary>
    /// Writes a bare label with the label separator appended and reads a line of input from the user.
    /// </summary>
    /// <param name="label">Label without trailing separator (e.g. "Domain name").</param>
    /// <param name="masked">If true, masks input (e.g. for passwords).</param>
    /// <param name="promptColor">Color for the prompt text; defaults to LabelColor when null.</param>
    /// <param name="inputColor">Color for the user's input as they type; defaults to InputColor when null.</param>
    /// <param name="defaultValue">Optional editable prefill in the input buffer (ignored when masked).</param>
    /// <returns>The trimmed user input.</returns>
    public string PromptLabel(
        string label,
        bool masked = false,
        ConsoleColor? promptColor = null,
        ConsoleColor? inputColor = null,
        string? defaultValue = null)
    {
        ApplyTopMarginIfNeeded();
        return Cli.PromptLabel(label, promptColor ?? _config.LabelColor, inputColor ?? _config.InputColor, masked, _globalLeftMargin, defaultValue);
    }

    /// <summary>
    /// Prompts user with yes/no question
    /// </summary>
    public bool PromptYesNo(string message, bool defaultValue = false, ConsoleColor? promptColor = null, int indent = 0)
    {
        ApplyTopMarginIfNeeded();
        return Cli.PromptYesNo(message, defaultValue, promptColor ?? _config.LabelColor, indent + _globalLeftMargin);
    }

    /// <summary>
    /// Prompts user to select one option from a list of objects.
    /// </summary>
    /// <typeparam name="T">The type of objects in the list.</typeparam>
    /// <param name="prompt">The prompt text to display.</param>
    /// <param name="options">The list of objects to choose from.</param>
    /// <param name="displaySelector">Function to get the display string for each object.</param>
    /// <param name="promptColor">Color for the prompt text; defaults to LabelColor when null.</param>
    /// <param name="optionsColor">Color for the numbered options list.</param>
    /// <param name="inputColor">Color for the user's input when typing their choice.</param>
    /// <returns>Result with selected object, or failure if canceled.</returns>
    public Result<T> PromptOptions<T>(string prompt, IReadOnlyList<T> options, Func<T, string> displaySelector, ConsoleColor? promptColor = null, ConsoleColor? optionsColor = null, ConsoleColor? inputColor = null)
    {
        ApplyTopMarginIfNeeded();
        return Cli.PromptOptions(prompt, options, displaySelector, promptColor ?? _config.LabelColor, optionsColor ?? _config.OptionsColor, inputColor ?? _config.InputColor, _globalLeftMargin);
    }

    /// <summary>
    /// Prompts user to select one option from a list of string options.
    /// </summary>
    /// <param name="prompt">The prompt text to display.</param>
    /// <param name="options">The list of string options to choose from.</param>
    /// <param name="promptColor">Color for the prompt text; defaults to LabelColor when null.</param>
    /// <param name="optionsColor">Color for the numbered options list.</param>
    /// <param name="inputColor">Color for the user's input when typing their choice.</param>
    /// <returns>Result with selected option string, or failure if canceled.</returns>
    public Result<string> PromptOptions(string prompt, IReadOnlyList<string> options, ConsoleColor? promptColor = null, ConsoleColor? optionsColor = null, ConsoleColor? inputColor = null)
    {
        ApplyTopMarginIfNeeded();
        return Cli.PromptOptions(prompt, options, promptColor ?? _config.LabelColor, optionsColor ?? _config.OptionsColor, inputColor ?? _config.InputColor, _globalLeftMargin);
    }

    /// <summary>
    /// Prompts user to select one or more options from a numbered list.
    /// </summary>
    /// <param name="prompt">The prompt text to display.</param>
    /// <param name="options">The list of string options to choose from.</param>
    /// <param name="promptColor">Color for the prompt text; defaults to LabelColor when null.</param>
    /// <param name="optionsColor">Color for the numbered options list.</param>
    /// <param name="inputColor">Color for the user's input when typing their choice.</param>
    /// <returns>Result with selected option strings (possibly empty).</returns>
    public Result<IReadOnlyList<string>> PromptOptionsMulti(
        string prompt,
        IReadOnlyList<string> options,
        ConsoleColor? promptColor = null,
        ConsoleColor? optionsColor = null,
        ConsoleColor? inputColor = null)
    {
        ApplyTopMarginIfNeeded();
        return Cli.PromptOptionsMulti(
            prompt,
            options,
            promptColor ?? _config.LabelColor,
            optionsColor ?? _config.OptionsColor,
            inputColor ?? _config.InputColor,
            _globalLeftMargin);
    }

    /// <summary>
    /// Prompts user to select one or more objects from a numbered list.
    /// </summary>
    /// <typeparam name="T">The type of objects in the list.</typeparam>
    /// <param name="prompt">The prompt text to display.</param>
    /// <param name="options">The list of objects to choose from.</param>
    /// <param name="displaySelector">Function to get the display string for each object.</param>
    /// <param name="promptColor">Color for the prompt text; defaults to LabelColor when null.</param>
    /// <param name="optionsColor">Color for the numbered options list.</param>
    /// <param name="inputColor">Color for the user's input when typing their choice.</param>
    /// <returns>Result with selected objects (possibly empty).</returns>
    public Result<IReadOnlyList<T>> PromptOptionsMulti<T>(
        string prompt,
        IReadOnlyList<T> options,
        Func<T, string> displaySelector,
        ConsoleColor? promptColor = null,
        ConsoleColor? optionsColor = null,
        ConsoleColor? inputColor = null)
    {
        ApplyTopMarginIfNeeded();
        return Cli.PromptOptionsMulti(
            prompt,
            options,
            displaySelector,
            promptColor ?? _config.LabelColor,
            optionsColor ?? _config.OptionsColor,
            inputColor ?? _config.InputColor,
            _globalLeftMargin);
    }

    /// <summary>
    /// Shows progress message with spinner and executes work asynchronously
    /// </summary>
    public async Task<Result> WriteProgressMessageAsync(
        string progressMessage,
        Func<Task<Result>> work,
        string? successMessage = null,
        string? errorMessage = null,
        ConsoleColor? successColor = null,
        ConsoleColor? errorColor = null,
        ProgressMessage.Animation animation = ProgressMessage.Animation.SpinnerPipe,
        int? totalSteps = null,
        int? currentStep = null,
        CancellationToken token = default)
    {
        ApplyTopMarginIfNeeded();
        return await ProgressMessage.ShowAsync(
            progressMessage, work, successMessage, errorMessage,
            _globalLeftMargin,
            _config.ProgressMessageColor,
            successColor, errorColor, animation, totalSteps, currentStep, token);
    }
}