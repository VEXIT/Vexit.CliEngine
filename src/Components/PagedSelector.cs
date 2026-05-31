/**************************************************
*
* Copyright:      © 2026 VEXIT ®, www.vexit.com
* Author:         Vex Tatarevic
* Date Created:   2026-04-27 - Paged multi-select component for CLI lists
*
**************************************************/

using Vexit.CliEngine.Constants;
using Vexit.Common.Models;

namespace Vexit.CliEngine.Components;

/// <summary>
/// Interactive paged selector for a list of string items.
/// </summary>
public static class PagedSelector
{
    public sealed class Options
    {
        public string Title { get; set; } = "Select records";
        public int PageSize { get; set; } = 10;
    }

    /// <summary>
    /// Opens a paged multi-select prompt and returns selected item values.
    /// Controls:
    /// - number(s) to toggle (e.g. 1,3,7)
    /// - &gt; next page
    /// - &lt; previous page
    /// - a select all on page
    /// - x clear selections on page
    /// - Enter finish
    /// - q cancel
    /// </summary>
    public static Result<List<string>> Prompt(ICliService cli, IReadOnlyList<string> items, Options? options = null)
    {
        options ??= new Options();
        if (items.Count == 0)
            return Result<List<string>>.Success(new List<string>());

        var pageSize = Math.Max(1, options.PageSize);
        var pageCount = (int)Math.Ceiling(items.Count / (double)pageSize);
        var page = 0;
        var selectedIndexes = new HashSet<int>();

        while (true)
        {
            var start = page * pageSize;
            var endExclusive = Math.Min(start + pageSize, items.Count);

            cli.WriteLn();
            cli.WriteLn(options.Title);
            cli.WriteLnDim($"Records {start + 1}-{endExclusive} of {items.Count} | Page {page + 1}/{pageCount} | Selected {selectedIndexes.Count}");
            cli.WriteLn();

            for (var i = start; i < endExclusive; i++)
            {
                var checkedMark = selectedIndexes.Contains(i) ? "[x]" : "[ ]";
                var line = $"{checkedMark} {i + 1}. {items[i]}";
                if (selectedIndexes.Contains(i))
                    cli.WriteLnLite(line);
                else
                    cli.WriteLnDim(line);
            }

            cli.WriteLn();
            cli.WriteLnDim("Commands: [number(s)] toggle, [>] next, [<] prev, [a] all page, [x] clear page, [Enter] done, [q] cancel");
            cli.WriteLabel("Input: ");
            var input = (cli.ReadInput() ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(input))
            {
                var selected = selectedIndexes
                    .OrderBy(i => i)
                    .Select(i => items[i])
                    .ToList();
                return Result<List<string>>.Success(selected);
            }

            if (string.Equals(input, "q", StringComparison.OrdinalIgnoreCase))
                return Result<List<string>>.Failure("Selection cancelled.", FailureCodes.USER_CANCEL);

            if (input == ">")
            {
                if (page < pageCount - 1)
                    page++;
                continue;
            }

            if (input == "<")
            {
                if (page > 0)
                    page--;
                continue;
            }

            if (string.Equals(input, "a", StringComparison.OrdinalIgnoreCase))
            {
                for (var i = start; i < endExclusive; i++)
                    selectedIndexes.Add(i);
                continue;
            }

            if (string.Equals(input, "x", StringComparison.OrdinalIgnoreCase))
            {
                for (var i = start; i < endExclusive; i++)
                    selectedIndexes.Remove(i);
                continue;
            }

            var tokens = input.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var parsedAny = false;
            foreach (var token in tokens)
            {
                if (!int.TryParse(token, out var oneBased))
                    continue;
                var idx = oneBased - 1;
                if (idx < 0 || idx >= items.Count)
                    continue;

                parsedAny = true;
                if (!selectedIndexes.Add(idx))
                    selectedIndexes.Remove(idx);
            }

            if (!parsedAny)
            {
                cli.WriteLnWarning("Invalid input. Use numbers or command keys shown above.");
            }
        }
    }
}
