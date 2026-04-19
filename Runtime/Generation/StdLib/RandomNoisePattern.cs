using UnityEngine;

namespace giorgiokalmund.Dora.Generation.StdLib
{
    [CreateAssetMenu(fileName = "RandomNoise", menuName = "Dora/Generation/RandomNoise")]
    public class RandomNoisePattern : BulkObjectPattern
    {
        // TODO: Only draw properties mutually exclusively
        [Header("Bounding Mesh")]
        [Tooltip("Bounding mesh for the generation.")]
        [SerializeField] private Collider templateBounds;
        [Header("Bounding Sphere")]
        [Tooltip("Bounding sphere's radius for the generation. Will ONLY be used if NO bounding mesh is provided!")]
        [SerializeField] private int radius;
        [Tooltip("Bounding sphere's center for the generation. Will ONLY be used if NO bounding mesh is provided!")]
        [SerializeField] private Vector3 sphereCenter;

        private Collider _instanceBounds;

        protected override Vector3 GetNextPosition()
        {
            if (templateBounds && GenerateInstance())
            {
                var bounds = _instanceBounds.bounds;
                return 
                       _instanceBounds.ClosestPoint(new Vector3(
                           Random.Range(bounds.min.x, bounds.max.x), 
                           Random.Range(bounds.min.y, bounds.max.y), 
                           Random.Range(bounds.min.z, bounds.max.z)
                       ));
            }
            
            return new Vector3(radius * Random.value, radius * Random.value, radius * Random.value) + sphereCenter;
        }
        
        protected override void OnFinishGeneration(GameObject obj)
        {
            if (_instanceBounds)
                obj.transform.SetParent(_instanceBounds.gameObject.transform, true);
        }

        public override bool CanGenerate() => true;

        public override void Clear()
        {
            base.Clear();
            if (_instanceBounds)
                DestroyImmediate(_instanceBounds.gameObject);
        }

        private bool GenerateInstance()
        {
            if (_instanceBounds == null)
            {
                var go = Instantiate(templateBounds.gameObject);
                go.name = $"[GENERATED] - Instance Bounds ({templateBounds.name})";
                _instanceBounds = go.GetComponent<Collider>();
                if (!_instanceBounds)
                {
                    QuestLogger.LogWarning("Cannot generate bounds as instance has no collider!");
                    return false;
                }
            }

            return true;
        }
    }
}