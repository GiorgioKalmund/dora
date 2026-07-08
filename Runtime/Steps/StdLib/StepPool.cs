using System;
using System.Linq;
using System.Text;
using giorgiokalmund.Dora.Questing;
using giorgiokalmund.Dora.Questing.Events;
using NaughtyAttributes;
using SpaceFoundationSystem.Util;
using UnityEngine;

namespace giorgiokalmund.Dora.Steps.StdLib
{
    [CreateAssetMenu(fileName = "StepPool", menuName = "Dora/Steps/Pool")]
    public class StepPool : QuestStep
    {
        enum Mode
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
        
        [SerializeField] private QuestStep[] stepPool;
        
        [SerializeField] private Mode mode;
        
        [ShowIf(nameof(mode), Mode.SPECIFIC)] [SerializeField]
        private int specificStepCount;
        
        protected override QuestValidationInformation HandleValidation()
        {
            if (mode == Mode.SPECIFIC && specificStepCount > stepPool.Length)
                return QuestValidationInformation.Failure( $"<specificStepCount> is too large ({specificStepCount}). Maximum allowed value: {stepPool.Length}");
            
            foreach (var questStep in stepPool)
            {
                var res = questStep.Validate();
                if (res.IsFailure)
                    return res;
            }

            return QuestValidationInformation.Success();
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
        protected override void ProcessEvent(IGameplayEvent e)
        {
            foreach (var questStep in stepPool)
            {
                questStep.Process(e);
            }
        }

        public override void OnReset()
        {
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
            Update();
            TryComplete();  
        } 
    }
}