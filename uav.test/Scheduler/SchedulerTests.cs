using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using uav.Schedule;

namespace uav.test.Scheduler;

[TestClass]
public class SchedulerTests
{
    [TestMethod]
    public async Task Scheduler_Should_Wait()
    {
        var scheduler = new uav.Schedule.Scheduler();
        var job = new TestJob();
        scheduler.AddJob(job);

        scheduler.Start();

        await Task.Delay(3000);


        Assert.AreEqual(0, job.calls);
    }

    private class TestJob : Job
    {
        public override string Name => "Name!";
        public int calls = 0;

        public override DateTimeOffset NextJobTime()
        {
            return DateTimeOffset.UtcNow.AddSeconds(60);
        }

        public override Task Run()
        {
            calls++;
            
            return Task.CompletedTask;
        }
    }
}