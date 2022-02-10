using System.Text;
using System.Text.RegularExpressions;

namespace uav.logic.Extensions
{
    public static class StringExtensions
    {
        public static bool IsNullOrEmpty(this string s) => string.IsNullOrEmpty(s);

        private static Regex slashCommandDashLocator = new Regex(@"(?<=[a-z])\s?(?=[A-Z])");
        public static string ToSlashCommand(this string s) => slashCommandDashLocator.Replace(s, "-").ToLower();

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

    }
}