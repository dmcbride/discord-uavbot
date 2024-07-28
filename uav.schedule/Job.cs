using System;
using System.Threading.Tasks;

namespace uav.Schedule;

public interface IJob
{
    public Task<TimeSpan?> NextJob();

    public Task Run();

    public string Name { get; }
}

public abstract class Job : IJob
{
    protected Job()
    {
    }

    public virtual async Task<TimeSpan?> NextJob()
    {
        var nextJobTime = await NextJobTime();
        if (nextJobTime == null)
        {
            return null;
        }
        return (nextJobTime.Value.ToUniversalTime() - DateTimeOffset.UtcNow).Add(new TimeSpan(0, 0, 0, 0, 1));
    }

    public abstract Task<DateTimeOffset?> NextJobTime();

    public abstract Task Run();

    public abstract string Name { get; }
}
