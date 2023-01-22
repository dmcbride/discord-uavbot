using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using uav.logic.Extensions;

namespace uav.logic.Models
{
    public class GV
    {
        private double gv;
        private GV(double d)
        {
            gv = d;
        }

        private static readonly IReadOnlyDictionary<string, int> suffixExponent;
        private static readonly IReadOnlyDictionary<int, string> toSuffixExponent;
        private static readonly IReadOnlyList<string> recommendedSuffixes;

        static GV()
        {
            recommendedSuffixes = new[] {
                "k", "M", "B", "T", "q", "Q", "s", "S", "O", "N", "D"
                }.Concat(Enumerable.Range((int)'a', (int)'m' - (int)'a' + 1).Select(i => "a" + (char)i))
                .ToList();
            var s = recommendedSuffixes
                .Select((s, i) => (s, i: 3 * (i + 1)))
                .ToDictionary(k => k.s, k => k.i);
            toSuffixExponent = s.ToDictionary(kv => kv.Value, kv => kv.Key); // reverse lookup
            s["m"] = s["M"];
            s["K"] = s["k"];
            suffixExponent = s;
        }

        public static IEnumerable<string> AllowedExtensions => recommendedSuffixes;

        private static readonly Regex SiNumber = new Regex(@"^(?<qty>\d+(?:[.,]\d+)?|[.,]\d+)(?<suffix>[a-zA-Z]{0,2})$");
        private static readonly Regex ExpNumber = new Regex(@"^(?<qty>\d+(?:[].,]\d+)?|[.,]\d+)[eE]\+?(?<exp>\d+)$");
        private static Regex _decimalSeparator = new Regex("[,.]");
        private static string FixComma(string s) => _decimalSeparator.Replace(s, ".");

        public static GV FromNumber(double gv)
        {
            return new GV(gv);
        }

        public static GV FromString(string v)
        {
            if (!TryFromString(v, out var GV, out var errorMessage) || GV is null)
            {
                throw new Exception(errorMessage);
            }
            return GV;
        }

        public static GV Zero = FromNumber(0);

        public static bool TryFromString(string v, [NotNullWhen(true)] out GV? gv, [NotNullWhen(false)] out string? errorMessage)
        {
            errorMessage = null;

            // some people put in $'s, just drop them.
            if (v.StartsWith('$'))
            {
                v = v.Substring(1);
            }

            var m = SiNumber.Match(v);
            var qty = 0d;
            gv = null;
            if (m.Success)
            {
                qty = Double.Parse(FixComma(m.Groups["qty"].Value));
                var suffix = m.Groups["suffix"].Value;
                if (suffixExponent.TryGetValue(suffix, out var exp))
                {
                    qty *= Math.Pow(10d, exp);
                }
                else if (!suffix.IsNullOrEmpty()) // you tried something else. Don't do that.
                {
                    if (suffixExponent.TryGetValue(suffix.ToLowerInvariant(), out _) ||
                        suffixExponent.TryGetValue(suffix.ToUpperInvariant(), out _))
                    {
                        errorMessage = $"Perhaps you got the case wrong. Suffixes are allowed to be one of: {string.Join(",", recommendedSuffixes)}.";
                    }
                    else
                    {
                        errorMessage = $"Suffixes are allowed to be one of: {string.Join(",", recommendedSuffixes)}.";
                    }
                    return false;
                }
                gv = new GV(qty);
                return true;
            }
            else if ((m = ExpNumber.Match(v)).Success)
            {
                qty = Double.Parse(FixComma(m.Groups["qty"].Value));
                qty *= Math.Pow(10d, Double.Parse(FixComma(m.Groups["exp"].Value)));
                gv = new GV(qty);
                return true;
            }
            errorMessage = "Not a valid number.";
            return false;
        }

        public int TierNumber => Exponential - 6;
        public int Exponential => (int)Math.Floor(Math.Log10(gv));
  
        private static (string? letter, string exp) ToStrings(double qty)
        {
            var powerOf10 = (int) Math.Floor(Math.Log10(qty));
            var powerOf1000 = powerOf10 / 3;

            string? letter = null;
            if (powerOf1000 == 0)
            {
                letter = $"{qty:g5}";
            }
            else if (toSuffixExponent.TryGetValue(powerOf1000 * 3, out var letterSuffix))
            {
                letter = $"{qty / Math.Pow(1000d, powerOf1000):g5}{letterSuffix}";
            }

            var exp = $"{qty / Math.Pow(10d, powerOf10):g3}E+{powerOf10}";

            return (letter, exp);
        }

        public override string ToString()
        {
            var (letter, exp) = ToStrings(gv);

            return letter == null ? exp : $"{letter} ({exp})";
        }

        public static implicit operator double(GV gv) => gv.gv;

        public int CreditTier() => (int)Math.Floor(Math.Log10(gv)) - 7;

        public (double min, double max) TierRange()
        {
            var tier = Math.Floor(Math.Log10(gv));
            return (Math.Pow(10d, tier), Math.Pow(10d, Math.Min(tier + 1, Credits.MaxTier)));
        }
   }
}