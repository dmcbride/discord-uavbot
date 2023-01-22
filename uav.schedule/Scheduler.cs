using System;
using System.Timers;
using System.Collections.Generic;
using System.Threading.Tasks;

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
        Console.WriteLine($"{DateTimeOffset.Now} - Waiting for {nextTime} (setup: {job.job.Name})");
        var timeToNext = nextTime.Value.TotalMilliseconds;
        job.timer = new Timer(timeToNext);
        job.timer.Elapsed += async (Object? source, ElapsedEventArgs e) => {
            Console.WriteLine($"{DateTimeOffset.Now} - Starting job {job.job.Name}");
            
            try {
                await job.job.Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{DateTimeOffset.Now} - Exception: {ex}");
            }
            Console.WriteLine($"{DateTimeOffset.Now} - Completed job {job.job.Name}");
            job.timer.Dispose();

            _ = StartJob(job);
        };
        job.timer.AutoReset = false;
        job.timer.Enabled = true;
    }
}

