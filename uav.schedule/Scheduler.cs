using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using log4net;

namespace uav.Schedule;

public class Scheduler
{
    private class JobInfo
    {
        public IJob? job;
        public Timer? timer;
    }
    private List<JobInfo> jobs = new List<JobInfo>();
    private bool started = false;
    private static readonly ILog logger = LogManager.GetLogger(typeof(Scheduler));

    public Scheduler()
    {
        
    }

    public void AddJob(IJob job)
    {
        var ji = new JobInfo { job = job };
        jobs.Add(ji);
        if (started)
        {
            _ = StartJob(ji);
        }
    }

    public void Start()
    {
        started = true;
        foreach (var job in jobs)
        {
            _ = StartJob(job);
        }
    }

    private async Task StartJob(JobInfo job)
    {
        var nextTime = await job.job!.NextJob();
        if (nextTime == null)
        {
            return;
        }

        logger.Debug($"Waiting for {nextTime} (setup: {job.job.Name})");
        var timeToNext = (long)nextTime.Value.TotalMilliseconds;
        job.timer = new Timer(OnTimerElapse, job, timeToNext, Timeout.Infinite);
    }

    private static async void OnTimerElapse(object? source)
    {
        var job = (JobInfo)source!;
        logger.Debug($"Starting job {job.job!.Name}");
        try {
            await job.job!.Run();
        }
        catch (Exception ex)
        {
            logger.Error($"Exception in job {job.job!.Name}", ex);
        }

        logger.Debug($"Completed job {job.job!.Name}");
        var timeToNext = await job.job.NextJob();
        if (timeToNext == null)
        {
            return;
        }
        job.timer!.Change((long)timeToNext.Value.TotalMilliseconds, Timeout.Infinite);
    }
}

