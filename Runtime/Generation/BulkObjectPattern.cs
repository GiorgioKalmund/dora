using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using SpaceFoundationSystem;
using UnityEditor;
using UnityEngine;

namespace giorgiokalmund.Dora.Generation
{
    public abstract class BulkObjectPattern : GenerationPattern
    {
        [field: SerializeField, Tooltip("")]
        public SerializableDictionary<GameObject, int> GenerationPool;

        [field: SerializeField, Tooltip("Holds the reference to the ids of generated items. ")]
        [field: ReadOnly]
        public List<string> GeneratedIds { get; protected set; }
        
        protected Dictionary<string, GenerationMember> Generated = new Dictionary<string, GenerationMember>();
        
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
                    var generation = memberHolder.AddComponent<GenerationMember>();
                    
                    OnFinishGeneration(memberHolder);
                    member.FindClosestAnchor();
                    
                    GeneratedIds.Add(generation.Id);
                    Generated.Add(generation.Id, generation);
                    EditorUtility.SetDirty(this);
                }   
            }
        }

        public override void Update()
        {
            if (Generated == null)
            {
                DoraLogger.LogWarning($"Cannot update [{GetType().Name}]. No generated items.");
                return;
            }

            List<string> keysToRemove = new List<string>();
            foreach (string genId in GeneratedIds)
            {
                if (!Generated.TryGetValue(genId, out _))
                {
                    var find = GenerationMember.Find(genId);
                    if (!find)
                    {
                        DoraLogger.LogWarning($"Could not find matching generated object for {genId}. Removing it from list of managed entries.");
                        keysToRemove.Add(genId);
                        continue;
                    }
                    
                    Generated[genId] = find;
                }

                if (Generated.TryGetValue(genId, out var obj) && obj != null && obj.gameObject != null && obj.gameObject.TryGetComponent(out LocationMember locMember))
                    locMember.FindClosestAnchor();
                else
                    keysToRemove.Add(genId);
            }

            foreach (var genId in keysToRemove)
                Remove(genId);
        }

        protected abstract Vector3 GetNextPosition();
        
        protected abstract void OnFinishGeneration(GameObject obj);

        public bool RequiresUpdate => Generated?.FirstOrDefault().Value == null;

        public override void Clear()
        {
            if (RequiresUpdate)
                Update();

            foreach (var generatedId in GeneratedIds)
                Remove(generatedId, true);
            GeneratedIds.Clear();
            Generated.Clear();
        }

        protected void Remove(string genId, bool soft = false)
        {
            Generated.TryGetValue(genId, out var go);
            if (go?.IsFixed ?? false)
                return;
            
            DestroyImmediate(go?.gameObject);
            if (!soft)
            {
                GeneratedIds.Remove(genId);
                Generated.Remove(genId);
            }
        }
    }
}