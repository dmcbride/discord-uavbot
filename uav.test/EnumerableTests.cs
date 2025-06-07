using System.Linq;
using uav.logic.Extensions;

namespace uav.test
{
    public class EnumerableTests
    {
        [Test]
        public async Task NAtATime_Should_CaptureTheRightSize()
        {
            var numbers = Enumerable.Range(1,9);
            foreach (var group in numbers.NAtATime(3))
            {
                await Assert.That(group.Count()).IsEqualTo(3);
                await Assert.That(group[0] % 3).IsEqualTo(1);
                await Assert.That(group[1] % 3).IsEqualTo(2);
                await Assert.That(group[2] % 3).IsEqualTo(0);
            }
        }
    }
}