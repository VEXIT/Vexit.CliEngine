/************************************************
 *
 * Copyright:		© 2025 VEXIT ®, www.vexit.com, Tomorrow is today...
 * Author:      	Vex Tatarevic
 * Date Created:    2025-10-31 - Initial creation for components - cli output ux components
 * DateUpdated:		2025-10-31
 *
 ************************************************/

using System.Text;


namespace Vexit.CliEngine.Components;

public enum AlignEnum { Left, Center, Right }

public static class Components
{
    public const int BorderWidth = 70;

    public static string AlignText(string text, int lineWidth, AlignEnum alignment)
    {
        if (text.Length >= lineWidth)
            return text.Substring(0, lineWidth);

        var padding = lineWidth - text.Length;

        return alignment switch
        {
            AlignEnum.Left => text + new string(' ', padding),
            AlignEnum.Center => new string(' ', padding / 2) + text + new string(' ', padding - padding / 2),
            AlignEnum.Right => new string(' ', padding) + text,
            _ => text
        };
    }

    public static string Title(string title, AlignEnum alignment = AlignEnum.Center)
    {
        var sb = new StringBuilder();
        var titleBorder = new string('=', BorderWidth);
        var centeredTitle = AlignText(title, BorderWidth, alignment);

        sb.AppendLine();
        sb.AppendLine(titleBorder);
        sb.AppendLine(centeredTitle);
        sb.AppendLine(titleBorder);
        sb.AppendLine();
        return sb.ToString();
    }
}