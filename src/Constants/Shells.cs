/*************************************************************
 *
 *  Copyright    : © VEXIT 2025, www.vexit.com
 *  Author       : Vex Tatarevic
 *  Date Created : 2025-10-28 - Created Shells constants for supported shell types
 *  Date Updated : 
 *
 ************************************************************/

namespace Vexit.CliEngine.Constants;

/// <summary>
/// Constants for supported shell types and their profile file mappings
/// </summary>
public static class Shells
{
    public const string Bash = "bash";
    public const string Zsh = "zsh";
    public const string Ksh = "ksh";
    public const string Fish = "fish";

    /// <summary>
    /// Array of all supported shell names
    /// </summary>
    public static readonly string[] Supported = { Bash, Zsh, Ksh, Fish };

    /// <summary>
    /// Human-readable list of supported shells for error messages
    /// </summary>
    public static string SupportedList => string.Join(", ", Supported);
}
