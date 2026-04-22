using UnityEngine;

namespace giorgiokalmund.Dora.Utils
{
    public static class VectorUtils
    {
        public static Vector3Int Floor(this Vector3 vec)
        {
            return new Vector3Int(Mathf.FloorToInt(vec.x), Mathf.FloorToInt(vec.y), Mathf.FloorToInt(vec.z));
        }
    }
}