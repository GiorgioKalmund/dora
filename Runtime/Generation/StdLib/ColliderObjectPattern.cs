using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using NUnit.Framework;
using SpaceFoundationSystem;
using UnityEngine;
using Space = SpaceFoundationSystem.Space;

namespace giorgiokalmund.Dora.Generation.StdLib
{
    public enum GenerationMode
    {
        ANCHOR,
        COLLIDER,
        SPHERE
    }
    
    public abstract class ColliderObjectPattern : BulkObjectPattern
    {
        [SerializeField] internal GenerationMode genMode = GenerationMode.ANCHOR;

        private const int MaxInsideOutsideChecks = 64;
        private static RaycastHit[] _raycastHitStorage = new RaycastHit[MaxInsideOutsideChecks];

        #region Anchor
        [SerializeField] 
        [Tooltip("Bounding mesh taken from an anchors corresponding region in the SFS graph.")]
        [ShowIf("genMode", GenerationMode.ANCHOR)] 
        [Dropdown("AllAnchors")]
        internal Anchor anchor;
        private List<Anchor> AllAnchors => SpaceFoundation.Current?.GetAnchors()?.Values.ToList() ?? new List<Anchor>();
        private Space _correspondingAnchorSpace;
        private MeshCollider _correspondingAnchorCollider;
        #endregion

        #region Collider
        [Tooltip("Holder of bounding mesh for the generation.")] [SerializeField]
        [ShowIf("genMode", GenerationMode.COLLIDER)]
        [RequiredType(typeof(Collider))]
        [Required]
        internal GameObject colliderBlueprintPrefab;
        protected Collider CustomCollider;
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


        protected sealed override Vector3 GetNextPosition()
        {
            switch (genMode)
            {
                case GenerationMode.ANCHOR:
                {
                    if (anchor)
                    {
                        if (!_correspondingAnchorSpace || !_correspondingAnchorSpace.anchor.Equals(anchor))
                        {
                            _correspondingAnchorSpace = FindObjectsByType<Space>().FirstOrDefault(s => s.anchor.Equals(anchor));
                            if (!_correspondingAnchorSpace)
                            {
                                DoraLogger.LogError($"[{GetType().Name}] The corresponding space to the anchor '{anchor.name}' could not be found!");
                                break;
                            }
                            
                            _correspondingAnchorCollider = _correspondingAnchorSpace.GetComponent<MeshCollider>();
                            if (!_correspondingAnchorCollider)
                            {
                                DoraLogger.LogError($"[{GetType().Name}] The corresponding space to the anchor '{anchor.name}' does not have a mesh collider! It should have one, as the SFS pass generates one.");
                                break;
                            }
                        }

                        return GetAlgorithmicPosition(_correspondingAnchorCollider, anchor);
                    }
                    
                    DoraLogger.LogError($"[{GetType().Name}] No location provided when!");
                    break;
                }
                case GenerationMode.COLLIDER:
                {
                    EnsureColliderExist();
                    return GetAlgorithmicPosition(CustomCollider);
                } 
                case GenerationMode.SPHERE:
                {
                    return new Vector3(radius * Random.value, radius * Random.value, radius * Random.value) + sphereCenter;
                }
            }

            return Vector3.zero;
        }

        /// <summary>
        /// Determines the algorithm to use for sampling a point in a collider.
        /// Override the base implementation with your own if desired.
        /// </summary>
        /// <remarks>Default to simple rejection sampling via <see cref="RejectionSampleInCollider"/>.</remarks>
        protected abstract Vector3 GetAlgorithmicPosition(Collider collider, Anchor location = null);
        
        public static bool IsInside(Anchor targetAnchor, Vector3 point)
        {
            Assert.IsNotNull(targetAnchor.correspondingSpaceFoundation, "anchor.correspondingSpaceFoundation != null");
            var sfs = targetAnchor.correspondingSpaceFoundation;

            return sfs.DetermineLocation(point)?.Equals(targetAnchor) ?? false;
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
            if (CustomCollider)
                obj.transform.SetParent(CustomCollider.gameObject.transform, true);
        }

        public override bool CanGenerate()
        {
            switch (genMode)
            {
                case GenerationMode.ANCHOR: return anchor;
                case GenerationMode.COLLIDER: return CustomCollider;
                default: return true;
            }
        }

        public override void Clear()
        {
            base.Clear();
            if (CustomCollider)
                DestroyImmediate(CustomCollider.gameObject);
        }

        private void EnsureColliderExist()
        {
            if (CustomCollider != null)
                return;
            
            var go = Instantiate(colliderBlueprintPrefab.gameObject);
            go.name = $"[GENERATED] - Instance Bounds ({colliderBlueprintPrefab.name})";
            CustomCollider = go.GetComponent<Collider>();
            if (!CustomCollider)
                DoraLogger.LogWarning("Cannot generate bounds as instance has no collider!");
        }
    }
}