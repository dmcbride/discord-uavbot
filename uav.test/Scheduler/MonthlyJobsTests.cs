using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using uav.Schedule;

namespace uav.test.Scheduler;

[TestClass]
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

        public string LastAction { get; private set; }
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

    [TestMethod]
    public async Task MonthlyJobs_Should_HandleTimes()
    {
        var j = new TestMonthlyJob();

        j.FakeNow = new DateTime(2022, 3, 28, 0, 30, 0);
        j.AddDays(-(int)j.FakeNow.Day + 1); // this should bring us to midnight beginning of month.

        Assert.AreEqual("1", j.Name, j.NowLooksLike());
        await j.Run();
        Assert.AreEqual("4", j.LastAction, j.NowLooksLike());

        j.AddHours(1);
        Assert.AreEqual("2", j.Name, j.NowLooksLike());
        await j.Run();
        Assert.AreEqual("1", j.LastAction, j.NowLooksLike());

        j.AddHours(12);
        Assert.AreEqual("3", j.Name, j.NowLooksLike());
        await j.Run();
        Assert.AreEqual("2", j.LastAction, j.NowLooksLike());

        j.StartOf(28);
        Assert.AreEqual("3", j.Name, j.NowLooksLike());
        await j.Run();
        Assert.AreEqual("3", j.LastAction, j.NowLooksLike());

        j.AddDays(1);
        Assert.AreEqual("4", j.Name, j.NowLooksLike());
        await j.Run();
        Assert.AreEqual("3", j.LastAction, j.NowLooksLike());

        j.AddHours(13);
        Assert.AreEqual("1", j.Name, j.NowLooksLike());
        await j.Run();
        Assert.AreEqual("4", j.LastAction, j.NowLooksLike());
    }

    private record JobType(int RelativeDay, TimeOnly? Time = null) : IMonthlySchedulable;

    [TestMethod]
    public void IMonthlySchedulable_Should_Work()
    {
        var now = new DateTime(2022, 3, 28, 0, 30, 0);
        var job = new JobType(-3, new TimeOnly(12, 0));
        Assert.IsTrue(job.IsUpcoming(now));

        var nextTime = job.NextTime(now);
        Assert.IsTrue(nextTime - now < new TimeSpan(24, 0, 0), $"Next time: {nextTime}; now: {now}");

        job = new JobType(1, null);
        nextTime = job.NextTime(now);
        Assert.IsTrue(nextTime > now);
    }
}
