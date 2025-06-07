using System.Text;
using System.Text.RegularExpressions;

namespace uav.logic.Extensions;

public static partial class StringExtensions
{
    public static bool IsNullOrEmpty(this string? s) => string.IsNullOrEmpty(s);
    public static bool HasValue(this string? s) => !string.IsNullOrEmpty(s);

    public static string IfNullOrEmptyThen(this string? s, string fallback) => s.IsNullOrEmpty() ? fallback : s!;

    public static string ToSlashCommand(this string s) => SlashCommandDashLocator().Replace(s, "-").ToLower();

    public static StringBuilder SafeAppend(this StringBuilder sb, string s)
    {
        if (s is not null)
        {
            sb.Append(s);
        }
        return sb;
    }

    public static StringBuilder SafeAppendLine(this StringBuilder sb, string s)
    {
        if (s is not null)
        {
            sb.AppendLine(s);
        }
        return sb;
    }

    //discord plays funny with certain characters, so escape them.
    [GeneratedRegex(@"(?=[*_])")]
    private static partial Regex FixNameRegex();
    public static string FixName(this string name) => FixNameRegex().Replace(name, "\\");

    [GeneratedRegex(@"(?<=[a-z])\s?(?=[A-Z])")]
    private static partial Regex SlashCommandDashLocator();
}
