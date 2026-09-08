using UnityEngine;

namespace giorgiokalmund.Dora.Generation.StdLib
{
    public abstract class GroundedColliderObjectPattern : ColliderObjectPattern, IPatternGrounder
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