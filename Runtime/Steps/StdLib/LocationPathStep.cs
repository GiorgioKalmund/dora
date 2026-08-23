using System;
using System.Text;
using giorgiokalmund.Dora.Questing;
using giorgiokalmund.Dora.Questing.Events;
using giorgiokalmund.Dora.Saving;
using NaughtyAttributes;
using SpaceFoundationSystem;
using UnityEngine;

namespace giorgiokalmund.Dora.Steps.StdLib
{
    [CreateAssetMenu(fileName = "LocationPath", menuName = "Dora/Steps/LocationPath")]
    public class LocationPathStep : QuestStep<LocationPathStep.State>
    {
        #region SerializedState
        [SerializeField] protected SpaceFoundationData spaceFoundationData;
        // TODO: Maybe only show options of valid strings here, similar to regular LocationStep / validate on the fly
        [SerializeField] private string[] path;
        [SerializeField] private bool hasMaxDistance;
        [ShowIf(nameof(hasMaxDistance))]
        [SerializeField] private float maxDistance;
        [ShowIf(nameof(hasMaxDistance))]
        [ShowNonSerializedField]
        private float _accumulatedDistance;
        
        /// <summary>
        /// Used to create the path description (<see cref="PathDescription"/>). Cached to avoid recreating objects.
        /// </summary>
        StringBuilder _stringBuilder = new ();
        #endregion

        #region State

        [Serializable]
        public struct State : ISerializableData<State>
        {
            public int pathIndex;

           public void Dispose() { }

            public bool Equals(State other)
            {
                return pathIndex == other.pathIndex;
            }
        }

        #endregion
        
        public int CurrentPathIdx => currentState.pathIndex;
        private string CurrentPathID => path[currentState.pathIndex];
        
        protected override ValidationResult HandleValidation(bool isRuntime)
        {
            if (!spaceFoundationData)
                return ValidationResult.Failure("No SpaceFoundation Data!");

            _accumulatedDistance = 0;
            string overflowCandidate = null;
            for (var i = 0; i < path.Length; i++)
            {
                string anchorID = path[i];
                if (!spaceFoundationData.ContainsAnchor(anchorID))
                    return ValidationResult.Failure($"The anchorID '{anchorID}' is not part of the provided SpaceFoundationData {spaceFoundationData.name}");

                if (i != path.Length - 1)
                {
                    Vector3Int anchorPosDiscretized = spaceFoundationData.GetAnchorVoxelPosition(anchorID);
                    Vector3Int anchorPosNextDiscretized = spaceFoundationData.GetAnchorVoxelPosition(path[i+1]);
                    
                    _accumulatedDistance += (anchorPosNextDiscretized - anchorPosDiscretized).magnitude;
                    if (hasMaxDistance)
                    {
                        if (_accumulatedDistance > maxDistance && overflowCandidate == null)
                            overflowCandidate = anchorID;
                    }
                }
            }
            
            if (hasMaxDistance && overflowCandidate != null)
                return ValidationResult.Failure($"The distance of the path ({_accumulatedDistance}m) is larger than the maximum allowed distance ({maxDistance}m)\nThe first candidate to initialize the overflow was the path segment related to anchorID {overflowCandidate}.");
            
            return ValidationResult.Success();
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
                    _stringBuilder.Append($"[{spaceFoundationData.GetAnchorName(path[i])}]");
                    if (i < path.Length - 1)
                        _stringBuilder.Append("-");
                }
                return _stringBuilder.ToString();
            }
        }

        protected override bool CanProcess(IGameplayEvent e) => e is EnteredLocationEvent;

        protected override bool CheckCompletion()
        {
            return currentState.pathIndex >= path.Length;
        }

        protected override bool ProcessEvent(IGameplayEvent e, ref State state)
        {
            EnteredLocationEvent entered = (EnteredLocationEvent)e;
            if (entered.Location.Equals(CurrentPathID))
            {
                state.pathIndex++;
                if (!TryComplete())
                {
                    Update();
                    return false;
                }
                
                return true;
            }

            return false;
        }

        public override void ResetState()
        {
            base.ResetState();
            _accumulatedDistance = 0;
        }

        protected override State GetInitialState()
        {
            return new State()
            {
                pathIndex = 0
            };
        }
    }
}