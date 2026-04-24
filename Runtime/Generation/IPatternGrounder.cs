using UnityEngine;

namespace giorgiokalmund.Dora.Generation
{
    public enum GroundingMode
    {
        // TODO: Come up with more modes
        EXACT
    }
    
    public interface IPatternGrounder
    {
        // TODO: More open interaction
        public const int MaxRaycastDistance = 100;
    }

    public static class PatternGrounderHelper
    {
        public static void Ground(this IPatternGrounder _, GameObject obj, LayerMask mask, GroundingMode mode)
        {
            if (Physics.Raycast(obj.transform.position, Vector3.down, out var hit, IPatternGrounder.MaxRaycastDistance, mask))
            {
                obj.transform.position = hit.point;
            }
            else
            {
                QuestLogger.LogWarning("Raycast failed!");
            }
        }
    }
}