using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace uav.logic.Extensions
{
    public static class TaskExtensions
    {
        public static async IAsyncEnumerable<TOut> SelectAsync<TIn, TOut>(this IEnumerable<TIn> source, Func<TIn, Task<TOut>> selector)
        {
            foreach (var item in source)
            {
                yield return await selector(item);
            }
        }
    }
}