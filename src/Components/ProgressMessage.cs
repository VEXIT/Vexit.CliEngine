/*************************************************************
 *
 *  Copyright    : © VEXIT 2025, www.vexit.com
 *  Author       : Vex Tatarevic
 *  Date Created : 2025-01-12 - Progress message component with spinner
 *  Date Updated :
 *
 *************************************************************/

using Vexit.Common.Models;

namespace Vexit.CliEngine.Components;

/// <summary>
/// Displays progress messages with animated spinners during async operations. <br/>
/// Shows success/error messages when operations complete.
/// </summary>
public static class ProgressMessage
{
    /// <summary>
    /// Animation types for the spinner
    /// </summary>
    public enum Animation
    {
        /// <summary>
        /// Pipe-based animation: | / - \
        /// </summary>
        SpinnerPipe,

        /// <summary>
        /// Unicode dot animation: ⠋ ⠙ ⠹ ⠸ ⠼ ⠴ ⠦ ⠧ ⠇ ⠏
        /// </summary>
        SpinnerDots
    }

    /// <summary>
    /// Executes work with progress display and completion messages.
    /// </summary>
    /// <param name="progressMessage">Message shown during progress (with spinner)</param>
    /// <param name="work">The async work function to execute</param>
    /// <param name="successMessage">Message shown on success (defaults to "{progressMessage} ✓")</param>
    /// <param name="errorMessage">Message shown on error (defaults to "{progressMessage} ✗")</param>
    /// <param name="animation">Animation type for spinner</param>
    /// <param name="totalSteps">Total number of steps (optional, for progress bar)</param>
    /// <param name="currentStep">Current step number (optional, for progress bar)</param>
    /// <param name="token">Cancellation token</param>
    /// <returns>Result of the work execution</returns>
    public static async Task<Result> ShowAsync(
        string progressMessage,
        Func<Task<Result>> work,
        string? successMessage = null,
        string? errorMessage = null,  
        int indent = 0,
        ConsoleColor color = Cli.Color.Dim,
        ConsoleColor? successColor = null,
        ConsoleColor? errorColor = null,
        Animation animation = Animation.SpinnerPipe,
        int? totalSteps = null,
        int? currentStep = null,
        CancellationToken token = default)
    {
        // Set default messages if not provided
        successMessage ??= $"{progressMessage} ✓";
        errorMessage ??= $"{progressMessage} ✗";

        // Get animation frames
        var frames = GetAnimationFrames(animation);

        // Create cancellation token for spinner
        using var spinnerCts = CancellationTokenSource.CreateLinkedTokenSource(token);

        // Start spinner/progress task
        var displayTask = Task.Run(async () =>
        {
            var frameIndex = 0;
            var startTime = DateTime.UtcNow;

            while (!spinnerCts.IsCancellationRequested)
            {
                var elapsed = DateTime.UtcNow - startTime;

                string displayText;
                string progressBar = "";
                if (totalSteps.HasValue && currentStep.HasValue && totalSteps.Value > 0)
                {
                    // Show progress bar
                    var bar = CreateProgressBar(currentStep.Value, totalSteps.Value, 20);
                    var percentage = (int)Math.Round((double)currentStep.Value / totalSteps.Value * 100);
                    progressBar = $" {bar} {percentage}%";
                }

                // Show spinner with elapsed time
                var spinner = CreateSpinner(frames, ref frameIndex);
                var timer = CreateTimer(elapsed);
                displayText = $"{progressMessage}{progressBar} {spinner} {timer}";


                // Use \r on stderr (Cli.Write → Console.Error) to overwrite the current line in place
                Cli.Write($"\r{displayText}", color, false, indent);

                try
                {
                    await Task.Delay(250, spinnerCts.Token);
                }
                catch (TaskCanceledException)
                {
                    // Display cancelled - exit loop
                    break;
                }
            }
        }, spinnerCts.Token);

        try
        {
            // Execute the work
            var result = await work();

            // Stop display
            spinnerCts.Cancel();
            // Guard: if the spinner task itself faulted or was cancelled, swallow that —
            // it must not replace the real work result with an unrelated exception.
            try { await displayTask; } catch { /* spinner errors must not propagate */ }

            // Clear the spinner line on stderr before printing the completion message
            ClearLine();

            if (result.IsSuccess)
            {
                Cli.WriteLn(successMessage, successColor ?? color, indent);
            }
            else
            {
                Cli.WriteLn(errorMessage, errorColor ?? color, indent);
            }

            return result;
        }
        catch (Exception ex)
        {
            // Stop display on exception
            spinnerCts.Cancel();
            // Guard: awaiting a faulted displayTask here would re-throw and swallow the
            // original exception, so we safely discard any spinner-task error.
            try { await displayTask; } catch { /* spinner errors must not propagate */ }

            // Clear the spinner line on stderr before printing the error message
            ClearLine();
            Cli.WriteLnError(errorMessage);

            return Result.Failure(ex.Message);
        }
    }

    /// <summary>
    /// Gets the animation frames for the specified animation type
    /// </summary>
    private static string[] GetAnimationFrames(Animation animation)
    {
        return animation switch
        {
            Animation.SpinnerPipe => new[] { "|", "/", "-", "\\" },
            Animation.SpinnerDots => new[] { "⠋", "⠙", "⠹", "⠸", "⠼", "⠴", "⠦", "⠧", "⠇", "⠏" },
            _ => new[] { "|", "/", "-", "\\" }
        };
    }

    /// <summary>
    /// Creates a spinner frame
    /// </summary>
    /// <param name="frames">Array of spinner frames</param>
    /// <param name="frameIndex">Current frame index (will be incremented)</param>
    /// <returns>Current spinner frame</returns>
    private static string CreateSpinner(string[] frames, ref int frameIndex)
    {
        return frames[frameIndex++ % frames.Length];
    }

    /// <summary>
    /// Creates a formatted timer string
    /// </summary>
    /// <param name="elapsed">Elapsed time</param>
    /// <returns>Formatted timer string like (00:15)</returns>
    private static string CreateTimer(TimeSpan elapsed)
    {
        return $"({elapsed:mm\\:ss})";
    }

    /// <summary>
    /// Clears the spinner line on stderr by overwriting it with spaces (must match <see cref="Cli.Write"/> / Console.Error).
    /// </summary>
    private static void ClearLine()
    {
        try
        {
            // Terminal width for the clear width; 120 when WindowWidth is unavailable
            var width = Console.WindowWidth > 0 ? Console.WindowWidth : 120;

            // \r + spaces + \r on stderr — same stream as the in-place spinner updates
            Console.Error.Write("\r" + new string(' ', width) + "\r");
        }
        catch
        {
            // Fixed width on stderr when WindowWidth or the clear write fails
            Console.Error.Write("\r" + new string(' ', 120) + "\r");
        }
    }

    /// <summary>
    /// Creates a progress bar string
    /// </summary>
    /// <param name="current">Current step (0-based)</param>
    /// <param name="total">Total steps</param>
    /// <param name="width">Width of the progress bar</param>
    /// <returns>Progress bar string like [████████░░░░░░░░]</returns>
    private static string CreateProgressBar(int current, int total, int width = 20)
    {
        var progress = Math.Max(0, Math.Min(1, (double)current / total));
        var filled = (int)Math.Round(progress * width);
        var empty = width - filled;

        var bar = new string('█', filled) + new string('░', empty);
        return $"[{bar}]";
    }
}