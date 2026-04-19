using System;
using System.Collections.Generic;
using System.Linq;
using giorgiokalmund.Dora.Utils;
using JetBrains.Annotations;
using NUnit.Framework;
using UnityEngine;

namespace giorgiokalmund.Dora
{
    [CreateAssetMenu(fileName = "Quest", menuName = "Dora/Quest", order = 1)]
    public class Quest :  BaseComponent<QuestManager>
    {
        [field: ReadOnly]
        [field: SerializeField, Tooltip("Cannot be recovered or completed. Can be set during every state except if already <see cref=\"QuestState.COMPLETED\"/>.")]
        public bool IsBotched { get; protected set; }

        [field: ReadOnly]
        [field: SerializeField, Tooltip("The state of the quest. Can only move forward. (Unless restarted / reset)")]
        public QuestState State { get; internal set; }

        /// Whether the quest is botched or completed. This indicated that no operations which affect the quest are possible anymore.
        public bool IsBotchedOrCompleted => IsBotched || State == QuestState.COMPLETED;

        [field: SerializeField, Tooltip("Information about the quest in general.")]
        public QuestInformation Information { get; protected set; }

        [field: SerializeField, Tooltip("Optional initial Requirements for the quest to be met.")]
        [CanBeNull]
        public QuestRequirements BaseRequirements { get; protected set; }
        
        [field: SerializeField, Tooltip("Optional rewards when the quest is completed.")]
        [CanBeNull]
        public QuestRewards Rewards { get; protected set; }

        [field: SerializeField, Tooltip("Individual steps of the quest.")]
        public QuestStep[] Steps { get; protected set; }
        private int _stepIndex;

        internal IEnumerable<QuestRequirements> AllRequirements => Steps?.Select(s => s.Requirements);
        internal IEnumerable<QuestRequirements> AllRequirementsToValidate => AllRequirements?.Where(r => !r.SkipValidation);

        [CanBeNull]
        public QuestStep CurrentStep
        {
            get
            {
                if (State != QuestState.ACCEPTED || _stepIndex < 0 || _stepIndex >= Steps.Length)
                    return null;
                return Steps[_stepIndex];
            }
        }

        private void OnEnable()
        {
            ValidateInternals();
            QuestValidator.Validate(this);
        }

        private void Awake()
        {
            Information.Title = name;
        }

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
            if (BaseRequirements)
            {
                var result = BaseRequirements.Validate();
                if (result.IsFailure)
                    return result;
            }

            return ValidateQuestSteps();
        }

        internal string[] GetInternalValidationResult()
        {
            List<string> errorMessages = new List<string>();
            if (Steps == null)
                errorMessages.Add($"[{GetType()}]: Steps cannot be null!");
            else if (Steps.FirstOrDefault(s => s != null) == null)
                errorMessages.Add($"[{GetType()} - {Information.Title}]: Steps cannot be empty!");

            return errorMessages.ToArray();
        }

        internal void ValidateInternals()
        {
            string[] messages = GetInternalValidationResult();
            foreach (var message in messages)
                QuestLogger.LogError(message);
        }

        [CanBeNull]
        protected QuestStep NextStep()
        {
            if (IsBotchedOrCompleted)
                return null;

            if (State != QuestState.ACCEPTED)
                return null;

            Assert.IsTrue(Steps?.Length > 0, $"[{GetType()}]: Cannot advance to next step. There are no steps provided.");

            if (_stepIndex >= Steps.Length)
                return null;

            CurrentStep?.OnComplete.RemoveListener(HandleStepCompleted);
            _stepIndex++;
            CurrentStep?.OnComplete.AddListener(HandleStepCompleted);
            return CurrentStep;
        }

        public bool TryAdvanceState()
        {
            if (IsBotchedOrCompleted)
                return false;

            if (BaseRequirements && State < QuestState.ACCEPTED && BaseRequirements.Validate().IsFailure)
                return false;

            var next = State.GetNext();
            if (next.HasValue)
            {
                if (next.Value == QuestState.ACHIEVED && !CanBeAchieved())
                    return false;

                State = next.Value;
                if (State == QuestState.COMPLETED)
                    HandOutRewards();    
                
                Manager?.onQuestStateChanged.Invoke(this, State);
                return true;
            }

            return false;
        }
        
        public bool TryAdvanceState(QuestState state)
        {
            if (IsBotchedOrCompleted)
                return false;

            if (state <= State)
                return false;

            while (!State.Equals(state))
            {
                if (!TryAdvanceState())
                    return false;
            }
            
            return true;
        }

        public bool TryDonate(object value)
        {
            return CurrentStep?.Donate(value) ?? false;
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
            TryAdvanceState();
        }

        internal void HandOutRewards()
        {
            Rewards?.HandOut();
        }

        public bool CanBeAchieved()
        {
            if (IsBotchedOrCompleted)
                return false;

            if (CurrentStep == null)
                return false;

            if (_stepIndex == Steps.Length - 1 && CurrentStep.IsCompleted)
                return true;

            return false;
        }

        public bool Complete() => TryAdvanceState(QuestState.COMPLETED);
    }
}