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
            StringAssert.Contains(gv.ToString(), v);
        }

        [DataTestMethod]
        [DataRow("1k", "1E+3")]
        public void Converts(string v, string expected)
        {
            var parsed = GV.TryFromString(v, out var gv, out var msg);
            Assert.IsTrue(parsed);
            StringAssert.Contains(gv.ToString(), expected);
        }

        [TestMethod]
        public void Tier()
        {
            GV.TryFromString("10M", out var gv, out var msg);
            Assert.AreEqual(1, gv.TierNumber);
        }
    }
}