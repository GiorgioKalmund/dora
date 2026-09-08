using System.Linq;
using UnityEngine;

namespace giorgiokalmund.Dora.Utils
{
    public static class LayerMaskUtils
    {
        public static string GetDisplayName(this LayerMask mask)
        {
            if (mask.value == 0)
                return "Nothing";

            if (mask.value == -1)
                return "Everything";

            var layers = Enumerable.Range(0, 32)
                .Where(i => (mask.value & (1 << i)) != 0)
                .Select(LayerMask.LayerToName)
                .Where(name => !string.IsNullOrEmpty(name));

            return string.Join(", ", layers);
        }
    }
}