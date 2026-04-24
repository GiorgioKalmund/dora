using SpaceFoundationSystem;
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

        /// <summary>
        /// </summary>
        /// <remarks>
        /// Due to the nature that grounding can result in an object to not be inside any anchor region, but instead in the void of the delimiter region, no location
        /// might be assigned. Thus, we shift the entire member up visually and all of its children down. Now the member correctly tracks its position with no visual changes.
        /// <b>If this does not work for your use-case, please manually tweak the generated assets.</b>
        /// </remarks>
        public static void GroundLocationMemberVisually(this IPatternGrounder _, LocationMember member)
        {
            if (!member)
                return;
            var offsetY = SpaceFoundation.s_VoxelSize;
            member.transform.Translate(0, offsetY, 0);
            foreach (Transform child in member.transform)
            {
                child?.Translate(0, -offsetY, 0);
            }
        }
    }
}