using System.Collections.Generic;
using SpaceFoundationSystem;
using UnityEditor;
using UnityEngine;

namespace giorgiokalmund.Dora.Generation
{
    public abstract class BulkObjectPattern : GenerationPattern
    {
        [field: SerializeField, Tooltip("")]
        public SerializableDictionary<GameObject, int> GenerationPool;
        
        [field: ReadOnly]
        [field: SerializeField, Tooltip("")]
        public List<GameObject> Generated { get; protected set; }
        
        public override void Generate()
        {
            if (ShouldClearOnGenerate)
                Clear();
            var dict = GenerationPool.ToDictionary();
            if (dict.Keys.Count == 0 || !CanGenerate())
                return;
            foreach (var monoBehaviour in dict.Keys)
            {
                for (int i = 0; i < dict[monoBehaviour]; i++)
                {
                    var instance = PrefabUtility.InstantiatePrefab(monoBehaviour) as GameObject;
                    if (!instance)
                        continue;
                    instance.transform.position = GetNextPosition();
                    instance.name = $"[GENERATED] - {monoBehaviour.name} ({GetType()})";
                    var member = instance.AddComponent<LocationMember>();
                    member.FindClosestAnchor();
                    Generated.Add(instance);
                    OnFinishGeneration(instance);
                }
            }
        }

        protected abstract Vector3 GetNextPosition();
        
        protected abstract void OnFinishGeneration(GameObject obj);

        public override void Clear()
        {
            foreach (var monoBehaviour in Generated)
                DestroyImmediate(monoBehaviour.gameObject);
            Generated.Clear();
        }
    }
}