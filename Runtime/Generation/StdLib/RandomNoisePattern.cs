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
        [ValidateInput(nameof(ValidAnchorId), "Anchor id invalid")]
        [Dropdown("AnchorNames")]
        internal string anchorId;
        private MeshCollider _anchorMeshCollider;
        
        private bool ValidAnchorId(string id) { return !string.IsNullOrEmpty(id) && (SpaceFoundation.Current?.ValidAnchorId(id) ?? false); }
        private string GetAnchorDescription()
        {
            var anchor = SpaceFoundation.Current.TryGetAnchor(anchorId);
            return (ValidAnchorId(anchorId) && anchor ? $"{anchor.name}" : "Unknown Location");
        }
        private List<string> AnchorNames => SpaceFoundation.Current?.GetAnchors()?.Keys.ToList() ?? new List<string>();
        [ShowNativeProperty] private string BoundTo => GetAnchorDescription();
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
                    if (!string.IsNullOrEmpty(anchorId))
                    {
                        if (!_anchorMeshCollider)
                            _anchorMeshCollider = FindObjectsByType<Space>().FirstOrDefault(s => s.anchor.GetUniqueId().Equals(anchorId))?.gameObject.GetComponent<MeshCollider>();
                        if (!_anchorMeshCollider)
                        {
                            QuestLogger.LogWarning("Cannot find appropriate location to generate content!");
                            return Vector3.zero;
                        }
                        
                        //Debug.Log($"Searching inside mesh collider of {_anchorMeshCollider.gameObject.name}");
                        var bounds = _anchorMeshCollider.bounds;
                        var margin = 1f;
                        Vector3 randomPoint;
                        do
                        {
                            randomPoint = new Vector3(
                                Random.Range(bounds.min.x + margin, bounds.max.x - margin),
                                bounds.center.y,
                                Random.Range(bounds.min.z + margin, bounds.max.z - margin)
                            );
                        } while (!IsInside(_anchorMeshCollider, randomPoint));
                        return randomPoint;
                    }
                    
                    QuestLogger.LogWarning("No location provided!");
                    return Vector3.zero;
                }
                case GenerationMode.COLLIDER:
                {
                    if (GenerateInstance())
                    {
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
                    return Vector3.zero;
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
                case GenerationMode.ANCHOR: return ValidAnchorId(anchorId);
                case GenerationMode.COLLIDER: return _customCollider;
                default: return true;
            }
        }

        public override void Clear()
        {
            base.Clear();
            if (_customCollider)
                DestroyImmediate(_customCollider.gameObject);
            if (_anchorMeshCollider)
                _anchorMeshCollider.convex = false;
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
                    QuestLogger.LogWarning("Cannot generate bounds as instance has no collider!");
                    return false;
                }
            }

            return true;
        }
    }
}