using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using SpaceFoundationSystem;
using UnityEngine;
using Space = SpaceFoundationSystem.Space;

#if UNITY_EDITOR
using NaughtyAttributes.Editor;
#endif

namespace giorgiokalmund.Dora.Generation.StdLib
{
    public enum GenerationMode
    {
        ANCHOR,
        COLLIDER,
        SPHERE
    }
    
    [CreateAssetMenu(fileName = "RandomNoise", menuName = "Dora/Generation/RandomNoise")]
    public class RandomNoisePattern : BulkObjectPattern
    {
        [SerializeField] internal GenerationMode genMode = GenerationMode.ANCHOR;

        private static RaycastHit[] _raycastHitStorage = new RaycastHit[32];

        #region Anchor
        [SerializeField] 
        [Tooltip("Bounding mesh taken from an anchors corresponding region in the SFS graph.")]
        [ShowIf("genMode", GenerationMode.ANCHOR)] 
        [Dropdown("AllAnchors")]
        internal Anchor anchor;

        private List<Anchor> AllAnchors => SpaceFoundation.Current?.GetAnchors()?.Values.ToList() ?? new List<Anchor>();
        #endregion

        #region Collider
        [Tooltip("Holder of bounding mesh for the generation.")] [SerializeField]
        [ShowIf("genMode", GenerationMode.COLLIDER)]
        [RequiredType(typeof(Collider))]
        [Required]
        internal GameObject colliderBlueprintPrefab;
        private Collider _customCollider;
        #endregion
        
        #region Sphere
        [Tooltip("Bounding sphere's radius for the generation. Will ONLY be used if NO bounding mesh is provided!")] [SerializeField] 
        [ShowIf("genMode", GenerationMode.SPHERE)]
        private int radius;
        [Tooltip("Bounding sphere's center for the generation. Will ONLY be used if NO bounding mesh is provided!")]
        [SerializeField] 
        [ShowIf("genMode", GenerationMode.SPHERE)]
        private Vector3 sphereCenter;
        #endregion


        protected override Vector3 GetNextPosition()
        {
            switch (genMode)
            {
                case GenerationMode.ANCHOR:
                {
                    if (anchor)
                    {
                        // TODO: SubspacePositions is only temporary. No way to access all voxels for an anchor?
                        // TODO: ElementAt is O(N). Maybe for enough calls we can convert to list once and then sample O(1) 
                        return anchor.SubspacePositions.ElementAt(Random.Range(0, anchor.SubspacePositions.Count));
                    }
                    
                    DoraLogger.LogWarning("No location provided!");
                    break;
                }
                case GenerationMode.COLLIDER:
                {
                    if (GenerateInstance())
                    {
                        // TODO: Rejection sampling not optimal
                        var bounds = _customCollider.bounds;
                        var margin = 1f;
                        Vector3 randomPoint;
                        do
                        {
                            randomPoint = new Vector3(
                                Random.Range(bounds.min.x + margin, bounds.max.x - margin),
                                bounds.center.y,
                                Random.Range(bounds.min.z + margin, bounds.max.z - margin)
                            );
                        } while (!IsInside(_customCollider, randomPoint));
                        return randomPoint;
                    }
                    break;
                }
                case GenerationMode.SPHERE:
                {
                    return new Vector3(radius * Random.value, radius * Random.value, radius * Random.value) + sphereCenter;
                }
            }

            return Vector3.zero;
        }
        
        public static bool IsInside(Collider col, Vector3 point)
        {
            Ray ray = new Ray(point, Vector3.up);
            Physics.RaycastNonAlloc(ray, _raycastHitStorage,1000f);

            int count = 0;
            foreach (var hit in _raycastHitStorage)
                if (hit.collider == col)
                    count++;

            return count % 2 == 1;
        }
        
        protected override void OnFinishGeneration(GameObject obj)
        {
            if (_customCollider)
                obj.transform.SetParent(_customCollider.gameObject.transform, true);
        }

        public override bool CanGenerate()
        {
            switch (genMode)
            {
                case GenerationMode.ANCHOR: return anchor;
                case GenerationMode.COLLIDER: return _customCollider;
                default: return true;
            }
        }

        public override void Clear()
        {
            base.Clear();
            if (_customCollider)
                DestroyImmediate(_customCollider.gameObject);
        }

        private bool GenerateInstance()
        {
            if (_customCollider == null)
            {
                var go = Instantiate(colliderBlueprintPrefab.gameObject);
                go.name = $"[GENERATED] - Instance Bounds ({colliderBlueprintPrefab.name})";
                _customCollider = go.GetComponent<Collider>();
                if (!_customCollider)
                {
                    DoraLogger.LogWarning("Cannot generate bounds as instance has no collider!");
                    return false;
                }
            }

            return true;
        }
    }
}