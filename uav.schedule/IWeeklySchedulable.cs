using System;
using System.Collections.Generic;
using System.Linq;

namespace uav.Schedule;

public interface IWeeklySchedulable
{
    public DayOfWeek Day { get; }
    public TimeOnly? Time { get; }

    public bool IsUpcoming(DateTime now)
    {
        var nowTimeOnly = TimeOnly.FromDateTime(now);
        if (Day > now.DayOfWeek)
        {
            return true;
        }
        if (Day == now.DayOfWeek)
        {
            if (Time == null && nowTimeOnly == TimeOnly.MinValue)
            {
                return true;
            }
            if (Time != null && Time.Value > nowTimeOnly)
            {
                return true;
            }
        }

        return false;
    }
}

public static class IWeeklySchedulableExtensions
{
    public static bool IsUpcoming(this IWeeklySchedulable job, DateTime now)
    {
        return job.IsUpcoming(now);
    }

    public static bool HasPassed(this IWeeklySchedulable job, DateTime now)
    {
        if (job.Day < now.DayOfWeek)
        {
            return true;
        }
        if (job.Day == now.DayOfWeek)
        {
            if (job.Time == null || job.Time.Value <= TimeOnly.FromDateTime(now))
            {
                return true;
            }
        }

        return false;
    }

    public static DateTimeOffset NextTime(this IWeeklySchedulable job, DateTime now)
    {
        var nextTime = now.Date.AddDays(job.Day - now.DayOfWeek);
        if (job.Time != null)
        {
            nextTime = nextTime.AddTicks(job.Time.Value.Ticks);
        }
        if (nextTime < now)
        {
            nextTime = nextTime.AddDays(7);
        }
        return nextTime;
    }

    public static T? NextOccurring<T>(this ICollection<T> jobs, DateTime now) where T : IWeeklySchedulable
    {
        T? firstJob = default;
        bool foundFirst = false;
        foreach(var job in jobs)
        {
            if (job.IsUpcoming(now))
            {
                return job;
            }
            if (!foundFirst)
            {
                // this may be required for next week.
                firstJob = job;
                foundFirst = true;
            }
        }
        if (!foundFirst)
        {
            throw new ArgumentException("Why are there no jobs?");
        }
        return firstJob;
    }

    public static T? LastOccurred<T>(this ICollection<T> jobs, DateTime now) where T : IWeeklySchedulable
    {
        T? firstJob = default;
        bool foundFirst = false;
        foreach(var job in jobs.Reverse())
        {
            if (job.HasPassed(now))
            {
                return job;
            }
            if (!foundFirst)
            {
                // this may be from the previous week
                firstJob = job;
                foundFirst = true;
            }
        }
        if (!foundFirst)
        {
            throw new ArgumentException("Why are there no jobs?");
        }
        return firstJob;
    }
}
