using System.Collections.Generic;
using System.Linq;

namespace uav.logic.Extensions
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> AndThen<T>(this T source, IEnumerable<T> next)
        {
            return new[] {source}.Concat(next);
        }

        public static IEnumerable<IList<T>> NAtATime<T>(this IEnumerable<T> source, int n)
        {
            using var it = source.GetEnumerator();

            while (it.MoveNext())
            {
                var a = new T[n];
                for (int i = 0; i < n; i++)
                {
                    a[i] = it.Current;
                    if (i != n - 1 && !it.MoveNext())
                    {
                        break;
                    }
                }
                yield return a;
            }
        }
    }
}