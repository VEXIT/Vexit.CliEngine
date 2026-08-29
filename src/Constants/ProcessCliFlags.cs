/*************************************************************
 *
 *  Copyright    : © 2026 VEXIT, www.vexit.com, Tomorrow is today...®
 *  Author       : Vex Tatarevic
 *  Date Created : 2026-08-22
 *  Date Updated : 
 *
 *************************************************************/

namespace Vexit.CliEngine.Constants;

/// <summary>
/// Process-level CLI flag names (no dash prefix). Format argv tokens at call sites, e.g. <c>$"--{Machine}"</c>, <c>$"-{MachineShort}"</c>.
/// Parsed on every <see cref="BaseClasses.CmdBase"/> via <c>[Option]</c>.
/// </summary>
public static class ProcessCliFlags
{
    public const string Machine = "machine";

    public const string MachineShort = "m";

    public const string Yes = "y";

    /// <summary>Default help text for <c>-y</c> on <see cref="BaseClasses.CmdBase"/>.</summary>
    public const string YesOptionDescription = "Accept defaults and skip interactive prompts";
}
