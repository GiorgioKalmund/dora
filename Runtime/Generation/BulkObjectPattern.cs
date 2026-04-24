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
                    
                    var memberHolder = new GameObject();
                    memberHolder.transform.position = GetNextPosition();
                    var member = memberHolder.AddComponent<LocationMember>();
                    
                    instance.transform.SetParent(memberHolder.transform,false);
                    instance.transform.localPosition = Vector3.zero;
                    memberHolder.name = $"[GENERATED] - {monoBehaviour.name} ({GetType()})";
                    
                    OnFinishGeneration(memberHolder);
                    member.FindClosestAnchor();
                    Generated.Add(memberHolder);
                }
            }
        }

        public override void Update()
        {
            foreach (var gameObject in Generated)
            {
                var member = gameObject.GetComponent<LocationMember>();
                if (member)
                    member.FindClosestAnchor();
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