using System.Collections.Generic;

namespace uav.logic.Extensions;

public static class DictionaryExtensions
{
    public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> source, TKey key, TValue @default = default)
    {
        if (source.TryGetValue(key, out var val))
        {
            return val;
        }

        return @default;
    }
}