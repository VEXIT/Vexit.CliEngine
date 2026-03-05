/************************************************
 *
 * Copyright:		© 2025 VEXIT ®, www.vexit.com, Tomorrow is today...
 * Author:      	Vex Tatarevic
 * Date Created:    2026-01-30 - CLI configuration options
 * Date Updated:
 ************************************************/

using Vexit.CliEngine.Utils;

namespace Vexit.CliEngine;

/// <summary>
/// Configuration options for CLI output styling and behavior.
/// </summary>
public class CliConfig
{
    /// <summary>
    /// Color for labels and prompts.
    /// </summary>
    public ConsoleColor LabelColor { get; set; } = Cli.Color.Primary;

    /// <summary>
    /// Color for user input text.
    /// </summary>
    public ConsoleColor InputColor { get; set; } = Cli.Color.Dim;

    /// <summary>
    /// Color for numbered options list.
    /// </summary>
    public ConsoleColor OptionsColor { get; set; } = Cli.Color.Dim;

    /// <summary>
    /// Color for progress messages and spinners.
    /// </summary>
    public ConsoleColor ProgressMessageColor { get; set; } = Cli.Color.Dim;
}