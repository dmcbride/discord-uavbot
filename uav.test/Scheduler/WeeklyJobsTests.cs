using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using uav.Schedule;

namespace uav.test.Scheduler;

[TestClass]
public class WeeklyJobsTests
{
    private class TestWeeklyJob : WeeklyJobs
    {
        protected override ICollection<JobDescription> jobDescriptions => new JobDescription[] {
            J(DayOfWeek.Sunday, "1", new TimeOnly(1, 0)),
            J(DayOfWeek.Sunday, "2", new TimeOnly(12, 0)),
            J(DayOfWeek.Friday, "F12", new TimeOnly(12, 0)),
            J(DayOfWeek.Saturday, "3"),
        };

        public string? LastAction { get; private set; }
        private Func<Task> A(string action)
        {
            return () => {
                LastAction = action;
                return Task.CompletedTask;
            };
        }
        private JobDescription J(DayOfWeek dow, string Action, TimeOnly? time = null) =>
            new JobDescription(dow, Action, A(Action), time);

        protected override DateTime Now => FakeNow;
        public DateTime FakeNow { get; set; }

        public void StartOf(DayOfWeek day) {
            AddDays(-(int)FakeNow.DayOfWeek + (int)day);
            FakeNow = FakeNow.Date;
        }
        public void AddDays(double days) => FakeNow = FakeNow.AddDays(days);
        public void AddHours(double hours) => FakeNow = FakeNow.AddHours(hours);
        public string NowLooksLike() => $"{Now.DayOfWeek} @ {Now.TimeOfDay}";
    }

    [TestMethod]
    public async Task WeeklyJobs_Should_HandleTimes()
    {
        var j = new TestWeeklyJob();

        j.FakeNow = DateTime.Now.Date;
        j.AddDays(-(int)j.FakeNow.DayOfWeek); // this should bring us to Sunday midnight.

        Assert.AreEqual("1", j.Name, j.NowLooksLike());
        await j.Run();
        Assert.AreEqual("3", j.LastAction, j.NowLooksLike());

        j.AddHours(1);
        Assert.AreEqual("2", j.Name, j.NowLooksLike());
        await j.Run();
        Assert.AreEqual("1", j.LastAction, j.NowLooksLike());

        j.AddHours(12);
        Assert.AreEqual("F12", j.Name, j.NowLooksLike());
        await j.Run();
        Assert.AreEqual("2", j.LastAction, j.NowLooksLike());

        j.StartOf(DayOfWeek.Friday);
        Assert.AreEqual("F12", j.Name, j.NowLooksLike());
        await j.Run();
        Assert.AreEqual("2", j.LastAction, j.NowLooksLike());

        j.AddHours(13);
        Assert.AreEqual("3", j.Name, j.NowLooksLike());
        await j.Run();
        Assert.AreEqual("F12", j.LastAction, j.NowLooksLike());

        j.StartOf(DayOfWeek.Saturday);
        Assert.AreEqual("3", j.Name, j.NowLooksLike());
        await j.Run();
        Assert.AreEqual("3", j.LastAction, j.NowLooksLike());

        j.AddDays(1);
        Assert.AreEqual("1", j.Name, j.NowLooksLike());
        await j.Run();
        Assert.AreEqual("3", j.LastAction, j.NowLooksLike());
    }

    private record JobType(DayOfWeek Day, TimeOnly? Time = null) : IWeeklySchedulable;

    [TestMethod]
    public void IWeeklySchedulable_Should_Work()
    {
        var now = DT(DayOfWeek.Friday, 0, 30);
        var job = new JobType(DayOfWeek.Friday, new TimeOnly(12, 0));
        Assert.IsTrue(job.IsUpcoming(now));

        var nextTime = job.NextTime(now);
        Assert.IsTrue(nextTime - now < new TimeSpan(24, 0, 0), $"Next time: {nextTime}; now: {now}");
    }

    private static DateTime _now = DateTime.UtcNow;
    private DateTime DT(DayOfWeek day, int hour = 0, int minute = 0)
    {
        var now = _now.Date;
        return now.AddDays(-(int)now.DayOfWeek + (int)day).AddHours(hour).AddMinutes(minute);
    }
}
