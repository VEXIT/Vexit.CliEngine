/************************************************
 *
 * Copyright:		© 2025 VEXIT ®, www.vexit.com, Tomorrow is today...
 * Author:      	Vex Tatarevic
 * Date Created:    2025-11-01 - Initial creation for data format enumeration
 * DateUpdated:		2025-11-30 - Converted to SmartEnum
 *
 ************************************************/

using Vexit.Common.BaseClasses;

namespace Vexit.CliEngine.Enums;

/// <summary>
/// Smart enum wrapper for <see cref="DataFormatEnum"/> providing string helpers.
/// Instances are auto-generated from enum values.
/// </summary>
public sealed class DataFormatSE : SmartEnumBase<DataFormatEnum, DataFormatSE>
{
    public static DataFormatSE Json => FromEnum(DataFormatEnum.Json);
    public static DataFormatSE Text => FromEnum(DataFormatEnum.Text);

    private DataFormatSE(DataFormatEnum enumValue, string name) : base(enumValue, name) { }
}

/// <summary>
/// Enumeration for supported data output formats in CLI commands
/// </summary>
public enum DataFormatEnum
{
    /// <summary>
    /// JSON format (default)
    /// </summary>
    Json,

    /// <summary>
    /// Plain text format (ToString representation)
    /// </summary>
    Text
}
