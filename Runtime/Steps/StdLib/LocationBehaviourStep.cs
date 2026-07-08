using System.Collections.Generic;
using System.Linq;
using giorgiokalmund.Dora.Generation;
using giorgiokalmund.Dora.Questing;
using JetBrains.Annotations;
using NaughtyAttributes;
using SpaceFoundationSystem;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace giorgiokalmund.Dora.Steps.StdLib
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
    
    [CreateAssetMenu(fileName = "LocationBehaviours", menuName = "Dora/Steps/LocationBehaviours")]
    public class LocationBehaviourStep : LocationStep, IPatternGenerator
    {
        [Header("Static Requirements")]
        [SerializeField] protected SerializableDictionary<GameObject, CountTracker> requiredObjects;
        [SerializeField] protected SerializableDictionary<MonoScript, CountTracker> requiredBehaviours;
        
        [Header("Pattern Generation")]
        [Expandable]
        [SerializeField] [CanBeNull] protected GenerationPattern generationPattern;

        /*
        public override string GetDescription()
        {
            return base.GetDescription() + $"\n\t{requiredBehaviours.ToDictionary().Count} Behaviours" + $"\n\t{requiredObjects.ToDictionary().Count} Scripts";;
        }
        */

        protected override QuestValidationInformation HandleValidation()
        {
            var result = base.HandleValidation();
            if (result.IsFailure)
                return result;

            var objects = requiredObjects.ToDictionary();
            if (objects != null)
            {
                foreach ((GameObject prefab, CountTracker tracker) in objects)
                {
                    // TODO: SLOW!
                    var allMembers = FindObjectsByType<LocationMember>(); 
                    
                    // Ensure all found instances are actually contained within the location
                    List<LocationMember> containing = new ();
                    foreach (var locationMember in allMembers)
                    {
                        locationMember.FindClosestAnchor();
                        if (locationMember.currentLocation 
                            && locationMember.currentLocation.Equals(anchorID)  // Same location
                            && PrefabUtility.GetCorrespondingObjectFromOriginalSource(locationMember.gameObject)) // Actual instance of prefab
                            containing.Add(locationMember);
                    }
                    
                    int objectCount = containing.Count;
                    switch (tracker.mode)
                    {
                        case CountMode.MINIMUM:
                        {
                            if (objectCount < tracker.count)
                                return QuestValidationInformation.Failure($"Not enough {prefab.name} present at {SpaceFoundation.Current.GetAnchorName(anchorID)} ({anchorID}). Expected {tracker.count}, got {objectCount}.");
                            break;
                        }
                        case CountMode.EXACT:
                        {
                            if (objectCount != tracker.count)
                                return QuestValidationInformation.Failure($"Not exact amount of {prefab.name} present at {SpaceFoundation.Current.GetAnchorName(anchorID)} ({anchorID}). Expected {tracker.count}, got {objectCount}.");
                            break;
                        }
                        case CountMode.MAXIMUM:
                        {
                            if (objectCount > tracker.count)
                                return QuestValidationInformation.Failure($"Too many {prefab.name} present at {SpaceFoundation.Current.GetAnchorName(anchorID)} ({anchorID}). Expected {tracker.count}, got {objectCount}.");
                            break;
                        }
                    }
            
                }
            }
            
            var scripts = requiredBehaviours.ToDictionary();
            if (scripts != null)
            {
                foreach ((MonoScript script, CountTracker tracker) in scripts)
                {
                    if (!script)
                        return QuestValidationInformation.Failure( "Empty MonoScript entry!");
                    
                    var type = script.GetClass();
                    if (type == null || !typeof(Object).IsAssignableFrom(type))
                        return QuestValidationInformation.Failure( $"MonoScript <b>{script.name}</b> does not inherit from UnityEngine.Object!");

                    // TODO: Slow!
                    var instances = FindObjectsByType(type).Where(o => o is MonoBehaviour).OfType<MonoBehaviour>().ToList();
                    var containing = instances.Where(go => go.gameObject.transform.DetermineLocation().Equals(anchorID)).ToList();
                    
                    int objectCount = containing.Count;
                    switch (tracker.mode)
                    {
                        case CountMode.MINIMUM:
                        {
                            if (objectCount < tracker.count)
                                return QuestValidationInformation.Failure($"Not enough {script.GetClass().Name} present at {SpaceFoundation.Current.GetAnchorName(anchorID)} ({anchorID}). Expected {tracker.count}, got {objectCount}.");
                            break;
                        }
                        case CountMode.EXACT:
                        {
                            if (objectCount != tracker.count)
                                return QuestValidationInformation.Failure($"Not exact amount of {script.GetClass().Name} present at {SpaceFoundation.Current.GetAnchorName(anchorID)} ({anchorID}). Expected {tracker.count}, got {objectCount}.");
                            break;
                        }
                        case CountMode.MAXIMUM:
                        {
                            if (objectCount > tracker.count)
                                return QuestValidationInformation.Failure($"Too many {script.GetClass().Name} present at {SpaceFoundation.Current.GetAnchorName(anchorID)} ({anchorID}). Expected {tracker.count}, got {objectCount}.");
                            break;
                        }
                    }
                }
            }
            
            return QuestValidationInformation.Success();
        }

        public GenerationPattern GetCurrentPattern()
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