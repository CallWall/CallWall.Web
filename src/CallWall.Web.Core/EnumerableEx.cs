using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace CallWall.Web
{
    public static class EnumerableEx
    {
        public static HashSet<T> ToSet<T>(this IEnumerable<T> source)
        {
            return new HashSet<T>(source);
        }

        public static IReadOnlyDictionary<TKey, TValue> ToReadOnlyDictionary<TKey, TValue>(
            this IEnumerable<TValue> source, Func<TValue, TKey> keySelector)
        {
            return new ReadOnlyDictionary<TKey, TValue>(source.ToDictionary(keySelector));
        }
    }
}