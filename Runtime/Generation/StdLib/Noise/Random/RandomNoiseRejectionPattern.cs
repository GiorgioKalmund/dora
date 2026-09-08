using SpaceFoundationSystem;
using UnityEngine;

namespace giorgiokalmund.Dora.Generation.StdLib.Noise.Random
{
    [CreateAssetMenu(menuName = "Dora/Generation/Noise/Random/RandomNoiseRejectionPattern")]
    public class RandomNoiseRejectionPattern : ColliderObjectPattern
    {
        protected override Vector3 GetAlgorithmicPosition(Collider collider, Anchor location = null)
        {
            var bounds = collider.bounds;
            Vector3 randomPoint;
            do
            {
                randomPoint = new Vector3(
                    UnityEngine.Random.Range(bounds.min.x, bounds.max.x),
                    UnityEngine.Random.Range(bounds.min.y, bounds.max.y),
                    UnityEngine.Random.Range(bounds.min.z, bounds.max.z)
                );
            } while (location ? !IsInside(location, randomPoint) : !IsInside(collider, randomPoint));
            return randomPoint;
        }
    }
}