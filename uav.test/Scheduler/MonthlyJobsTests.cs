using System;
using System.Collections.Generic;
using uav.Schedule;

namespace uav.test.Scheduler;

public class MonthlyJobsTests
{
    private class TestMonthlyJob : MonthlyJobs
    {
        public TestMonthlyJob()
        {
        }

        protected override ICollection<JobDescription> jobDescriptions => new JobDescription[] {
            J(1, "1", new TimeOnly(1, 0)),
            J(1, "2", new TimeOnly(12, 0)),
            J(-3, "3"), // 28th
            J(-2, "4", new TimeOnly(12, 0)), // 29th
        };

        public string? LastAction { get; private set; }
        private Func<Task> A(string action)
        {
            return () => {
                LastAction = action;
                return Task.CompletedTask;
            };
        }
        private JobDescription J(int dom, string Action, TimeOnly? time = null) =>
            new JobDescription(dom, Action, A(Action), time);

        protected override DateTime Now => FakeNow;
        public DateTime FakeNow { get; set; }

        public void StartOf(int day) {
            if (day < 1)
            {
                day = DateTime.DaysInMonth(FakeNow.Year, FakeNow.Month) + day;
            }
            AddDays(-FakeNow.Day + day);
            FakeNow = FakeNow.Date;
        }
        public void AddDays(double days) => FakeNow = FakeNow.AddDays(days);
        public void AddHours(double hours) => FakeNow = FakeNow.AddHours(hours);
        public string NowLooksLike() => $"{Now.Day} @ {Now.TimeOfDay}";
    }

    [Test]
    public async Task MonthlyJobs_Should_HandleTimes()
    {
        var j = new TestMonthlyJob();

        j.FakeNow = new DateTime(2022, 3, 28, 0, 30, 0);
        j.AddDays(-(int)j.FakeNow.Day + 1); // this should bring us to midnight beginning of month.

        await Assert.That(j.Name).IsEqualTo("1").Because(j.NowLooksLike());
        await j.Run();
        await Assert.That(j.LastAction).IsEqualTo("4").Because(j.NowLooksLike());

        j.AddHours(1);
        await Assert.That(j.Name).IsEqualTo("2").Because(j.NowLooksLike());
        await j.Run();
        await Assert.That(j.Name).IsEqualTo("2").Because(j.NowLooksLike());

        j.AddHours(12);
        await Assert.That(j.Name).IsEqualTo("3").Because(j.NowLooksLike());
        await j.Run();
        await Assert.That(j.LastAction).IsEqualTo("2").Because(j.NowLooksLike());

        j.StartOf(28);
        await Assert.That(j.Name).IsEqualTo("3").Because(j.NowLooksLike());
        await j.Run();
        await Assert.That(j.LastAction).IsEqualTo("3").Because(j.NowLooksLike());

        j.AddDays(1);
        await Assert.That(j.Name).IsEqualTo("4").Because(j.NowLooksLike());
        await j.Run();
        await Assert.That(j.LastAction).IsEqualTo("3").Because(j.NowLooksLike());

        j.AddHours(13);
        await Assert.That(j.Name).IsEqualTo("1").Because(j.NowLooksLike());
        await j.Run();
        await Assert.That(j.LastAction).IsEqualTo("4").Because(j.NowLooksLike());
    }

    private record JobType(int RelativeDay, TimeOnly? Time = null) : IMonthlySchedulable;

    [Test]
    public async Task IMonthlySchedulable_Should_Work()
    {
        var now = new DateTime(2022, 3, 28, 0, 30, 0);
        var job = new JobType(-3, new TimeOnly(12, 0));
        await Assert.That(job.IsUpcoming(now)).IsTrue();

        var nextTimeNullable = job.NextTime(now);
        await Assert.That(nextTimeNullable).IsNotNull();
        var nextTime = nextTimeNullable!.Value;
        await Assert.That(nextTime! - now).IsLessThan(new TimeSpan(24, 0, 0));

        job = new JobType(1, null);
        nextTime = job.NextTime(now)!.Value;
        await Assert.That(nextTime).IsGreaterThan(now);
    }
}
