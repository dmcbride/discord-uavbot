using System;
using System.Threading.Tasks;

namespace uav.Schedule;

public interface IJob
{
    public TimeSpan NextJob();

    public Task Run();

    public string Name { get; }
}

public abstract class Job : IJob
{
    protected Job()
    {
    }

    public virtual TimeSpan NextJob()
    {
        var nextJobTime = NextJobTime();
        return nextJobTime.ToUniversalTime() - DateTimeOffset.UtcNow;
    }

    public abstract DateTimeOffset NextJobTime();

    public abstract Task Run();

    public abstract string Name { get; }
}
