/************************************************
 *
 * Copyright:		© 2025 VEXIT ®, www.vexit.com, Tomorrow is today...
 * Author:      	Vex Tatarevic
 * Date Created:    2025-09-15 - Initial creation for base command class
 * DateUpdated:		2025-12-22 - Refactored to inherit from CliBase for SRP
 *                  2026-08-24 - Added MachineMode (-m / --machine) on every command
 *                  2026-08-26 - Added AcceptDefaults (-y) and NonInteractive on CmdBase (-m implies -y)
 ************************************************/

using Vexit.CliEngine.Attributes;
using Vexit.CliEngine.Constants;
using Vexit.CliEngine.Utils;
using Vexit.Common.Models;

namespace Vexit.CliEngine.BaseClasses;

/// <summary>
/// Base class for all commands
/// </summary>
public abstract class CmdBase
{
    /// <summary>
    /// Set when the caller passes <c>-m</c> or <c>--machine</c> (agents, scripts, CI).
    /// <para>
    /// We can check this flag to write code that will only run when machine mode is enabled, to return structured data required by program pipeline or agent.
    /// For example:
    /// </para>
    /// <example>
    /// <code>
    ///  if (MachineMode)
    ///  {
    ///      _cli.WriteJsonData(records);
    ///      return Ok();
    ///  }
    ///
    ///  _cli.WriteLn("These are the records:");
    ///  PrintHumanTable(records);
    ///  return Ok();
    /// </code>
    /// </example>
    /// </summary>
    [Option(ProcessCliFlags.Machine, ProcessCliFlags.MachineShort,
        "Request machine-readable stdout (JSON/text). Human output stays on stderr.",
        HideFromHelp = true)]
    public bool MachineMode { get; set; }

    /// <summary>
    /// Skip interactive prompts (<c>-y</c>). Inherited by every command; shown in help (unlike <see cref="MachineMode"/>).
    /// </summary>
    [Option(ProcessCliFlags.Yes, null, ProcessCliFlags.YesOptionDescription)]
    public bool AcceptDefaults { get; set; }

    /// <summary>
    /// Non-interactive mode: <see cref="MachineMode"/> (automation) or explicit <see cref="AcceptDefaults"/> (<c>-y</c>).
    /// </summary>
    protected bool NonInteractive => MachineMode || AcceptDefaults;

    // The CommandController ONLY ever calls this method.
    // Arguments are automatically bound to properties via [Option] and [Argument] attributes.
    public virtual Task<Result> ExecuteAsync()
    {
        // By default, it calls the synchronous version.
        // This makes the synchronous path the "default" experience.
        return Task.FromResult(Execute());
    }

    // --- The Override for Synchronous Commands ---
    // This is what 95% of commands will override.
    // Arguments are automatically bound to properties via [Option] and [Argument] attributes.
    public virtual Result Execute()
    {
        // Default implementation for command groups that don't have a direct action.
        return Result.Ok();
    }


    // --- HELPER METHODS for cleaner command implementation ---

    // --- NON-GENERIC HELPERS (for commands returning no data) ---

    /// <summary>
    /// Returns a successful Result with no data.
    /// </summary>
    protected Result Ok(string? message = null)
        => Result.Ok(message);

    /// <summary>
    /// Returns a failure Result with no data.
    /// </summary>
    protected Result Failure(string failureCode, string? failureMessage = null)
        => Result.Failure(failureCode: failureCode, failureMessage: failureMessage ?? string.Empty);


    /// <summary>
    /// Converts a failed non-generic Result into a failed non-generic Result.
    /// </summary>
    protected Result Failure(Result failedResult) =>
        Result.Failure(failureCode: failedResult.FailureCode ?? string.Empty, failureMessage: failedResult.Message ?? string.Empty);

    protected Result FailWithCode(string code)
   => Result.FailWithCode(code);

    protected Result FailWithMessage(string message)
        => Result.FailWithMessage(message);



    // --- GENERIC HELPERS (for operations/commands returning data) ---

    /// <summary>
    /// Returns a successful Result<T> with data.
    /// </summary>
    protected Result<T> Ok<T>(T data, string? message = null) => Result<T>.Success(data, message);

    /// <summary>
    /// Returns a failure Result<T> with an error message.
    /// </summary>    
    protected Result<T> Failure<T>(string failureCode, string? failureMessage = null)
        => Result<T>.Failure(failureCode: failureCode, failureMessage: failureMessage ?? string.Empty);

    protected Result<T> FailWithCode<T>(string code)
        => Result<T>.FailWithCode(code);

    protected Result<T> FailWithMessage<T>(string message)
        => Result<T>.FailWithMessage(message);

    /// <summary>
    /// Converts a failed generic Result into a failed non-generic Result.
    /// </summary>
    protected Result Failure<T>(Result<T> failedResult) =>
        Result.Failure(failureCode: failedResult.FailureCode ?? string.Empty, failureMessage: failedResult.Message ?? string.Empty);

}