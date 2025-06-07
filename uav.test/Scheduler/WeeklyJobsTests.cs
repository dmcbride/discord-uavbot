using System;
using System.Collections.Generic;
using uav.Schedule;

namespace uav.test.Scheduler;

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

    [Test]
    public async Task WeeklyJobs_Should_HandleTimes()
    {
        var j = new TestWeeklyJob();

        j.FakeNow = DateTime.Now.Date;
        j.AddDays(-(int)j.FakeNow.DayOfWeek); // this should bring us to Sunday midnight.

        await Assert.That(j.Name).IsEqualTo("1").Because(j.NowLooksLike());
        await j.Run();
        await Assert.That(j.LastAction).IsEqualTo("3").Because(j.NowLooksLike());

        j.AddHours(1);
        await Assert.That(j.Name).IsEqualTo("2").Because(j.NowLooksLike());
        await j.Run();
        await Assert.That(j.LastAction).IsEqualTo("1").Because(j.NowLooksLike());

        j.AddHours(12);
        await Assert.That(j.Name).IsEqualTo("F12").Because(j.NowLooksLike());
        await j.Run();
        await Assert.That(j.LastAction).IsEqualTo("2").Because(j.NowLooksLike());

        j.StartOf(DayOfWeek.Friday);
        await Assert.That(j.Name).IsEqualTo("F12").Because(j.NowLooksLike());
        await j.Run();
        await Assert.That(j.LastAction).IsEqualTo("2").Because(j.NowLooksLike());

        j.AddHours(13);
        await Assert.That(j.Name).IsEqualTo("3").Because(j.NowLooksLike());
        await j.Run();
        await Assert.That(j.LastAction).IsEqualTo("F12").Because(j.NowLooksLike());

        j.StartOf(DayOfWeek.Saturday);
        await Assert.That(j.Name).IsEqualTo("3").Because(j.NowLooksLike());
        await j.Run();
        await Assert.That(j.LastAction).IsEqualTo("3").Because(j.NowLooksLike());

        j.AddDays(1);
        await Assert.That(j.Name).IsEqualTo("1").Because(j.NowLooksLike());
        await j.Run();
        await Assert.That(j.LastAction).IsEqualTo("3").Because(j.NowLooksLike());
    }

    private record JobType(DayOfWeek Day, TimeOnly? Time = null) : IWeeklySchedulable;

    [Test]
    public async Task IWeeklySchedulable_Should_Work()
    {
        var now = DT(DayOfWeek.Friday, 0, 30);
        var job = new JobType(DayOfWeek.Friday, new TimeOnly(12, 0));
        await Assert.That(job.IsUpcoming(now)).IsTrue();

        var nextTime = job.NextTime(now);
        await Assert.That(nextTime - now).IsLessThan(new TimeSpan(24, 0, 0));
    }

    private static DateTime _now = DateTime.UtcNow;
    private DateTime DT(DayOfWeek day, int hour = 0, int minute = 0)
    {
        var now = _now.Date;
        return now.AddDays(-(int)now.DayOfWeek + (int)day).AddHours(hour).AddMinutes(minute);
    }
}
