/*************************************************************
 * 
 *  Copyright    : © VEXIT 2025, www.vexit.com  
 *  Author       : Vex Tatarevic
 *  Date Created : 2025-06-20 - Created CliUtil class for command line interface utility methods
 *  Date Updated : 2025-11-01 | Vex | Moved from Vexit.Common.Utils to Vexit.CliEngine.Utils... Implemented stderr for UX output, added WriteData for machine-readable data
 *                 2025-12-22 | Vex | Added WriteFormat and WriteLnFormat methods for XML-like formatting, e.g. <i>text</i> for info, <w>text</w> for warning, <e>text</e> for error, <s>text</s> for success, <d>text</d> for dim, <l>text</l> for lite.
 *                 2026-01-13 | Vex | Added GlobalTopMargin and GlobalLeftMargin properties for global CLI margin settings, read from environment variables.
 *                 2026-02-26 | Vex | Added PromptOptions and PromptOptions<T> methods for prompting the user to select one option from a list of string or object options.
 *                 2026-06-17 | Vex | Updated WriteLn and WriteFormat - now writing to STDERR, keeping STDOUT clean for machine data.
 *                 2026-07-20 | Vex | Added PromptOptionsMulti and PromptOptionsMulti<T> for multi-select numbered options (e.g. 1,2 or 1-3).
 *                 2026-07-29 | Vex | ReadLn caret navigation (arrows/Home/End/Delete) and optional initialValue; Prompt defaultValue prefill.
 *                 2026-07-29 | Vex | ReadLn single-line only: discard pending console keys after Enter (blocks multi-line paste flood).
 *                 2026-08-01 | Vex | Added PromptLabel wrapper - appends the ": " so callers and text constants don't have to.
 *                 2026-08-24 | Vex | Added WriteJsonData / WriteTextData overloads for machine stdout.
 *
 ************************************************************/


using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Vexit.CliEngine.Enums;
using Vexit.Common.Models;

namespace Vexit.CliEngine.Utils;

/// <summary>
/// Command line interface utility methods
/// </summary>
public static class CliUtil
{

    /// <summary>
    /// Static class containing context color constants
    /// </summary>
    public static class Color
    {
        // Contextual Colors
        public const ConsoleColor Success = ConsoleColor.DarkGreen;
        public const ConsoleColor Error = ConsoleColor.Red;
        public const ConsoleColor Warning = ConsoleColor.Yellow;
        public const ConsoleColor Info = ConsoleColor.Cyan;
        public const ConsoleColor Code = ConsoleColor.DarkGreen;
        public const ConsoleColor Primary = ConsoleColor.White;
        public const ConsoleColor Secondary = ConsoleColor.DarkGray;
        // Formatting Colors
        public const ConsoleColor Dim = ConsoleColor.DarkGray;
        public const ConsoleColor Lite = ConsoleColor.Gray;

    }

    // Global CLI margin settings (read from environment variables)
    public static readonly int GlobalTopMargin = GetEnvInt("VEXIT_CLI_TOP_MARGIN", 0);
    public static readonly int GlobalLeftMargin = GetEnvInt("VEXIT_CLI_LEFT_MARGIN", 0);

    private static int GetEnvInt(string name, int defaultValue)
    {
        var value = Environment.GetEnvironmentVariable(name);
        return int.TryParse(value, out var result) ? result : defaultValue;
    }

    //--------------------------------
    // Write contextual with new line
    //--------------------------------
    public static void WriteLnSuccess(string message, int indent = 0)
    {
        WriteSuccess(message, indent, true);
    }
    public static void WriteLnError(string message, int indent = 0)
    {
        WriteError(message, indent, true);
    }
    public static void WriteLnWarning(string message, int indent = 0)
    {
        WriteWarning(message, indent, true);
    }
    public static void WriteLnInfo(string message, int indent = 0)
    {
        WriteInfo(message, indent, true);
    }
    public static void WriteLnDim(string message, int indent = 0)
    {
        WriteDim(message, indent, true);
    }
    public static void WriteLnLite(string message, int indent = 0)
    {
        WriteLite(message, indent, true);
    }
    public static void WriteLnCode(string message, int indent = 0)
    {
        WriteCode(message, indent, true);
    }

    //-----------------------------------------
    // Write contextual with new line optional (default is false)    
    //--------------------------------
    public static void WriteSuccess(string message, int indent = 0, bool newLine = false)
    {
        Write(message, Color.Success, newLine, indent);
    }
    public static void WriteError(string message, int indent = 0, bool newLine = false)
    {
        Write(message, Color.Error, newLine, indent);
    }
    public static void WriteWarning(string message, int indent = 0, bool newLine = false)
    {
        Write(message, Color.Warning, newLine, indent);
    }
    public static void WriteInfo(string message, int indent = 0, bool newLine = false)
    {
        Write(message, Color.Info, newLine, indent);
    }
    public static void WriteDim(string message, int indent = 0, bool newLine = false)
    {
        Write(message, Color.Dim, newLine, indent);
    }
    public static void WriteLite(string message, int indent = 0, bool newLine = false)
    {
        Write(message, Color.Lite, newLine, indent);
    }
    public static void WriteCode(string message, int indent = 0, bool newLine = false)
    {
        Write(message, Color.Code, newLine, indent);
    }

    public static void WriteLabel(string message, int indent = 0, ConsoleColor color = Color.Primary)
    {
        WriteFormat(message, false, indent, color);
    }


    /// <summary>
    /// Writes a new line to the console
    /// </summary>
    public static void WriteLn() { Console.Error.WriteLine(); }
    public static void WriteLn(string text, ConsoleColor color, int indent = 0) { Write(text, color, true, indent); }
    public static void WriteLn(string text, int indent = 0) { Write(text, null, true, indent); }

    /// <summary>
    /// Writes formatted text with contextual colors using XML-like tags. <br />
    /// Supported tags: <br />
    ///  &lt; i&gt;text&lt;/ i&gt; (info/cyan) <br />
    ///  &lt; w&gt;text&lt;/ w&gt; (warning/yellow) <br />
    ///  &lt; e&gt;text&lt;/ e&gt; (error/red) <br />
    ///  &lt; s&gt;text&lt;/ s&gt; (success/darkgreen) <br />
    ///  &lt; d&gt;text&lt;/ d&gt; (dim/darkgray) <br />
    ///  &lt; l&gt;text&lt;/ l&gt; (lite/gray) <br />
    ///  &lt; c&gt;text&lt;/ c&gt; (code/darkgreen) <br />
    /// Text outside tags uses default color.
    /// </summary>
    /// <param name="text">Text with XML-like formatting tags</param>
    /// <param name="indent">Number of spaces to indent</param>
    public static void WriteLnFormat(string text, int indent = 0, ConsoleColor? mainColor = null)
    {
        WriteFormat(text, true, indent, mainColor);
    }

    /// <summary>
    /// Writes formatted text with contextual colors using XML-like tags (without newline). <br />
    /// See <see cref="WriteLnFormat"/> for supported tags.
    /// </summary>
    /// <param name="text">Text with XML-like formatting tags</param>
    /// <param name="indent">Number of spaces to indent</param>
    public static void WriteFormat(string text, int indent = 0, ConsoleColor? mainColor = null)
    {
        WriteFormat(text, false, indent, mainColor ?? Color.Primary);
    }

    private static void WriteFormat(string text, bool newLine, int indent = 0, ConsoleColor? mainColor = ConsoleColor.White)
    {
        // Add indent spaces first (STDERR: human-facing, keep STDOUT clean for machine data)
        for (int i = indent; i-- > 0;)
        {
            Console.Error.Write(" ");
        }

        // Parse XML-like tags and apply colors
        var regex = new Regex(@"<(?<tag>[iIwWeEsSdDlLcC])>(?<content>.*?)</\k<tag>>", RegexOptions.IgnoreCase);
        var lastIndex = 0;

        foreach (Match match in regex.Matches(text))
        {
            // Write text before the tag
            if (match.Index > lastIndex)
            {
                var plainText = text.Substring(lastIndex, match.Index - lastIndex);
                Write(plainText, mainColor, false);
            }

            // Write tagged content with appropriate color
            var tag = match.Groups["tag"].Value.ToLower();
            var content = match.Groups["content"].Value;

            ConsoleColor? color = tag switch
            {
                "i" => Color.Info,      // Info (Cyan)
                "w" => Color.Warning,   // Warning (Yellow)
                "e" => Color.Error,     // Error (Red)
                "s" => Color.Success,   // Success (DarkGreen)
                "d" => Color.Dim,       // Dim (DarkGray)
                "l" => Color.Lite,      // Lite (Gray)
                "c" => Color.Code,      // Code (DarkGreen)
                _ => mainColor,         // Default (White)
            };

            Write(content, color, false);

            lastIndex = match.Index + match.Length;
        }

        // Write remaining text after last tag
        if (lastIndex < text.Length)
        {
            var remainingText = text.Substring(lastIndex);
            Write(remainingText, mainColor, false);
        }

        if (newLine)
        {
            // STDERR: human-facing newline, keep STDOUT clean for machine data
            Console.Error.WriteLine();
        }

        Console.ResetColor();
    }

    public static void Write(string text, ConsoleColor? color, bool newLine = false, int indent = 0)
    {
        var sb = new StringBuilder();

        // Add indent spaces
        for (int i = indent; i-- > 0;)
        {
            sb.Append(" ");
        }

        sb.Append(text);

        if (newLine)
        {
            sb.Append("\n");
        }

        if (color.HasValue)
        {
            Console.ForegroundColor = color.Value;
        }

        // STDERR: Write to stderr for human-facing output
        Console.Error.Write(sb.ToString());
        Console.ResetColor();
    }
    public static void Write(string text) { Console.Error.Write(text); }

    /// <summary>
    /// Used for writing to stdout stream read by machine processes like AI agents or automation scripts. <br />
    /// It can write structured data objects as json (useful for returning success payloads) or plain text (useful for returning error codes).
    /// </summary>
    /// <typeparam name="T">Type of data to serialize</typeparam>
    /// <param name="data">Data object to serialize and write</param>
    /// <param name="format">Output format (Json by default)</param>
    public static void WriteData<T>(T data, DataFormatEnum format = DataFormatEnum.Json)
    {
        string output;
        switch (format)
        {
            case DataFormatEnum.Json:
                output = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
                break;
            case DataFormatEnum.Text:
            default:
                output = data?.ToString() ?? string.Empty;
                break;
        }
        // STDOUT: Write to stdout for machine-readable data
        Console.Out.WriteLine(output);
    }

    /// <summary>Writes <paramref name="data"/> as indented JSON on stdout.</summary>
    public static void WriteJsonData<T>(T data) => WriteData(data, DataFormatEnum.Json);

    /// <summary>Writes <paramref name="data"/> as plain text on stdout (e.g. failure codes).</summary>
    public static void WriteTextData<T>(T data) => WriteData(data, DataFormatEnum.Text);

    /// <summary>
    /// Reads a single line of input with optional prefill and caret editing.<br />
    /// Supports Left/Right arrows, Home/End, Delete, Backspace, and insert-at-caret.<br />
    /// Enter submits the line and discards any further keys already queued (e.g. rest of a multi-line paste)<br />
    /// so leftover lines cannot flood the next prompt. Multi-line capture is a separate API when needed.<br />
    /// Echoes to STDERR so STDOUT stays clean for machine-readable data.
    /// </summary>
    /// <param name="color">Foreground color for echoed input characters.</param>
    /// <param name="masked">
    /// If true, masks input with asterisks (e.g. passwords).<br />
    /// Prefill via <paramref name="initialValue"/> is ignored when masked.
    /// </param>
    /// <param name="initialValue">Optional text seeded into the buffer and shown for editing (ignored when masked).</param>
    /// <returns>The raw input string (not trimmed).</returns>
    public static string ReadLn(ConsoleColor? color = null, bool masked = false, string? initialValue = null)
    {
        DiscardPendingConsoleKeys();

        var input = new StringBuilder();
        var caret = 0;

        if (!masked && !string.IsNullOrEmpty(initialValue))
        {
            input.Append(initialValue);
            caret = input.Length;
            WriteInputChars(initialValue, color, masked: false);
        }

        while (true)
        {
            var keyInfo = Console.ReadKey(intercept: true);
            var key = keyInfo.Key;

            if (key == ConsoleKey.Enter)
            {
                Console.Error.WriteLine();
                DiscardPendingConsoleKeys();
                break;
            }

            if (key == ConsoleKey.LeftArrow)
            {
                if (caret > 0)
                {
                    caret--;
                    Console.Error.Write('\b');
                }
                continue;
            }

            if (key == ConsoleKey.RightArrow)
            {
                if (caret < input.Length)
                {
                    WriteInputChar(input[caret], color, masked);
                    caret++;
                }
                continue;
            }

            if (key == ConsoleKey.Home)
            {
                while (caret > 0)
                {
                    caret--;
                    Console.Error.Write('\b');
                }
                continue;
            }

            if (key == ConsoleKey.End)
            {
                while (caret < input.Length)
                {
                    WriteInputChar(input[caret], color, masked);
                    caret++;
                }
                continue;
            }

            if (key == ConsoleKey.Delete)
            {
                if (caret < input.Length)
                {
                    input.Remove(caret, 1);
                    RedrawInputSuffix(input, caret, color, masked);
                }
                continue;
            }

            if (key == ConsoleKey.Backspace)
            {
                if (caret > 0)
                {
                    caret--;
                    input.Remove(caret, 1);
                    Console.Error.Write('\b');
                    RedrawInputSuffix(input, caret, color, masked);
                }
                continue;
            }

            if (!char.IsControl(keyInfo.KeyChar))
            {
                input.Insert(caret, keyInfo.KeyChar);
                RedrawInputSuffix(input, caret, color, masked);
                WriteInputChar(input[caret], color, masked);
                caret++;
            }
        }

        return input.ToString();
    }

    /// <summary>
    /// Drops keys already queued in the console input buffer (typically leftover multi-line paste).
    /// </summary>
    private static void DiscardPendingConsoleKeys()
    {
        while (Console.KeyAvailable)
        {
            Console.ReadKey(intercept: true);
        }
    }

    /// <summary>
    /// Rewrites buffer content from <paramref name="caret"/> to end, clears one leftover column, then returns the cursor to <paramref name="caret"/>.
    /// </summary>
    private static void RedrawInputSuffix(StringBuilder input, int caret, ConsoleColor? color, bool masked)
    {
        var remaining = input.Length - caret;
        for (var i = caret; i < input.Length; i++)
        {
            WriteInputChar(input[i], color, masked);
        }

        Console.Error.Write(' ');
        for (var i = 0; i < remaining + 1; i++)
        {
            Console.Error.Write('\b');
        }
    }

    private static void WriteInputChars(string text, ConsoleColor? color, bool masked)
    {
        foreach (var ch in text)
        {
            WriteInputChar(ch, color, masked);
        }
    }

    private static void WriteInputChar(char character, ConsoleColor? color, bool masked)
    {
        var displayChar = masked ? '*' : character;
        if (color.HasValue)
        {
            var originalColor = Console.ForegroundColor;
            Console.ForegroundColor = color.Value;
            Console.Error.Write(displayChar);
            Console.ForegroundColor = originalColor;
        }
        else
        {
            Console.Error.Write(displayChar);
        }
    }

    public static string ReadLnDim(bool masked = false) { return ReadLn(ConsoleColor.DarkGray, masked); }

    public static string ReadInput(ConsoleColor color = Color.Primary, bool masked = false)
    {
        return ReadLn(color, masked).Trim();
    }

    /// <summary>
    ///  Read multi line input
    /// </summary>
    /// <returns></returns>
    public static string ReadLnMulti()
    {
        var lines = new List<string>();
        string? line;
        while ((line = Console.ReadLine()) != null)
        {
            lines.Add(line);
        }
        return string.Join(Environment.NewLine, lines);
    }

    /// <summary>
    /// Parses a command-line string into an array of arguments, correctly handling quoted strings.
    /// </summary>
    /// <param name="input">The raw command-line string.</param>
    /// <returns>An array of arguments.</returns>
    public static string[] ParseCommandLine(string input)
    {
        var args = new List<string>();
        var currentArg = new System.Text.StringBuilder();
        var inQuotes = false;
        var quoteChar = '\0';

        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];

            if (!inQuotes && (c == '"' || c == '\''))
            {
                // Start of quoted string
                inQuotes = true;
                quoteChar = c;
            }
            else if (inQuotes && c == quoteChar)
            {
                // End of quoted string
                inQuotes = false;
                quoteChar = '\0';
            }
            else if (!inQuotes && char.IsWhiteSpace(c))
            {
                // End of argument
                if (currentArg.Length > 0)
                {
                    args.Add(currentArg.ToString());
                    currentArg.Clear();
                }
            }
            else
            {
                // Add character to current argument
                currentArg.Append(c);
            }
        }

        // Add final argument if any
        if (currentArg.Length > 0)
        {
            args.Add(currentArg.ToString());
        }

        return args.ToArray();
    }

    /// <summary>
    /// Writes the prompt text (no newline) and reads a single line of input from the user.<br />
    /// Multi-line paste after Enter is discarded by <see cref="ReadLn"/> so it cannot flood later prompts.
    /// </summary>
    /// <param name="promptText">The prompt to display (e.g. "Enter domain name: ").</param>
    /// <param name="promptColor">Color for the prompt text; defaults to primary when null.</param>
    /// <param name="inputColor">Color for the user's input as they type; defaults to promptColor when null.</param>
    /// <param name="masked">If true, masks input (e.g. for passwords).</param>
    /// <param name="indent">Number of spaces to indent the prompt.</param>
    /// <param name="defaultValue">
    /// Optional editable prefill shown in the input buffer (ignored when masked).<br />
    /// User can accept with Enter or edit with caret keys.
    /// </param>
    /// <returns>The trimmed user input.</returns>
    public static string Prompt(
        string promptText,
        ConsoleColor? promptColor = null,
        ConsoleColor? inputColor = null,
        bool masked = false,
        int indent = 0,
        string? defaultValue = null)
    {
        WriteFormat(promptText, false, indent, promptColor ?? Color.Primary);
        var initialValue = masked ? null : defaultValue;
        return ReadLn(inputColor ?? promptColor ?? Color.Primary, masked, initialValue).Trim();
    }

    /// <summary>
    /// Prompts with a bare label and appends the label separator, so label text stays free of chrome.<br />
    /// Use <see cref="Prompt"/> directly when the prompt must end with something other than a separator.
    /// </summary>
    /// <param name="label">Label without trailing separator (e.g. "Domain name").</param>
    /// <param name="promptColor">Color for the prompt text; defaults to primary when null.</param>
    /// <param name="inputColor">Color for the user's input as they type; defaults to promptColor when null.</param>
    /// <param name="masked">If true, masks input (e.g. for passwords).</param>
    /// <param name="indent">Number of spaces to indent the prompt.</param>
    /// <param name="defaultValue">Optional editable prefill shown in the input buffer (ignored when masked).</param>
    /// <returns>The trimmed user input.</returns>
    public static string PromptLabel(
        string label,
        ConsoleColor? promptColor = null,
        ConsoleColor? inputColor = null,
        bool masked = false,
        int indent = 0,
        string? defaultValue = null)
    {
        const string labelSeparator = ": ";

        return Prompt($"{label}{labelSeparator}", promptColor, inputColor, masked, indent, defaultValue);
    }

    /// <summary>
    /// Prompts the user with a yes/no question and returns the response as a boolean.
    /// </summary>
    /// <param name="question">The question to ask the user.</param>
    /// <param name="defaultYes">If true, default to yes; if false, default to no.</param>
    /// <returns>True if user answers yes, false if user answers no.</returns>
    public static bool PromptYesNo(string question, bool defaultYes = true, ConsoleColor? color = null, int indent = 0)
    {
        var defaultText = defaultYes ? "[Y/n]" : "[y/N]";
        var promptText = $"{question} {defaultText}: ";
        WriteFormat(promptText, false, indent, color ?? Color.Primary);
        while (true)
        {
            var response = ReadLn()?.Trim().ToLowerInvariant();

            if (string.IsNullOrEmpty(response))
            {
                return defaultYes;
            }

            if (response == "y" || response == "yes")
            {
                return true;
            }

            if (response == "n" || response == "no")
            {
                return false;
            }

            Write("Please answer 'y' for yes or 'n' for no: ", color, false, indent);
        }
    }

    /// <summary>
    /// Prompts the user to select one option from a list of string options. <br/>
    /// Displays numbered options and returns the selected option string. <br/>
    /// Re-prompts on invalid input with warning. Press Enter to cancel.
    /// </summary>
    /// <param name="prompt">The prompt text to display before the options.</param>
    /// <param name="options">The list of string options to choose from.</param>
    /// <param name="promptColor">Color for the prompt text.</param>
    /// <param name="optionsColor">Color for the numbered options list.</param>
    /// <param name="inputColor">Color for the user's input when typing their choice.</param>
    /// <param name="indent">Number of spaces to indent the entire prompt.</param>
    /// <returns>Result with selected option string, or failure if canceled.</returns>
    /// <example>
    /// <code>
    /// var options = new[] { "Porkbun", "GoDaddy", "Cloudflare" };
    /// string chosen = CliUtil.PromptOptions("Select DNS Provider:", options);
    /// // User sees: 1) Porkbun, 2) GoDaddy, 3) Cloudflare; chosen is e.g. "GoDaddy".
    /// </code>
    /// </example>
    public static Result<string> PromptOptions(
        string prompt,
        IReadOnlyList<string> options,
        ConsoleColor? promptColor = null,
        ConsoleColor? optionsColor = null,
        ConsoleColor? inputColor = null,
        int indent = 0)
    {
        // Display prompt
        WriteLnFormat(prompt, indent, promptColor);

        // Display numbered options
        for (var i = 0; i < options.Count; i++)
        {
            WriteLn($"{i + 1}) {options[i]}", optionsColor ?? Color.Primary, indent);
        }
        WriteLn();

        // Prompt for choice with retry on invalid input
        while (true)
        {
            var choiceStr = Prompt($"Choice [1-{options.Count}]: ", promptColor, inputColor, false, indent);

            // Empty input means cancel
            if (string.IsNullOrWhiteSpace(choiceStr))
            {
                return Result<string>.FailWithMessage("Selection canceled by user");
            }

            // Validate and return selection
            if (int.TryParse(choiceStr, out var choice) && choice >= 1 && choice <= options.Count)
            {
                return Result<string>.Success(options[choice - 1]);
            }

            // Invalid input - show warning and retry
            WriteLnWarning($"Please enter a number between 1 and {options.Count}, or press Enter to cancel.", indent);
            WriteLn();
        }
    }

    /// <summary>
    /// Prompts the user to select one option from a list of objects. <br/>
    /// Displays numbered options using the displaySelector and returns the selected object. <br/>
    /// Thin wrapper: converts to strings, calls string <see cref="PromptOptions"/>, maps result back to T.
    /// </summary>
    /// <typeparam name="T">The type of objects in the list.</typeparam>
    /// <param name="prompt">The prompt text to display before the options.</param>
    /// <param name="options">The list of objects to choose from.</param>
    /// <param name="displaySelector">Function to get the display string for each object.</param>
    /// <param name="promptColor">Color for the prompt text.</param>
    /// <param name="optionsColor">Color for the numbered options list.</param>
    /// <param name="inputColor">Color for the user's input when typing their choice.</param>
    /// <param name="indent">Number of spaces to indent the entire prompt.</param>
    /// <returns>Result with selected object, or failure if canceled.</returns>
    /// <example>
    /// <code>
    /// var selectedProvider = CliUtil.PromptOptions("Select DNS Provider:", DnsProviderSE.List, p =&gt; p.Name);
    /// DnsProvider = selectedProvider.Name;
    /// // User sees: 1) Porkbun, 2) GoDaddy; selectedProvider is the chosen DnsProviderSE instance.
    /// </code>
    /// </example>
    public static Result<T> PromptOptions<T>(
        string prompt,
        IReadOnlyList<T> options,
        Func<T, string> displaySelector,
        ConsoleColor? promptColor = null,
        ConsoleColor? optionsColor = null,
        ConsoleColor? inputColor = null,
        int indent = 0)
    {
        var displayStrings = options.Select(displaySelector).ToArray();
        var stringResult = PromptOptions(prompt, displayStrings, promptColor, optionsColor, inputColor, indent);

        if (stringResult.IsFailure)
        {
            return Result<T>.FailWithMessage(stringResult.Message!);
        }

        var index = Array.IndexOf(displayStrings, stringResult.Data);
        return index >= 0 ? Result<T>.Success(options[index]) : Result<T>.Success(options[0]); // Should always find a match
    }

    /// <summary>
    /// Prompts the user to select one or more options from a numbered list. <br/>
    /// Accepts comma-separated choices and inclusive ranges (e.g. <c>1,2</c> or <c>1-3</c>). <br/>
    /// Press Enter with no input to confirm an empty selection.
    /// </summary>
    /// <param name="prompt">The prompt text to display before the options.</param>
    /// <param name="options">The list of string options to choose from.</param>
    /// <param name="promptColor">Color for the prompt text.</param>
    /// <param name="optionsColor">Color for the numbered options list.</param>
    /// <param name="inputColor">Color for the user's input when typing their choice.</param>
    /// <param name="indent">Number of spaces to indent the entire prompt.</param>
    /// <returns>Result with selected option strings (possibly empty).</returns>
    public static Result<IReadOnlyList<string>> PromptOptionsMulti(
        string prompt,
        IReadOnlyList<string> options,
        ConsoleColor? promptColor = null,
        ConsoleColor? optionsColor = null,
        ConsoleColor? inputColor = null,
        int indent = 0)
    {
        if (options.Count == 0)
            return Result<IReadOnlyList<string>>.Success(Array.Empty<string>());

        WriteLnFormat(prompt, indent, promptColor);

        for (var i = 0; i < options.Count; i++)
            WriteLn($"{i + 1}) {options[i]}", optionsColor ?? Color.Primary, indent);

        WriteLn();

        while (true)
        {
            var choiceStr = Prompt(
                $"Select options (e.g. 1,2 or 1-{options.Count}): ",
                promptColor,
                inputColor,
                false,
                indent);

            if (string.IsNullOrWhiteSpace(choiceStr))
                return Result<IReadOnlyList<string>>.Success(Array.Empty<string>());

            if (!TryParseOptionSelection(choiceStr, options.Count, out var selectedIndexes))
            {
                WriteLnWarning(
                    $"Please enter option numbers between 1 and {options.Count} (e.g. 1,2 or 1-{options.Count}), or press Enter for none.",
                    indent);
                WriteLn();
                continue;
            }

            var selected = selectedIndexes
                .OrderBy(i => i)
                .Select(i => options[i - 1])
                .ToList();

            return Result<IReadOnlyList<string>>.Success(selected);
        }
    }

    /// <summary>
    /// Prompts the user to select one or more objects from a numbered list. <br/>
    /// Thin wrapper: displays via <paramref name="displaySelector"/>, delegates to string <see cref="PromptOptionsMulti"/>.
    /// </summary>
    /// <typeparam name="T">The type of objects in the list.</typeparam>
    /// <param name="prompt">The prompt text to display before the options.</param>
    /// <param name="options">The list of objects to choose from.</param>
    /// <param name="displaySelector">Function to get the display string for each object.</param>
    /// <param name="promptColor">Color for the prompt text.</param>
    /// <param name="optionsColor">Color for the numbered options list.</param>
    /// <param name="inputColor">Color for the user's input when typing their choice.</param>
    /// <param name="indent">Number of spaces to indent the entire prompt.</param>
    /// <returns>Result with selected objects (possibly empty).</returns>
    public static Result<IReadOnlyList<T>> PromptOptionsMulti<T>(
        string prompt,
        IReadOnlyList<T> options,
        Func<T, string> displaySelector,
        ConsoleColor? promptColor = null,
        ConsoleColor? optionsColor = null,
        ConsoleColor? inputColor = null,
        int indent = 0)
    {
        var displayStrings = options.Select(displaySelector).ToArray();
        var stringResult = PromptOptionsMulti(
            prompt, displayStrings, promptColor, optionsColor, inputColor, indent);

        if (stringResult.IsFailure)
            return Result<IReadOnlyList<T>>.FailWithMessage(stringResult.Message!);

        var selected = new List<T>();
        foreach (var display in stringResult.Data!)
        {
            var index = Array.IndexOf(displayStrings, display);
            if (index >= 0)
                selected.Add(options[index]);
        }

        return Result<IReadOnlyList<T>>.Success(selected);
    }

    private static bool TryParseOptionSelection(string input, int optionCount, out List<int> selectedOneBased)
    {
        selectedOneBased = [];
        var seen = new HashSet<int>();

        foreach (var token in input.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var dash = token.IndexOf('-');
            if (dash > 0 && dash < token.Length - 1)
            {
                if (!int.TryParse(token[..dash], out var start)
                    || !int.TryParse(token[(dash + 1)..], out var end))
                    return false;

                if (start > end)
                    (start, end) = (end, start);

                for (var n = start; n <= end; n++)
                {
                    if (n < 1 || n > optionCount)
                        return false;

                    if (seen.Add(n))
                        selectedOneBased.Add(n);
                }

                continue;
            }

            if (!int.TryParse(token, out var choice))
                return false;

            if (choice < 1 || choice > optionCount)
                return false;

            if (seen.Add(choice))
                selectedOneBased.Add(choice);
        }

        return selectedOneBased.Count > 0;
    }
}
