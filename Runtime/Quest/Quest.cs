using System;
using System.Collections.Generic;
using System.Linq;
using giorgiokalmund.Dora.Utils;
using JetBrains.Annotations;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

namespace giorgiokalmund.Dora
{
    [CreateAssetMenu(fileName = "Quest", menuName = "Dora/Quest", order = 1)]
    public class Quest :  BaseComponent<QuestManager>, IEquatable<Quest>, IComparable<Quest>
    {
        [field: ReadOnly]
        [field: SerializeField, Tooltip("Cannot be recovered or completed. Can be set during every state except if already <see cref=\"QuestState.COMPLETED\"/>.")]
        public bool IsBotched { get; protected set; }
        
        [field: SerializeField, Tooltip("Whether this quest is invisible to the player. Certain events might not fire if set to true.")]
        public bool IsHidden { get; protected set; }

        [field: SerializeField, Tooltip("The state of the quest. Can only move forward. (Unless restarted / reset)")]
        [field: ReadOnly]
        public QuestState State { get; private set; }

        public bool IsCompleted => State == QuestState.COMPLETED;

        /// Whether the quest is botched or completed. This indicated that no operations which affect the quest are possible anymore.
        public bool IsBotchedOrCompleted => IsBotched || IsCompleted;

        [field: SerializeField, Tooltip("Information about the quest in general.")]
        public QuestInformation Information { get; protected set; }

        [field: SerializeField, Tooltip("Optional initial Requirements for the quest to be met.")]
        [CanBeNull]
        public QuestStep BaseStep { get; protected set; }
        
        [field: SerializeField, Tooltip("Optional rewards when the quest is completed.")]
        [CanBeNull]
        public QuestRewards Rewards { get; protected set; }

        [field: SerializeField, Tooltip("Individual steps of the quest."), Expandable]
        public QuestStep[] Steps { get; protected set; }
        [field: SerializeField][field:ReadOnly] private int currentStepIdx;
        public int CurrentCurrentStepIdx => currentStepIdx;

        internal IEnumerable<QuestStep> AllRequirementsToValidate => Steps.Where(r =>  !r?.SkipValidation ?? false);

        public UnityEvent<QuestState> onStateChanged = new UnityEvent<QuestState>();
        public UnityEvent<QuestStep> onStepStarted = new UnityEvent<QuestStep>();
        public UnityEvent<QuestStep> onStepCompleted = new UnityEvent<QuestStep>();
        
        [CanBeNull]
        public QuestStep CurrentStep
        {
            get
            {
                if (State != QuestState.ACCEPTED || currentStepIdx < 0 || currentStepIdx >= Steps.Length)
                    return null;
                return Steps[currentStepIdx];
            }
        }

        private void Awake()
        {
            Information.Title = name;
        }
        
        public void Reset()
        {
            State = QuestState.UNKNOWN;
            currentStepIdx = 0;
            IsBotched = false;
            IsHidden = false;
        } 

        #region Validation

        public QuestValidationInformation[] ValidateAllQuestSteps()
        {
            if (AllRequirementsToValidate == null)
                return Array.Empty<QuestValidationInformation>();
            
            List<QuestValidationInformation> failures = new List<QuestValidationInformation>();
            foreach (var requirement in AllRequirementsToValidate)
            {
                var result = requirement.Validate();
                if (result.IsFailure)
                    failures.Add(result);
            }

            return failures.ToArray();
        }
        
        [NotNull]
        public QuestValidationInformation ValidateQuestSteps()
        {
            if (AllRequirementsToValidate == null)
                return QuestValidationInformation.Failure("There are no steps to validate!");
            
            foreach (var requirement in AllRequirementsToValidate)
            {
                var result = requirement.Validate();
                if (result.IsFailure)
                    return result;
            }

            return QuestValidationInformation.Success();
        }

        [NotNull]
        public QuestValidationInformation Validate()
        {
            if (BaseStep)
            {
                var result = BaseStep.Validate();
                if (result.IsFailure)
                    return result;
            }

            return ValidateQuestSteps();
        }

        internal string[] GetInternalValidationResult()
        {
            List<string> errorMessages = new List<string>();
            if (String.IsNullOrEmpty(Information.identifier))
                errorMessages.Add("Identifier cannot be empty or null.");
            if (Steps == null)
                errorMessages.Add("Steps cannot be null.");
            else if (Steps.FirstOrDefault(s => s != null) == null)
                errorMessages.Add("Steps cannot be empty.");

            return errorMessages.ToArray();
        }

        #endregion

        internal void InitForScene()
        {
            
        }
        
        [CanBeNull]
        protected QuestStep NextStep()
        {
            if (IsBotchedOrCompleted)
            {
                DoraLogger.LogWarning("Cannot move onto next step. Quest botched or already completed.");
                return null;
            }

            if (State != QuestState.ACCEPTED)
            {
                DoraLogger.LogWarning("Cannot move onto next step. Quest not accepted yet.");
                return null;
            }
            
            if (Steps == null)
            {
                DoraLogger.LogWarning($"[{GetType()}]: Cannot advance to next step. There are no steps provided.");
                return null;
            };

            if (currentStepIdx >= Steps.Length)
            {
                DoraLogger.LogWarning("Cannot move onto next step. No more steps left.");
                return null;
            }

            if (CurrentStep != null)
                onStepCompleted.Invoke(CurrentStep);
            CurrentStep?.OnComplete.RemoveListener(HandleStepCompleted);
            currentStepIdx++;
            CurrentStep?.OnComplete.AddListener(HandleStepCompleted);
            if (CurrentStep != null)
                onStepStarted.Invoke(CurrentStep);
            return CurrentStep;
        }

        /// <summary>
        /// Advances the state based on the restricted flow of the state logic.
        /// </summary>
        /// <param name="newState"></param>
        /// <returns></returns>
        internal bool TryAdvanceState(out QuestState newState)
        {
            newState = State;

            if (State < QuestState.ACCEPTED && (BaseStep && BaseStep.Validate().IsFailure))
            {
                DoraLogger.LogWarning("cannot advance state. not accepted or base not met");
                return false;
            }

            var next = State.GetNext();
            if (next.HasValue)
            {
                newState = next.Value;
                return TrySetState(next.Value);
            }

            DoraLogger.LogWarning("no next state :(");
            return false;
        }

        internal bool TrySetState(QuestState newState)
        {
            if (IsBotched)
            {
                DoraLogger.LogWarning($"Cannot set {Information} to state '{newState}' as it is botched.");
                return false;
            }
            
            if (IsCompleted)
            {
                DoraLogger.LogWarning($"Cannot set {Information} to state '{newState}' as it already completed.");
                return false;
            }
            
            if (newState.CompareTo(State) <= 0)
            {
                DoraLogger.LogError($"Cannot set {Information} to state '{newState}' as it is already in state '{State}'");
                return false;
            }

            if (newState == QuestState.ACHIEVED && !CanBeAchieved())
            {
                DoraLogger.LogWarning($"Cannot set {Information} to state '{newState}' as it cannot be achieved right now.");
                return false;
            }

            State = newState;
            onStateChanged.Invoke(State);
            Manager?.onQuestStateChanged.Invoke(this, State);
            
            if (State == QuestState.ACCEPTED)
            {
                CurrentStep?.OnComplete.AddListener(HandleStepCompleted);
            }

            if (State == QuestState.ACHIEVED && Rewards == null)
                return TryAdvanceState(out _);
            
            if (State == QuestState.COMPLETED)
                HandOutRewards();
            return true;
        }

        public bool Botch()
        {
            if (IsBotchedOrCompleted)
                return false;

            IsBotched = true;
            return true;
        }

        private void HandleStepCompleted()
        {
            if (NextStep() == null)
                TryAdvanceState(out _);
        }

        internal void HandOutRewards()
        {
            Rewards?.HandOut();
        }

        public bool CanBeAchieved()
        {
            if (IsBotchedOrCompleted)
                return false;

            if (Steps == null)
                return false;

            // Check for equality here as in the final step completion we still call NextStep, which advances the index one last time
            if (currentStepIdx == Steps.Length && (CurrentStep?.IsCompleted ?? true))
                return true;

            return false;
        }

        #region IEquatable - based on QuestInformation

        public bool Equals(Quest other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return base.Equals(other) && Equals(Information, other.Information);
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Quest)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), Information);
        }

        #endregion

        #region IComparable - based on QuestInformation

        public int CompareTo(Quest other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (other is null) return 1;
            return Comparer<QuestInformation>.Default.Compare(Information, other.Information);
        }

        #endregion
    }
}