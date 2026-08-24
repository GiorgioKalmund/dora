using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using giorgiokalmund.Dora.Questing;
using giorgiokalmund.Dora.Questing.Events;
using giorgiokalmund.Dora.Saving;
using NaughtyAttributes;
using SpaceFoundationSystem.Util;
using UnityEngine;

namespace giorgiokalmund.Dora.Steps.StdLib
{
    [CreateAssetMenu(fileName = "StepPool", menuName = "Dora/Steps/Pool")]
    public class StepPool : QuestStep<StepPool.State>
    {
        #region Mode

        public enum Mode
        {
            /// <summary>
            /// Requires only exactly any <b>1</b> step to be completed to complete the pool.
            /// </summary>
            ANY, 
            /// <summary>
            /// Requires exactly <b><see cref="StepPool.specificStepCount"/></b> steps to be completed to complete the pool.
            /// </summary>
            SPECIFIC, 
            /// <summary>
            /// Requires all steps to be completed to complete the pool.
            /// </summary>
            ALL
        }
        #endregion

        #region PoolMemberSnapshot

        /// <summary>
        /// Intermediate representation to help identify a snapshot at application time.
        /// This is required as in a pool steps are completed in an unordered fashion, and thus the snapshots
        /// stored in the state do not necessarily line up with the steps in the pool.
        /// </summary>
        [Serializable]
        public struct PoolMemberSnapshot : ISerializableData<PoolMemberSnapshot>
        {
            public int index;
            public QuestStepSnapshot stepSnapshot;

            public void Dispose()
            {
                stepSnapshot.Dispose();
            }

            public bool Equals(PoolMemberSnapshot other)
            {
                return index == other.index && stepSnapshot.Equals(other.stepSnapshot);
            }

            public override bool Equals(object obj)
            {
                return obj is PoolMemberSnapshot other && Equals(other);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(index, stepSnapshot);
            }
        }
        

        #endregion
        
        /// <summary>
        /// The mode the pool is in.
        /// </summary>
        [Header("General")]
        [SerializeField, Tooltip("The mode the pool is in.")] private Mode mode;

        /// <summary>
        /// If set, will not pass on an incoming event to the following steps in the pool if it was successfully processed by a step.
        /// </summary>
        [SerializeField, Tooltip("If set, will not pass on an incoming event to the following steps in the pool if it was successfully processed by a step.")] private bool consumeEventsOnSuccess;
        
        [Header("'SPECIFIC' only")]
        [ShowIf(nameof(mode), Mode.SPECIFIC)]
        public int specificStepCount;

        // TODO: Documentation / tooltips
        [Serializable]
        public struct State : ISerializableData<State>
        {
            [SerializeField] internal PoolMemberSnapshot[] poolSnapshots;
            
            public void Dispose()
            {
                
            }

            public override bool Equals(object obj)
            {
                return obj is State other && Equals(other);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(poolSnapshots);
            }

            // This ensures idempotence when loading the same state multiple times / equality actually checks all children in pool as well.
            public bool Equals(State other)
            {
                if (ReferenceEquals(other.poolSnapshots, poolSnapshots))
                    return true;

                if (other.poolSnapshots == null || poolSnapshots == null)
                    return false;

                if (poolSnapshots.Length != other.poolSnapshots.Length)
                    return false;

                for (var i = 0; i < other.poolSnapshots.Length; i++)
                    if (!other.poolSnapshots[i].Equals(poolSnapshots[i]))
                        return false;

                return true;
            }

            public override string ToString()
            {
                var sb = new StringBuilder();
                sb.Append($"({poolSnapshots.Length}):\n");
                foreach (var questStepSnapshot in poolSnapshots)
                {
                    sb.Append("\t" + questStepSnapshot.stepSnapshot.serializedData + "\n");
                }

                return sb.ToString();
            }
        }
        
        [Expandable]
        [SerializeField]
        private AbstractQuestStep[] stepPool;
        
        protected override State GetInitialState()
        {
            return new State()
            {
                poolSnapshots = null
            };
        }

        public override bool ProgressHasBeenMade()
        {
            return base.ProgressHasBeenMade() || stepPool.Any(s => s.ProgressHasBeenMade());
        }

        public override ISerializableData GetSerializationState(ISerializationProvider serializer)
        {
            List<PoolMemberSnapshot> snapshots = new List<PoolMemberSnapshot>();
            for (var i = 0; i < stepPool.Length; i++)
            {
                if (stepPool[i].ProgressHasBeenMade())
                    snapshots.Add(new PoolMemberSnapshot()
                    {
                        index = i,
                        stepSnapshot = stepPool[i].CreateSnapshot(serializer)
                    });
                    
            }
            
            currentState.poolSnapshots = snapshots.ToArray();
            return currentState;
        }

        public override bool ModifyAppliedState(ISerializationProvider serializer, ref State state)
        {
            // Logical similar to Quest.cs. 
            // See explanation there.
            // 
            // However, here we need to additionally remember the index which the belongs to the state data.
            {
                Dictionary<int, QuestStepSnapshot> rollbackBuffer = new Dictionary<int, QuestStepSnapshot>();
                bool rollbackNeeded = false;
            
                for (var i = 0; i < stepPool.Length; i++)
                {
                    // TODO: @Performance there might be a better way to store and check this. Essentially O(N^2) :(
                    // If part of snapshot, apply new snapshot
                    var targetIndex = Array.FindIndex(state.poolSnapshots, s => s.index == i);
                    if (targetIndex != -1)
                    {
                        rollbackBuffer[i] = stepPool[i].CreateSnapshot(serializer);
                        if (!stepPool[i].ApplySnapshot(ref state.poolSnapshots[targetIndex].stepSnapshot, serializer))
                        {
                            rollbackNeeded = true;
                            break;
                        };
                    }
                    // else, it should be fully reset (as no state was saved for it). We ensure this by resetting it, as previous snapshots might have changed it.
                    else
                        stepPool[i].ResetStep();
                }
            
                if (rollbackNeeded)
                {
                    // For the steps which require a rollback, we roll them back. 
                    foreach (var (index, rollbackSnapshot) in rollbackBuffer)
                    {
                        var s = rollbackSnapshot;
                        stepPool[index].ApplySnapshot(ref s, serializer);
                    }
                
                    // Indicate failure
                    return false;
                }

                // No issues when applying --> no rollback, signal success
                return true;
            }
        }
        
        

        protected override ValidationResult HandleValidation(bool isRuntime)
        {
            if (mode == Mode.SPECIFIC && specificStepCount > stepPool.Length)
                return ValidationResult.Failure( $"<specificStepCount> is too large ({specificStepCount}). Maximum allowed value: {stepPool.Length}");
            
            foreach (var questStep in stepPool)
            {
                var res = questStep.Validate(isRuntime);
                if (res.IsFailure)
                    return res;
            }

            return ValidationResult.Success();
        }
        
        public override string GetDescription()
        {
            if (stepPool == null)
                return "<No sub-steps in this Pool!";
            
            StringBuilder sb = new StringBuilder($"{mode}");
            int completedCount = stepPool.Count(s => s.IsCompleted);
            if (mode == Mode.SPECIFIC)
                sb.Append($" {completedCount}/{specificStepCount}:\t");
            else sb.Append(":\t");
            
            for (var i = 0; i < stepPool.Length; i++)
            {
                if (stepPool[i].IsCompleted)
                    continue;
                if (stepPool.Length > 1)
                    sb.Append("(");
                sb.Append(stepPool[i].GetDescription());
                if (stepPool.Length > 1)
                    sb.Append(")");
                
                if (i < stepPool.Length - 1 && (stepPool.Length - completedCount > 1))
                {
                    if (mode == Mode.ALL)
                        sb.Append(" && ");
                    else 
                        sb.Append(" || ");
                }
            }
            return sb.ToString();
        }

        protected override bool CanProcess(IGameplayEvent _) => true;

        protected override bool ProcessEvent(IGameplayEvent e, ref State _)
        {
            bool success = false;
            foreach (var questStep in stepPool)
            {
                if (questStep.IsCompleted)
                    continue;
            
                if (questStep.Process(e))
                {
                    if (consumeEventsOnSuccess)
                        return true;
                    
                    // If not consume on first success,
                    // still indicate that some success has happened
                    success = true;
                }
            }

            return success;
        }

        public override void ResetState()
        {
            base.ResetState();
            
            if (stepPool == null)
                return;
            foreach (var questStep in stepPool)
                questStep.ResetStep();
        }

        protected override bool CheckCompletion()
        {
            if (stepPool == null || stepPool.IsEmpty())
                return false;
            
            int completedCount = stepPool.Count(s => s.IsCompleted);
            if (mode == Mode.ANY && completedCount >= 1)
                return true;
            if (mode == Mode.SPECIFIC && completedCount >= specificStepCount)
                return true;
            if (mode == Mode.ALL && completedCount == stepPool.Length)
                return true;
            
            return false;
        }


        public override void OnQuestManagerInit()
        {
            foreach (var questStep in stepPool)
            {
                questStep.OnComplete.AddListener(UpdateOrTryComplete);
                questStep.OnQuestManagerInit();
            }
        }

        public override void OnQuestManagerDeinit()
        {
            foreach (var questStep in stepPool)
            {
                questStep.OnQuestManagerDeinit();
                questStep.OnComplete.RemoveListener(UpdateOrTryComplete);
            }
        }

        private void UpdateOrTryComplete()
        {
            if (!TryComplete())
                Update();
        } 
    }
}