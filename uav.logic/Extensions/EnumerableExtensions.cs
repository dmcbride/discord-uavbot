using System;
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

        public static IEnumerable<T> AndThen<T>(this T source, params T[] next) => source.AndThen((IEnumerable<T>)next);

        public static IEnumerable<T> AndThen<T>(this IEnumerable<T> source, params T[] next) => source.Concat(next);
        public static IEnumerable<T> AndThen<T>(this IEnumerable<T> source, IEnumerable<T> next) => source.Concat(next);

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

        static Random rng = new Random();
        public static void Shuffle<T>(this IList<T> source)
        {
            for (int i = 0; i < source.Count; i++)
            {
                var swap = rng.Next(source.Count - i) + i;
                (source[i], source[swap]) = (source[swap], source[i]);
            }
        }

        public static IEnumerable<T> NaturalOrderBy<T>(this IEnumerable<T> source, Func<T, string> selector)
        {
            return source.OrderBy(selector, new NaturalStringComparer());
        }
    }
}