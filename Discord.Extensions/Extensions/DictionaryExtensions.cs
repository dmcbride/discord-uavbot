namespace Discord.Extensions.Extensions;

public static class DictionaryExtensions
{
  public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> valueFactory)
  {
    if (dictionary.TryGetValue(key, out var value))
    {
      return value;
    }
    else
    {
      value = valueFactory(key);
      dictionary.Add(key, value);
      return value;
    }
  }

  public static TValue? GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue>? source, TKey key, TValue? @default = default)
  {
    if (source is not null && source.TryGetValue(key, out var val))
    {
      return val;
    }

    return @default;
  }

  public static TValue? GetOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue>? source, TKey key, TValue? @default = default)
  {
    if (source is not null && source.TryGetValue(key, out var val))
    {
      return val;
    }

    return @default;
  }

  public static TValue? GetOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue>? source, TKey key, Func<TValue?> @default)
  {
    if (source is not null && source.TryGetValue(key, out var val))
    {
      return val;
    }

    return @default();
  }

  // same as above, but taking a Dictionary directly
  public static TValue? GetOrDefault<TKey, TValue>(this Dictionary<TKey, TValue>? source, TKey key, TValue? @default = default)
    where TKey : notnull
  {
    return ((IDictionary<TKey, TValue>?)source).GetOrDefault(key, @default);
  }
}