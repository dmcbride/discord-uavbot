using uav.logic.Models;

namespace uav.test
{
    public class GvTest
    {
        [Test]
        [Arguments("100q")]
        [Arguments("100Q")]
        [Arguments("1E+85")]
        [Arguments("1E+15")]
        public async Task RoundTrip(string v)
        {
            var parsed = GV.TryFromString(v, out var gv, out var msg);
            await Assert.That(parsed).IsTrue();
            await Assert.That(gv!.ToString()).Contains(v);
        }

        [Test]
        [Arguments("1k", "1E+3")]
        public async Task Converts(string v, string expected)
        {
            var parsed = GV.TryFromString(v, out var gv, out var msg);
            await Assert.That(parsed).IsTrue();
            await Assert.That(gv!.ToString()).Contains(expected);
        }

        [Test]
        public async Task Tier()
        {
            GV.TryFromString("10M", out var gv, out var msg);
            await Assert.That(gv!.TierNumber).IsEqualTo(1);
        }

        [Test]
        public async Task regex_test()
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
          await Assert.That(gvBase).IsEqualTo("3.6E+24");
        }
    }
}