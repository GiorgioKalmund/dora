using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
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
        private Space _correspondingAnchorSpace;
        private MeshCollider _correspondingAnchorCollider;
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
                    if (GenerateInstance())
                    {
                        return GetAlgorithmicPosition(_customCollider);
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

        /// <summary>
        /// Determines the algorithm to use for sampling a point in a collider.
        /// Override the base implementation with your own if desired.
        /// </summary>
        /// <remarks>Default to simple rejection sampling via <see cref="RejectionSampleInBounds"/>.</remarks>
        protected virtual Vector3 GetAlgorithmicPosition(Collider collider, Anchor location = null)
        {
            // Default to using RejectionSampling
            return RejectionSampleInBounds(collider.bounds, location);
        }
        
        private Vector3 RejectionSampleInBounds(Bounds bounds, Anchor checkAnchor = null)
        {
            Vector3 randomPoint;
            do
            {
                randomPoint = new Vector3(
                    Random.Range(bounds.min.x, bounds.max.x),
                    Random.Range(bounds.min.y, bounds.max.y),
                    Random.Range(bounds.min.z, bounds.max.z)
                );
            } while (checkAnchor ? !IsInside(checkAnchor, randomPoint) : !IsInside(_customCollider, randomPoint));
            return randomPoint;
        }

        public static bool IsInside(Anchor anchor, Vector3 point)
        {
            return anchor.correspondingSpaceFoundation.DetermineLocation(point).Equals(anchor);
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