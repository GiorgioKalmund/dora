using System;
using giorgiokalmund.Dora.Questing.Events;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;

namespace giorgiokalmund.Dora.Questing
{
    [Serializable]
    public abstract class QuestStep : ScriptableObject 
    {
        [field: SerializeField, Tooltip("Whether to automatically validate the requirements in the editor.")]
        public bool SkipValidation { get; protected set; }
        
        [field: SerializeField, Tooltip("Whether this requirement should be hidden from the player / user. Some events might not fire if this is set to true.")]
        public bool IsCompleted { get; protected set; }

        /// Whether it is possible for the requirements to be achieved.
        public bool CanBeAchieved => HandleValidation().IsSuccess;

        public bool CanBeCompleted => CheckCompletion();

        // TODO: Maybe make event such that += is enforced and no children call invoke it directly and are instead forced to call Complete();
        [NotNull] internal UnityEvent OnComplete = new ();
        [NotNull] internal UnityEvent OnUpdated = new ();
        
        [NotNull]
        protected abstract QuestValidationInformation HandleValidation();

        [NotNull]
        internal QuestValidationInformation Validate()
        {
            if (!SkipValidation)
                return HandleValidation();
            return QuestValidationInformation.Success();
        }

        protected bool TryComplete()
        {
            bool result = CanBeCompleted;
            if (result)
                Complete();
            return result;
        }

        protected void Complete()
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
            SkipValidation = false;
            IsCompleted = false;
            OnReset();
        }

        // TODO: Maybe differentiate between editor description and gameplay description?
        public abstract string GetDescription();

        protected virtual bool CheckCompletion() { return true; }

        internal void Process(IGameplayEvent e)
        {
            if (CanProcess(e))
                ProcessEvent(e);
        }

        protected abstract bool CanProcess(IGameplayEvent e);
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