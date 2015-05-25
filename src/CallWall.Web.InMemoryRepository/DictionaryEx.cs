using System;
using System.Collections.Generic;

namespace CallWall.Web.InMemoryRepository
{
    public static class DictionaryEx
    {
        public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> source, 
            TKey key,
            Func<TKey, TValue> defaultSelector)
        {
            TValue value;
            if (source.TryGetValue(key, out value))
            {
                return value;
            }
            return defaultSelector(key);
        }
    }
}