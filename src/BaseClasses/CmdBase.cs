/************************************************
 *
 * Copyright:		© 2025 VEXIT ®, www.vexit.com, Tomorrow is today...
 * Author:      	Vex Tatarevic
 * Date Created:    2025-09-15 - Initial creation for base command class
 * DateUpdated:		2025-12-22 - Refactored to inherit from CliBase for SRP
 ************************************************/

using Vexit.CliEngine.Utils;
using Vexit.Common.Models;

namespace Vexit.CliEngine.BaseClasses;

/// <summary>
/// Base class for all commands
/// </summary>
public abstract class CmdBase
{
    // --- The Public Entry Point for the Engine ---
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
        => Result.Failure(failureCode: failureCode, failureMessage: failureMessage);


    /// <summary>
    /// Converts a failed non-generic Result into a failed non-generic Result.
    /// </summary>
    protected Result Failure(Result failedResult) =>
        Result.Failure(failureCode: failedResult.FailureCode ?? string.Empty, failureMessage: failedResult.Message);

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
        => Result<T>.Failure(failureMessage: failureMessage, failureCode: failureCode);

    protected Result<T> FailWithCode<T>(string code)
        => Result<T>.FailWithCode(code);

    protected Result<T> FailWithMessage<T>(string message)
        => Result<T>.FailWithMessage(message);

    /// <summary>
    /// Converts a failed generic Result into a failed non-generic Result.
    /// </summary>
    protected Result Failure<T>(Result<T> failedResult) =>
        Result.Failure(failureCode: failedResult.FailureCode ?? string.Empty, failureMessage: failedResult.Message);

}