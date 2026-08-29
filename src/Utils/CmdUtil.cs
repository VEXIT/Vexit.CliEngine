/*************************************************************
 *
 *  Copyright    : © 2026 VEXIT, www.vexit.com, Tomorrow is today...®
 *  Author       : Vex Tatarevic
 *  Date Created : 2026-08-24 - Command argv helpers (machine / root flags)
 *
 *************************************************************/

using Vexit.CliEngine.Constants;

namespace Vexit.CliEngine.Utils;

/// <summary>
/// Helpers for command argv / process-level CLI concerns shared by consumers (<c>Program.cs</c>) and the engine.
/// </summary>
public static class CmdUtil
{
    /// <summary>
    /// Returns true when <paramref name="args"/> contains <c>-m</c> or <c>--machine</c>.
    /// Used in consumer <c>Program.cs</c> to emit failure codes on stdout.
    /// </summary>
    public static bool IsMachineRequest(IReadOnlyList<string> args)
    {
        foreach (var arg in args)
        {
            if (arg == $"--{ProcessCliFlags.Machine}" || arg == $"-{ProcessCliFlags.MachineShort}")
                return true;
        }

        return false;
    }

    /// <summary>
    /// Returns true when every argument is a root version flag (<c>-v</c> or <c>--version</c>).
    /// </summary>
    public static bool IsRootVersionRequest(string[] args) =>
        args.Length > 0 && args.All(IsVersionFlag);

    public static bool IsHelpFlag(string arg) =>
        arg == $"--{RootCliFlags.Help}" || arg == $"-{RootCliFlags.HelpShort}";

    public static bool IsVersionFlag(string arg) =>
        arg == $"--{RootCliFlags.Version}" || arg == $"-{RootCliFlags.VersionShort}";
}
