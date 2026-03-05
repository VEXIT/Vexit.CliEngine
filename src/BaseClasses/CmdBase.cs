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
    {
        return Result.Ok(message);
    }

    /// <summary>
    /// Returns a failure Result with no data.
    /// </summary>
    protected Result Failure(string errorMessage, string? errorCode = null)
    {
        return Result.Failure(errorMessage, errorCode);
    }
    
    /// <summary>
    /// Converts a failed generic Result into a failed non-generic Result.
    /// </summary>
    protected Result Failure<T>(Result<T> failedResult)
    {
        return Result.Failure(failedResult.Message, failedResult.ErrorCode);
    }

    /// <summary>
    /// Converts a failed non-generic Result into a failed non-generic Result.
    /// </summary>
    protected Result Failure(Result failedResult)
    {
        return failedResult.IsFailure ? Result.Failure(failedResult.Message, failedResult.ErrorCode) : Result.Ok();
    }


    // --- GENERIC HELPERS (for operations/commands returning data) ---

    /// <summary>
    /// Returns a successful Result<T> with data.
    /// </summary>
    protected Result<T> Ok<T>(T data, string? message = null)
    {
        return Result<T>.Success(data, message);
    }

    /// <summary>
    /// Returns a failure Result<T> with an error message.
    /// </summary>
    protected Result<T> Failure<T>(string errorMessage, string? errorCode = null)
    {
        return Result<T>.Failure(errorMessage, errorCode);
    }
}