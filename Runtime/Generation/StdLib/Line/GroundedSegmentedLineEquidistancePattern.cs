using UnityEngine;

namespace giorgiokalmund.Dora.Generation.StdLib.Line
{
    [CreateAssetMenu(menuName = "Dora/Generation/Line/GroundedSegmentedLineEquidistancePattern")]
    public class GroundedSegmentedLineEquidistancePattern : SegmentedLineEquidistancePattern, IPatternGrounder
    {
        [Header("Ground")]
        [SerializeField] protected GroundingMode mode;
        [SerializeField] protected LayerMask hitMask;
        protected override void OnFinishGeneration(GameObject obj)
        {
            base.OnFinishGeneration(obj);
            this.Ground(obj, hitMask, mode);
            this.GroundLocationMemberVisually(obj.GetComponent<LocationMember>());
        }
    }
}