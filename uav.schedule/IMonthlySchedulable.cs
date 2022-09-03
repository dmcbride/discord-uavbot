using System;
using System.Collections.Generic;
using System.Linq;

namespace uav.Schedule;

public interface IMonthlySchedulable
{
    public int RelativeDay { get; }
    public TimeOnly? Time { get; }

    public int Day(DateTime now)
    {
        var dom = RelativeDay;
        if (dom < 1)
        {
            var lastDayThisMonth = DateTime.DaysInMonth(now.Year, now.Month);
            dom = lastDayThisMonth + dom;
        }

        return dom;
    }

    public bool IsUpcoming(DateTime now)
    {
        var dom = Day(now);

        if (dom > now.Day)
        {
            return true;
        }
        if (dom == now.Day)
        {
            var nowTimeOnly = TimeOnly.FromDateTime(now);
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

    public DateTime NextTime(DateTime now)
    {
        var dom = Day(now);
        var nowTimeOnly = TimeOnly.FromDateTime(now);
        if (dom < now.Day ||
            dom == now.Day && (
                (Time == null && nowTimeOnly != TimeOnly.MinValue) ||
                (Time != null && TimeOnly.FromDateTime(now) > Time)
            )
        )
        {
            // need to go to next month.
            now = now.Date.AddDays(1 - now.Day).AddMonths(1);
            dom = Day(now);
        }
        var nextTime = now.Date.AddDays(dom - now.Day);
        if (Time != null)
        {
            nextTime = nextTime.AddTicks(Time.Value.Ticks);
        }
        return nextTime;
    }
}

public static class IMonthlySchedulableExtensions
{
    public static bool IsUpcoming(this IMonthlySchedulable job, DateTime now)
    {
        return job.IsUpcoming(now);
    }

    public static bool HasPassed(this IMonthlySchedulable job, DateTime now)
    {
        var dom = job.Day(now);
        if (dom < now.Day)
        {
            return true;
        }
        if (dom == now.Day)
        {
            if (job.Time == null || job.Time.Value <= TimeOnly.FromDateTime(now))
            {
                return true;
            }
        }

        return false;
    }

    public static DateTime NextTime(this IMonthlySchedulable job, DateTime now)
    {
        return job.NextTime(now);
    }

    public static T NextOccuring<T>(this ICollection<T> jobs, DateTime now) where T : IMonthlySchedulable
    {
        T nextJob = default;
        var foundAny = false;
        // we have to do the sort because a job day of "-3" could be before the 27th some months and after the 27th other months
        var orderedJobs = jobs.OrderBy(j => j.NextTime(now)).ToArray();
        foreach(var job in orderedJobs)
        {
            if (job.IsUpcoming(now))
            {
                return job;
            }
            if (!foundAny)
            {
                // this may be required for next month.
                nextJob = job;
                foundAny = true;
            }
        }
        if (!foundAny)
        {
            throw new ArgumentException("Why are there no jobs?");
        }
        return nextJob;
    }

    public static T LastOccurred<T>(this ICollection<T> jobs, DateTime now) where T : IMonthlySchedulable
    {
        T firstJob = default;
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
