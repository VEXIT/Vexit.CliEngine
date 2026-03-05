/************************************************
 *
 * Copyright:		© 2025 VEXIT ®, www.vexit.com, Tomorrow is today...
 * Author:      	Vex Tatarevic
 * Date Created:    2025-11-01 - Initial creation for failure code constants
 * DateUpdated:		2025-11-01
 *
 ************************************************/

namespace Vexit.CliEngine.Constants;

/// <summary>
/// Standard failure codes for MetaCli operations
/// </summary>
public static class FailureCodes
{
    public const string VALIDATION_ERROR = "VALIDATION_ERROR";
    public const string UNKNOWN_COMMAND = "UNKNOWN_COMMAND";
    public const string COMMAND_NOT_FOUND = "COMMAND_NOT_FOUND";
    public const string HOOK_EXECUTION_FAILED = "HOOK_EXECUTION_FAILED";
    public const string INVALID_INPUT = "INVALID_INPUT";
    public const string USER_CANCEL = "USER_CANCEL";
}
