using System.Linq;
using giorgiokalmund.Dora.Generation;
using giorgiokalmund.Dora.Generation.StdLib;
using JetBrains.Annotations;
using NaughtyAttributes;
using SpaceFoundationSystem;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace giorgiokalmund.Dora.Requirements
{
    [System.Serializable] 
    public struct CountTracker
    {
        public int count;
        public CountMode mode;

        public CountTracker(int count, CountMode mode)
        {
            this.count = count;
            this.mode = mode;
        }
    }
    
    [CreateAssetMenu(fileName = "LocationBehaviours", menuName = "Dora/Requirements/LocationBehaviours")]
    public class LocationBehaviourStep : LocationStep, IPatternGenerator
    {
        [SerializeField] protected SerializableDictionary<GameObject, CountTracker> requiredObjects;
        [SerializeField] protected SerializableDictionary<MonoScript, CountTracker> requiredBehaviours;
        
        [Header("Pattern Generation")]
        [Expandable]
        [SerializeField] [CanBeNull] protected GenerationPattern generationPattern;

        public override string GetDescription()
        {
            return base.GetDescription() + $"\n\t{requiredBehaviours.ToDictionary().Count} Behaviours" + $"\n\t{requiredObjects.ToDictionary().Count} Scripts";;
        }

        protected override QuestValidationInformation HandleValidation()
        {
            var result = base.HandleValidation();
            if (result.IsFailure)
                return result;

            if (!SpaceFoundation.Current.TryGetAnchor(anchorID, out Anchor anchor))
                return QuestValidationInformation.Failure($"Could not find anchor '{anchorID}' in scene.");

            var objects = requiredObjects.ToDictionary();
            if (objects != null)
            {
                foreach ((GameObject prefab, CountTracker tracker) in objects)
                {
                    // Primary check: Quick connected anchor comparison, else broader slower check for prefab instances.
                    var instances = FindObjectsByType<LocationMember>().Where(m => anchor.Equals(m.currentLocation) || prefab.Equals(PrefabUtility.GetCorrespondingObjectFromOriginalSource(m.gameObject))).ToList();
                    foreach (var locationMember in instances)
                        locationMember.FindClosestAnchor();
                    // Secondary check: Ensure all found instances are actually contained within the location
                    var containing = instances.Where(go => anchor.Equals(go.gameObject.transform.DetermineLocation())).ToList(); 
                    int objectCount = containing.Count;
                    if (objectCount < tracker.count)
                        return QuestValidationInformation.Failure($"Not enough {prefab.name} present at {anchor.gameObject.name} ({anchorID}). Expected {tracker.count}, got {objectCount}.");
            
                    if (tracker.mode == CountMode.EXACT && objectCount > tracker.count)
                        return QuestValidationInformation.Failure($"Not exact amount of {prefab.name} present at {anchor.gameObject.name} ({anchorID}). Expected {tracker.count}, got {objectCount}.");
                }
            }
            
            var scripts = requiredBehaviours.ToDictionary();
            if (scripts != null)
            {
                foreach ((MonoScript script, CountTracker tracker) in scripts)
                {
                    var type = script.GetClass();
                    if (type == null || !typeof(Object).IsAssignableFrom(type))
                        return QuestValidationInformation.Failure( $"MonoScript <b>{script.name}</b> does not inherit from UnityEngine.Object!");

                    var instances = FindObjectsByType(type).Where(o => o is MonoBehaviour).OfType<MonoBehaviour>().ToList();
                    var containing = instances.Where(go => anchor.Equals(go.gameObject.transform.DetermineLocation())).ToList();
                    int objectCount = containing.Count;
                    if (objectCount < tracker.count)
                        return QuestValidationInformation.Failure($"Not enough {script.GetClass().Name} present at {anchor.gameObject.name} ({anchorID}). Expected {tracker.count}, got {objectCount}.");
            
                    if (tracker.mode == CountMode.EXACT && objectCount > tracker.count)
                        return QuestValidationInformation.Failure($"Not exact amount of {script.GetClass().Name} present at {anchor.gameObject.name} ({anchorID}). Expected {tracker.count}, got {objectCount}.");
                }
            }
            
            return QuestValidationInformation.Success();
        }

        GenerationPattern IPatternGenerator.GetCurrentPattern()
        {
            if (generationPattern == null)
            {
                // pick appropriate pattern such as random grounded noise 
            }
            if (generationPattern is BulkObjectPattern bulk)
            {
                bulk.Clear();
                bulk.GenerationPool.Clear();
                foreach ((GameObject gameObject, CountTracker tracker) in requiredObjects.ToDictionary())
                {
                    bulk.GenerationPool.Add(gameObject, tracker.count);
                }
                EditorUtility.SetDirty(bulk);
            }
            return generationPattern;
        }
    }
}