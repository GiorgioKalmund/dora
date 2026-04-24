using UnityEngine;

namespace giorgiokalmund.Dora.Generation.StdLib
{
    [CreateAssetMenu(fileName = "GroundedRandomNoise", menuName = "Dora/Generation/GroundedRandomNoise")]
    public class GroundedRandomNoisePattern : RandomNoisePattern, IPatternGrounder
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