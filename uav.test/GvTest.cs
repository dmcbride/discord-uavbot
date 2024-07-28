using Microsoft.VisualStudio.TestTools.UnitTesting;
using uav.logic.Models;

namespace uav.test
{
    [TestClass]
    public class GvTest
    {
        [DataTestMethod]
        [DataRow("100q")]
        [DataRow("100Q")]
        [DataRow("1E+85")]
        [DataRow("1E+15")]
        public void RoundTrip(string v)
        {
            var parsed = GV.TryFromString(v, out var gv, out var msg);
            Assert.IsTrue(parsed);
            StringAssert.Contains(gv!.ToString(), v);
        }

        [DataTestMethod]
        [DataRow("1k", "1E+3")]
        public void Converts(string v, string expected)
        {
            var parsed = GV.TryFromString(v, out var gv, out var msg);
            Assert.IsTrue(parsed);
            StringAssert.Contains(gv!.ToString(), expected);
        }

        [TestMethod]
        public void Tier()
        {
            GV.TryFromString("10M", out var gv, out var msg);
            Assert.AreEqual(1, gv!.TierNumber);
        }

        [TestMethod]
        public void regex_test()
        {
          var text = @"Estimating resolution as 443
SELL GALAXY

Sell your galaxy and start a new one in
exchange for precious credits

Galaxy Value $3.6E+24
Base Reward 1804 4
Lounge Bonus 3879 4s

Space Station 1250 4
Exodus Bonus 6933 4
Total Reward 13866 4

DOUBLE CREDITS
@)100

PACKAGING

Lv 44
Cargo x12.25
";
          var gvExtractor = new System.Text.RegularExpressions.Regex(@"Galaxy\s+Value\s*\$(\d+\.?\d*(?:E\+\d+|[a-zA-Z]{1,2}|0))");
          var gvMatch = gvExtractor.Match(text);
          var gvBase = gvMatch.Groups[1].Value;
          Assert.AreEqual("3.6E+24", gvBase);
        }
    }
}