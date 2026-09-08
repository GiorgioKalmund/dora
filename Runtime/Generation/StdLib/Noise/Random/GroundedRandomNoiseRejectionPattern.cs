using UnityEngine;

namespace giorgiokalmund.Dora.Generation.StdLib.Noise.Random
{
    [CreateAssetMenu(menuName = "Dora/Generation/Noise/Random/GroundedRandomNoiseRejectionPattern")]
    public class GroundedRandomNoiseRejectionPattern : RandomNoiseRejectionPattern, IPatternGrounder
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