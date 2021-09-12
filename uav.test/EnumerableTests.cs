using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using uav.logic.Extensions;

namespace uav.test
{
    [TestClass]
    public class EnumerableTests
    {
        [TestMethod]
        public void NAtATime_Should_CaptureTheRightSize()
        {
            var numbers = Enumerable.Range(1,9);
            foreach (var group in numbers.NAtATime(3))
            {
                Assert.AreEqual(3, group.Count());
                Assert.AreEqual(1, group[0] % 3);
                Assert.AreEqual(2, group[1] % 3);
                Assert.AreEqual(0, group[2] % 3);
            }
        }
    }
}