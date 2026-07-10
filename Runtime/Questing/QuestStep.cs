using System;
using giorgiokalmund.Dora.Questing.Events;
using JetBrains.Annotations;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

namespace giorgiokalmund.Dora.Questing
{
    [Serializable]
    public abstract class QuestStep : ScriptableObject 
    {
        [field: SerializeField, Tooltip("Whether to skip internal static validation.")]
        public bool SkipValidation { get; protected set; }
        
        [field: SerializeField, Tooltip("Whether this step has been completed")]
        [field: ReadOnly]
        public bool IsCompleted { get; protected set; }

        /// Whether it is possible for the requirements to be achieved.
        public bool CanBeAchieved => HandleValidation().IsSuccess;

        public bool CanBeCompleted => CheckCompletion();

        // TODO: Maybe make event such that += is enforced and no children call invoke it directly and are instead forced to call Complete();
        [NotNull] internal UnityEvent OnComplete = new ();
        [NotNull] internal UnityEvent OnUpdated = new ();
        
        /// <summary>
        /// Returns the static validation result of the quest step.
        /// </summary>
        /// <returns>
        /// Should return a new <see cref="QuestValidationInformation.Failure"/> if something has gone wrong,
        /// else a <see cref="QuestValidationInformation.Success"/>.
        /// </returns>
        [NotNull] protected abstract QuestValidationInformation HandleValidation();

        /// <summary>
        /// Validates the internals using <see cref="HandleValidation"/> if internal static validation is not skipped (<see cref="SkipValidation"/>).
        /// </summary>
        /// <returns></returns>
        [NotNull] internal QuestValidationInformation Validate()
        {
            if (!SkipValidation)
                return HandleValidation();
            return QuestValidationInformation.Success();
        }
        
        /// <summary>
        /// Returns whether the current state of the quest step allows completion of the step.
        /// </summary>
        /// <remarks>Override this method to create your custom, detailed checks.</remarks>
        protected virtual bool CheckCompletion() { return true; }

        /// <summary>
        /// If <see cref="CanBeCompleted"/> / <see cref="CheckCompletion"/>, will <see cref="Complete"/> the step,
        /// otherwise returns false.
        /// </summary>
        protected bool TryComplete()
        {
            bool result = CanBeCompleted;
            if (result)
                Complete();
            return result;
        }
        
        /// <summary>
        /// Completes the quest and invokes <see cref="OnComplete"/>.
        /// </summary>
        /// <remarks>Idempotent. Event is only fired exactly once (unless <see cref="ResetStep"/> of course).</remarks>
        private void Complete()
        {
            if (IsCompleted)
            {
                DoraLogger.LogWarning($"Cannot complete quest step {ToString()}. Already completed");
                return;
            }
            
            IsCompleted = true;
            OnComplete.Invoke();
        }

        protected void Update()
        {
            OnUpdated.Invoke();
        }

        public void ResetStep()
        {
            IsCompleted = false;
            OnReset();
        }

        // TODO: Maybe differentiate between editor description and gameplay description?
        public abstract string GetDescription();

        /// <summary>
        /// Attempts to process an incoming <see cref="IGameplayEvent"/> if it can be processed (<see cref="CanProcess"/>).
        /// </summary>
        /// <param name="e">The incoming <see cref="IGameplayEvent"/>.</param>
        internal void Process(IGameplayEvent e)
        {
            if (CanProcess(e))
                ProcessEvent(e);
        }

        /// <summary>
        /// Whether the type or contents of the incoming <see cref="IGameplayEvent"/> should be passed onto <see cref="ProcessEvent"/>.
        /// </summary>
        /// <param name="e">The incoming <see cref="IGameplayEvent"/>.</param>
        protected abstract bool CanProcess(IGameplayEvent e);
        
        /// <summary>
        /// Processes any <see cref="IGameplayEvent"/> which has passed the <see cref="CanProcess"/> check.
        /// </summary>
        /// <param name="e">The incoming <see cref="IGameplayEvent"/></param>.
        protected abstract void ProcessEvent(IGameplayEvent e);

        /// <summary>
        /// Resets the quest requirement to its starting state. All variables which track progress should be reset.
        /// </summary>
        public virtual void OnReset()
        {
            // Intentionally left blank
        }

        public virtual void OnQuestManagerInit()
        {
            // Intentionally left blank
        }

        public virtual void OnQuestManagerDeinit()
        {
            // Intentionally left blank
        }
    }
}