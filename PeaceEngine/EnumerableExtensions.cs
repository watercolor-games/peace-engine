using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plex.Engine
{
    public static class EnumerableExtensions
    {
        public static bool CountLessThan<T>(this IEnumerable<T> enumerable, int count)
        {
            return enumerable.Count() < count;
        }

        public static IEnumerable<T> Yield<T>(params T[] values)
        {
            foreach (var v in values)
                yield return v;
        }
    }
}
