using System.Linq;
using giorgiokalmund.Dora.Generation;
using JetBrains.Annotations;
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
    public class LocationBehaviourRequirement : LocationRequirement, IPatternGenerator
    {
        [SerializeField] protected SerializableDictionary<GameObject, CountTracker> requiredObjects;
        [SerializeField] protected SerializableDictionary<MonoScript, CountTracker> requiredBehaviours;
        
        [SerializeField] [CanBeNull] protected GenerationPattern generationPattern;

        internal override string GetDescription()
        {
            return base.GetDescription() + $"\n\t{requiredBehaviours.ToDictionary().Count} Behaviours" + $"\n\t{requiredObjects.ToDictionary().Count} Scripts";;
        }

        protected override QuestValidationInformation HandleValidation()
        {
            var result = base.HandleValidation();
            if (result.IsFailure)
                return result;

            Anchor anchor = SpaceFoundation.TryGetAnchor(forLocation);
            if (!anchor) // TODO: Anchor data not filled in!
                return QuestValidationInformation.Failure( $"Could not get anchor object to validate location containment. Is the data properly set up?");

            var objects = requiredObjects.ToDictionary();
            if (objects != null)
            {
                foreach ((GameObject prefab, CountTracker tracker) in objects)
                {
                    // Primary check: Quick connected anchor comparison, else broader slower check for prefab instances.
                    var instances = FindObjectsByType<LocationMember>().Where(m => anchor.Equals(m.connectedAnchor) || prefab.Equals(PrefabUtility.GetCorrespondingObjectFromOriginalSource(m.gameObject))).ToList();
                    foreach (var locationMember in instances)
                        locationMember.FindClosestAnchor();
                    // Secondary check: Ensure all found instances are actually contained within the location
                    var containing = instances.Where(go => anchor.Equals(go.gameObject.transform.DetermineLocation())).ToList(); 
                    int objectCount = containing.Count;
                    if (objectCount < tracker.count)
                        return QuestValidationInformation.Failure($"Not enough {prefab.name} present at {anchor.gameObject.name} ({forLocation}). Expected {tracker.count}, got {objectCount}.");
            
                    if (tracker.mode == CountMode.EXACT && objectCount > tracker.count)
                        return QuestValidationInformation.Failure($"Not exact amount of {prefab.name} present at {anchor.gameObject.name} ({forLocation}). Expected {tracker.count}, got {objectCount}.");
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
                        return QuestValidationInformation.Failure($"Not enough {script.GetClass().Name} present at {anchor.gameObject.name} ({forLocation}). Expected {tracker.count}, got {objectCount}.");
            
                    if (tracker.mode == CountMode.EXACT && objectCount > tracker.count)
                        return QuestValidationInformation.Failure($"Not exact amount of {script.GetClass().Name} present at {anchor.gameObject.name} ({forLocation}). Expected {tracker.count}, got {objectCount}.");
                }
            }
            
            return QuestValidationInformation.Success();
        }

        GenerationPattern IPatternGenerator.GetCurrentPattern()
        {
            return generationPattern;
        }
    }
}