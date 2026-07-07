using System.Text;
using NaughtyAttributes;
using SpaceFoundationSystem;
using UnityEngine;

namespace giorgiokalmund.Dora.Requirements
{
    [CreateAssetMenu(fileName = "LocationPath", menuName = "Dora/Steps/LocationPath")]
    public class LocationPathStep : QuestStep, IDonator<string>
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
                    
                    if (hasMaxDistance)
                    {
                        accumulatedDistance += (anchorPosNextDiscretized - anchorPosDiscretized).magnitude;
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

        public bool Receive(string receivedAnchorID)
        {
            if (receivedAnchorID.Equals(CurrentPathID))
            {
                pathIndex++;
                if (pathIndex == path.Length) 
                    Complete();
                else
                    Update();
                return true;
            }
            
            return false;
        }

        public bool Steal(string donation)
        {
            return false;
        }

        public override void ResetRequirements()
        {
            pathIndex = 0;
            accumulatedDistance = 0;
        }
    }
}