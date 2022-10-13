using System;
using System.Collections.Generic;
using System.Linq;

namespace DMotion
{
    public static class IEnumerableUtils
    {
        public static int FindIndex<T>(this IEnumerable<T> source, Predicate<T> predicate)
        {
            return source.ToList().FindIndex(predicate);
        }
    }
}