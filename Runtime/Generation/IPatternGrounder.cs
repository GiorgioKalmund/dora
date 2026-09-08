using System;
using giorgiokalmund.Dora.Utils;
using SpaceFoundationSystem;
using UnityEngine;

/* TODO
 This file and concept needs reworking!
 */
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
        public static void Ground(this IPatternGrounder grounder, GameObject obj, LayerMask mask, GroundingMode mode)
        {
            switch (mode)
            {
                case GroundingMode.EXACT:
                {
                    var origin = obj.transform.position;
                    var direction = Vector3.down;
                    if (Physics.Raycast(origin, direction, out var hit, IPatternGrounder.MaxRaycastDistance, mask))
                        obj.transform.position = hit.point;
                    else
                        DoraLogger.LogError($"[{grounder.GetType().Name}]: Grounding raycast failed!\nMask: {mask.GetDisplayName()} ('{mask.value}')\nOrigin: {origin} - Direction: {direction}\nMaxDistance: {IPatternGrounder.MaxRaycastDistance}");
                } break;
                default:
                    throw new NotImplementedException($"GroundingMode {mode} is not handled by the helper!");
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