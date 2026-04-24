using SpaceFoundationSystem;
using UnityEngine;

namespace giorgiokalmund.Dora.Generation.StdLib
{
    [CreateAssetMenu(fileName = "GroundedLine", menuName = "Dora/Generation/GroundedLine")]
    public class GroundedLinePattern : LinePattern, IPatternGrounder
    {
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