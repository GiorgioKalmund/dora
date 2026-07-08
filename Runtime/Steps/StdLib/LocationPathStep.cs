using System;
using System.Text;
using giorgiokalmund.Dora.Questing;
using giorgiokalmund.Dora.Questing.Events;
using NaughtyAttributes;
using SpaceFoundationSystem;
using UnityEngine;

namespace giorgiokalmund.Dora.Steps.StdLib
{
    [CreateAssetMenu(fileName = "LocationPath", menuName = "Dora/Steps/LocationPath")]
    public class LocationPathStep : QuestStep
    {
        [SerializeField] protected SpaceFoundationData spaceFoundationData;
        // TODO: Maybe only show options of valid strings here, similar to regular LocationStep / validate on the fly
        [SerializeField] private string[] path;
        [SerializeField] private bool hasMaxDistance;
        [ShowIf(nameof(hasMaxDistance))]
        [SerializeField] private float maxDistance;
        [ShowIf(nameof(hasMaxDistance))]
        [ShowNonSerializedField]
        private float accumulatedDistance;
        
        /// <summary>
        /// Used to create the path description (<see cref="PathDescription"/>). Cached to avoid recreating objects.
        /// </summary>
        StringBuilder _stringBuilder = new StringBuilder();
        
        [SerializeField] [field: ReadOnly] private int pathIndex;
        public int CurrentPathIdx => pathIndex;
        private string CurrentPathID => path[pathIndex];
        
        protected override QuestValidationInformation HandleValidation()
        {
            if (!spaceFoundationData)
                return QuestValidationInformation.Failure("No SpaceFoundation Data!");

            accumulatedDistance = 0;
            string overflowCandidate = null;
            for (var i = 0; i < path.Length; i++)
            {
                string anchorID = path[i];
                if (!spaceFoundationData.anchors.Contains(anchorID))
                    return QuestValidationInformation.Failure($"The anchorID '{anchorID}' is not part of the provided SpaceFoundationData {spaceFoundationData.name}");

                if (i != path.Length - 1)
                {
                    Vector3Int anchorPosDiscretized = spaceFoundationData.anchorToVoxelPositionDict.Get(anchorID);
                    Vector3Int anchorPosNextDiscretized = spaceFoundationData.anchorToVoxelPositionDict.Get(path[i+1]);
                    
                    accumulatedDistance += (anchorPosNextDiscretized - anchorPosDiscretized).magnitude;
                    if (hasMaxDistance)
                    {
                        if (accumulatedDistance > maxDistance && overflowCandidate == null)
                            overflowCandidate = anchorID;
                    }
                }
            }
            
            if (hasMaxDistance && overflowCandidate != null)
                return QuestValidationInformation.Failure($"The distance of the path ({accumulatedDistance}m) is larger than the maximum allowed distance ({maxDistance}m)\nThe first candidate to initialize the overflow was the path segment related to anchorID {overflowCandidate}.");
            
            return QuestValidationInformation.Success();
        }

        public override string GetDescription()
        {
            return CurrentPathIdx < path.Length 
                ? $"Visit {SpaceFoundation.Current.GetAnchorName(CurrentPathID)}" 
                : PathDescription;
        }

        private string PathDescription {
            get
            {
                _stringBuilder.Clear();
                for (var i = 0; i < path.Length; i++)
                {
                    _stringBuilder.Append($"[{path[i]}]");
                    if (i < path.Length - 1)
                        _stringBuilder.Append("-");
                }
                return _stringBuilder.ToString();
            }
        }

        protected override bool CanProcess(IGameplayEvent e) => e is EnteredLocationEvent;

        protected override void ProcessEvent(IGameplayEvent e)
        {
            EnteredLocationEvent entered = (EnteredLocationEvent)e;
            if (entered.Location.Equals(CurrentPathID))
            {
                pathIndex++;
                if (pathIndex == path.Length) 
                    Complete();
                else
                    Update();
            }
        }

        public override void OnReset()
        {
            pathIndex = 0;
            accumulatedDistance = 0;
        }
    }
}