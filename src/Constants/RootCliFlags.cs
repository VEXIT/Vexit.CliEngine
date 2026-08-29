/************************************************
 *
 * Copyright:		© 2025 VEXIT ®, www.vexit.com, Tomorrow is today...
 * Author:      	Vex Tatarevic
 * Date Created:    2026-08-17 - Root-level CLI flag tokens handled by CommandController
 * DateUpdated:
 *
 ************************************************/

namespace Vexit.CliEngine.Constants;

/// <summary>
/// Root-level CLI flag names (no dash prefix). Format argv tokens at call sites, e.g. <c>$"--{Help}"</c>, <c>$"-{HelpShort}"</c>.
/// Handled by <see cref="CommandController"/> before command resolution.
/// </summary>
public static class RootCliFlags
{
    public const string Help = "help";

    public const string HelpShort = "h";

    public const string Version = "version";

    public const string VersionShort = "v";
}
