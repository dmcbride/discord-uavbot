using System;
using System.Collections.Generic;
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
                .Select((s, i) => (s, i: 3*(i+1)))
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
            if (!TryFromString(v, out var GV, out var errorMessage))
            {
                throw new Exception(errorMessage);
            }
            return GV;
        }

        public static GV Zero = FromNumber(0);

        public static bool TryFromString(string v, out GV gv, out string errorMessage)
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
                    var allowedSuffixes = $"Suffixes are allowed to be one of: {string.Join(",", recommendedSuffixes)}";
                    if (suffixExponent.TryGetValue(suffix.ToLowerInvariant(), out _) ||
                        suffixExponent.TryGetValue(suffix.ToUpperInvariant(), out _))
                    {
                        errorMessage = $"Perhaps you got the case wrong. {allowedSuffixes}.";
                    }
                    else
                    {
                        errorMessage = $"Perhaps you misspelled a letter. {allowedSuffixes}.";
                    }
                    return false;
                }
            }
            else if ((m = ExpNumber.Match(v)).Success)
            {
                qty = Double.Parse(FixComma(m.Groups["qty"].Value));
                qty *= Math.Pow(10d, Double.Parse(FixComma(m.Groups["exp"].Value)));
            }
            else
            {
                return false;
            }

            if (!Double.IsFinite(qty))
            {
                errorMessage = "CAREFUL! The money amount is too high, and it will likely irreversibly bug out IPM.";
                return false;
            }
            if (qty < 1.0 && qty != 0.0)
            {
                errorMessage = "The money amount is too low to mean anything reasonable in IPM.";
                return false;
            }

            gv = new GV(qty);
            return true;
        }

        public int TierNumber => Exponential - 6;
        public int Exponential => (int)Math.Floor(Math.Log10(gv));
  
        private static (string letter, string exp) ToStrings(double qty)
        {
            if (qty == 0.0)
            {
                return (null, "0");
            }

            var powerOf10 = (int) Math.Floor(Math.Log10(qty));
            var powerOf1000 = powerOf10 / 3;

            string letter = null;
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

        public static bool operator <(GV a, GV b) => a.gv < b.gv;
        public static bool operator >(GV a, GV b) => a.gv > b.gv;
        public static bool operator <=(GV a, GV b) => a.gv <= b.gv;
        public static bool operator >=(GV a, GV b) => a.gv >= b.gv;

        public static GV operator +(GV a, GV b) => new GV(a.gv + b.gv);
        public static GV operator -(GV a, GV b) => new GV(a.gv - b.gv);
        public static GV operator *(GV a, double b) => new GV(a.gv * b);
        public static GV operator *(double a, GV b) => new GV(a * b.gv);

        public int CreditTier() => (int)Math.Floor(Math.Log10(gv)) - 7;

        public (double min, double max) TierRange()
        {
            var tier = Math.Floor(Math.Log10(gv));
            return (Math.Pow(10d, tier), Math.Pow(10d, Math.Min(tier + 1, Credits.MaxTier)));
        }
    }
}