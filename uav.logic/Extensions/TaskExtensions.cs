using System;
using System.Collections.Generic;

namespace uav.logic.Extensions;

public static class TaskExtensions
{
    public static async IAsyncEnumerable<TOut> SelectAsync<TIn, TOut>(this IEnumerable<TIn> source, Func<TIn, Task<TOut>> selector)
    {
        foreach (var item in source)
        {
            yield return await selector(item);
        }
    }

    public static async Task ParallelThrottled(this IEnumerable<Func<Task>> taskFactories, int maxDegreeOfParallelism)
    {
        var tasksQueue = new Queue<Func<Task>>(taskFactories);
        var runningTasks = new List<Task>(maxDegreeOfParallelism);
        while (tasksQueue.Count > 0 || runningTasks.Count > 0)
        {
            while (runningTasks.Count < maxDegreeOfParallelism && tasksQueue.Count > 0)
            {
                var taskFactory = tasksQueue.Dequeue();
                runningTasks.Add(taskFactory());
            }

            var completedTask = await Task.WhenAny(runningTasks);
            runningTasks.Remove(completedTask);
        }

        await Task.WhenAll(runningTasks);
    }
}