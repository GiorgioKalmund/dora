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
    // TODO BIG @Fix: Rollback etc currently not properly get serialized and applied due to abstraction
    [CreateAssetMenu(fileName = "StepPool", menuName = "Dora/Steps/Pool")]
    public class StepPool : QuestStep<StepPool.State>
    {
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
            [SerializeField] internal QuestStepSnapshot[] poolSnapshots;
            
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
                    {
                        Debug.Log($"inequality comes from {other.poolSnapshots[i].serializedData} {other.poolSnapshots[i].isCompleted} vs {poolSnapshots[i].serializedData} {poolSnapshots[i].isCompleted}");
                        return false;
                    }

                return true;
            }

            public override string ToString()
            {
                var sb = new StringBuilder();
                sb.Append($"({poolSnapshots.Length}):\n");
                foreach (var questStepSnapshot in poolSnapshots)
                {
                    sb.Append("\t" + questStepSnapshot.serializedData + "\n");
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
            /* TODO: Compression is possible here as well. We just need to additionally store the index of the corresponding snapshots
             as pools are completed in an unordered fashion. ModifyAppliedState also needs to take the ordering into account then. */ 
            currentState.poolSnapshots = stepPool.Select(s => s.CreateSnapshot(serializer)).ToArray();
            return currentState;
        }

        public override bool ModifyAppliedState(ISerializationProvider serializer, ref State state)
        {
            if (state.poolSnapshots.Length != stepPool.Length)
            {
                DoraLogger.LogError($"Invalid lengths. The pool snapshot being applied does not contain the same amount of elements as the current pool!\nSnapshot: {state.poolSnapshots.Length}\tPool: {stepPool.Length}\n");
            }

            // Logical equivalent & copy-paste from Quest.cs
            // See explanation there.
            {
                // TODO: If compressing, the rollback buffer will be dynamically sized instead, as not all pools are stored in the data 
                QuestStepSnapshot[] rollbackBuffer = new QuestStepSnapshot[stepPool.Length];
                int rollbackIndex = -1;
            
                for (var i = 0; i < state.poolSnapshots.Length; i++)
                {
                    rollbackBuffer[i] = stepPool[i].CreateSnapshot(serializer);

                    if (!stepPool[i].ApplySnapshot(ref state.poolSnapshots[i], serializer))
                    {
                        rollbackIndex = i;
                        break;
                    };
                }
            
                if (rollbackIndex != -1)
                {
                    // For the steps which require a rollback, we roll them back. 
                    for (var i = 0; i < rollbackBuffer.Length; i++)
                    {
                        stepPool[i].ApplySnapshot(ref rollbackBuffer[i], serializer);
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