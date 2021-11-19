using System;
using System.Collections.Generic;

namespace uav.logic.Models;

public class Tournament
{
    public static string SpanToReadable(TimeSpan span)
    {
        var pieces = new List<string>();
        if (span.Days > 0)
        {
            pieces.Add($"{span.Days} day{(span.Days == 1 ? string.Empty : "s")}");
        }

        if (span.Hours > 0)
        {
            pieces.Add($"{span.Hours} hour{(span.Hours == 1 ? string.Empty : "s")}");
        }

        if (span.Minutes > 0)
        {
            pieces.Add($"{span.Minutes} minute{(span.Minutes == 1 ? string.Empty : "s")}");
        }

        if (span.Seconds > 0)
        {
            pieces.Add($"{span.Seconds}.{span.Milliseconds:D3} second{(span.Seconds == 1 ? string.Empty : "s")}");
        }

        return pieces.Count switch {
            0 => "no time!",
            1 => pieces[0],
            2 => string.Join(" and ", pieces),
            _ => string.Join(" and ", string.Join(", ", pieces.GetRange(0, pieces.Count - 1)), pieces[^1]),
        };
    } 
}