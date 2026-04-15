using System.Collections.Generic;

namespace giorgiokalmund.Dora.Utils
{
    public static class HashSetUtil
    {
        public static bool AddRange<T>(this HashSet<T> set, T[] items)
        {
            foreach (var item in items)
            {
                var res = set.Add(item);
                if (!res)
                    return false;
            }
            return true;
        }
    }
}