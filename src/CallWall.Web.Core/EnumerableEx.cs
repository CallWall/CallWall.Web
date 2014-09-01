using System.Collections.Generic;

namespace CallWall.Web
{
    public static class EnumerableEx
    {
        public static HashSet<T> ToSet<T>(this IEnumerable<T> source)
        {
            return new HashSet<T>(source);
        }
    }
}