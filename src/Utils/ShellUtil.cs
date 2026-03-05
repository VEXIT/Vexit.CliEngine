/*************************************************************
 *
 *  Copyright    : © VEXIT 2025, www.vexit.com
 *  Author       : Vex Tatarevic
 *  Date Created : 2025-10-28 - Created ShellUtil for shell detection and profile management
 *  Date Updated : 2025-11-01 - Vex | Moved from Vexit.Common.Utils to Vexit.MetaCli.Utils
 *
 ************************************************************/

using Vexit.CliEngine.Constants;
using Vexit.Common.Models;

namespace Vexit.CliEngine.Utils;

/// <summary>
/// Shell utility methods for shell detection and profile management
/// </summary>
public static class ShellUtil
{
    /// <summary>
    /// Gets the current shell from the SHELL environment variable
    /// </summary>
    /// <returns>Shell name (e.g., "bash", "zsh") or null if not detected</returns>
    public static string? GetShell()
    {
        var shell = Environment.GetEnvironmentVariable("SHELL");
        // On Windows check for COMSPEC environment variable
        if (string.IsNullOrWhiteSpace(shell) && OperatingSystem.IsWindows())
        {
            shell = Environment.GetEnvironmentVariable("COMSPEC");
        }
        if (string.IsNullOrWhiteSpace(shell))
        {
            return null;
        }
        // Path.GetFileName("/usr/bin/bash") returns "bash"
        // Path.GetFileName("/bin/zsh") returns "zsh" 
        // ToLowerInvariant() ensures consistent casing (BASH -> bash)
        return Path.GetFileNameWithoutExtension(shell).ToLowerInvariant();
    }

    /// <summary>
    /// Returns Vexit Result object which contains: <br />
    /// - IsSuccess - whether the current shell is supported or not  <br />
    /// - Data - Tuple of Shell and FilePath  <br />
    /// -- Shell - Name of the current shell if known, else null  <br />
    /// -- FilePath - Relative path to shell profile, starting at the user home directory
    /// </summary>
    /// <param name="shellName">Shell name (e.g., "bash", "zsh"). If null, detects current shell</param>
    /// <returns>
    /// 
    /// <code>
    /// { 
    ///   IsSuccess : true/false
    ///   Data: { 
    ///    Shell : "bash",
    ///    FilePath : ".bash_profile"
    ///   }
    /// }
    /// </code>
    /// </returns>
    public static Result<(string? Shell, string? FilePath)> GetProfile(string? shellName = null)
    {
        shellName ??= GetShell();
        string? fileName = null;
        if (string.IsNullOrWhiteSpace(shellName))
          return Result<(string?, string?)>.Failure("Shell name is required");

        // For bash on macOS/Windows, check which profile file exists
        if (shellName == Shells.Bash && (OperatingSystem.IsMacOS() || OperatingSystem.IsWindows()))
        {
            var userHomeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var bashProfile = Path.Combine(userHomeDir, ".bash_profile");
            var bashrc = Path.Combine(userHomeDir, ".bashrc");

            // Return whichever exists first, prioritizing .bash_profile
            if (File.Exists(bashProfile))
                fileName = ".bash_profile";

            if (File.Exists(bashrc))
                fileName = ".bashrc";
        }

        if (string.IsNullOrEmpty(fileName))
        {
            fileName = shellName switch
            {
                Shells.Bash => ".bashrc", // Linux bash
                Shells.Zsh => ".zshrc",
                Shells.Ksh => ".kshrc",
                Shells.Fish => ".config/fish/config.fish",
                _ => null // Unsupported shell
            };
        }

        var isSuccess = fileName != null;
        var returnObject = (shellName, fileName);
        return Result<(string?, string?)>.Success(returnObject);
    }

    /// <summary>
    /// Checks if the given shell is supported by VMod CLI
    /// </summary>
    /// <param name="shellName">Shell name to check</param>
    /// <returns>True if supported, false otherwise</returns>
    public static bool IsShellSupported(string? shellName = null)
    {
        shellName ??= GetShell();
        return !string.IsNullOrWhiteSpace(shellName) && Shells.Supported.Contains(shellName);
    }
}
