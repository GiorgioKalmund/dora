using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using NUnit.Framework;
using UnityEngine;

namespace giorgiokalmund.Dora
{
    [CreateAssetMenu(fileName = "Quest", menuName = "Dora/Quest", order = 1)]
    public class Quest :  ScriptableObject
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

        [CanBeNull]
        public QuestStep CurrentStep
        {
            get
            {
                if (_stepIndex < 0 || _stepIndex >= Steps.Length)
                    return null;
                return Steps[_stepIndex];
            }
        }

        private void Start()
        {
            ValidateInternals();
            QuestValidator.Validate(this);
        }

        public QuestValidationInformation[] ValidateAllQuestSteps()
        {
            List<QuestValidationInformation> failures = new List<QuestValidationInformation>();
            foreach (var questStep in Steps)
            {
                var result = questStep.Requirements.Validate();
                if (result.IsFailure)
                    failures.Add(result);
            }

            return failures.ToArray();
        }
        
        [NotNull]
        public QuestValidationInformation ValidateQuestSteps()
        {
            foreach (var questStep in Steps)
            {
                var result = questStep.Requirements.Validate();
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
            if (Information == null)
                errorMessages.Add($"[{GetType()}]: Information cannot be null!");
            if (Steps == null)
                errorMessages.Add($"[{GetType()}]: Steps cannot be null!");
            else if (Steps.FirstOrDefault(s => s != null) == null)
                errorMessages.Add($"[{GetType()}]: Steps cannot be empty!");

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

            _stepIndex++;
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

        public bool Botch()
        {
            if (IsBotchedOrCompleted)
                return false;

            IsBotched = true;
            return true;
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


        #region Helpers
        public bool Complete() => TryAdvanceState(QuestState.COMPLETED);

        public string TryGetQuestTitle()
        {
            if (Information == null || string.IsNullOrEmpty(Information.Title))
                return "<NO-TITLE>";
            return Information.Title;
        }
        
        public string TryGetQuestId()
        {
            if (Information == null || string.IsNullOrEmpty(Information.Id))
                return "<NO-ID>";
            return Information.Id;
        }

        #endregion
    }
}