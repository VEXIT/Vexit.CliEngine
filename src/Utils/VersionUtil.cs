/************************************************
 *
 * Copyright:		© 2025 VEXIT ®, www.vexit.com, Tomorrow is today...
 * Author:      	Vex Tatarevic
 * Date Created:    2026-08-17 - Consumer CLI version helper for root --version handling
 * DateUpdated:
 *
 ************************************************/

using System.Reflection;

namespace Vexit.CliEngine.Utils;

/// <summary>
/// Reads the version of the consumer CLI app (the process entry assembly — e.g. <c>vx</c>, <c>vxs</c>), not CliEngine.
/// </summary>
public static class VersionUtil
{
    private const string _unknownVersion = "unknown";

    /// <summary>
    /// Returns the version baked into the entry assembly at build time from the consumer <c>.csproj</c> <c>&lt;Version&gt;</c> property.<br />
    /// Uses <see cref="Assembly.GetEntryAssembly"/> so the value reflects the executable the user ran, not this library.<br />
    /// Format: <c>major.minor.build</c>. Returns <c>unknown</c> when the assembly version is not set.
    /// </summary>
    public static string GetVersion()
    {
        var version = Assembly.GetEntryAssembly()?.GetName().Version;
        return version != null ? $"{version.Major}.{version.Minor}.{version.Build}" : _unknownVersion;
    }
}
