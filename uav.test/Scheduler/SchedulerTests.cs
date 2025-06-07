using System;
using uav.Schedule;

namespace uav.test.Scheduler;

public class SchedulerTests
{
    [Test]
    public async Task Scheduler_Should_Wait()
    {
        var scheduler = new uav.Schedule.Scheduler();
        var job = new TestJob();
        scheduler.AddJob(job);

        scheduler.Start();

        await Task.Delay(3000);

        await Assert.That(job.calls).IsEqualTo(0);
    }

    private class TestJob : Job
    {
        public override string Name => "Name!";
        public int calls = 0;

        public override Task<DateTimeOffset?> NextJobTime()
        {
            return Task.FromResult(DateTimeOffset.UtcNow.AddSeconds(60) as DateTimeOffset?);
        }

        public override Task Run()
        {
            calls++;
            
            return Task.CompletedTask;
        }
    }
}