using System;
using System.Collections.Generic;
using System.Linq;

namespace DMotion
{
    public static class IEnumerableUtils
    {
        public static int FindIndex<T>(this IEnumerable<T> source, Predicate<T> predicate)
        {
            var i = 0;
            foreach (var e in source)
            {
                if (predicate(e))
                {
                    return i;
                }
                i++;
            }
            return -1;
        }
    }
}