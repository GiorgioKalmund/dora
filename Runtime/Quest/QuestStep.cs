using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;

namespace giorgiokalmund.Dora
{
    [Serializable]
    public abstract class QuestStep : ScriptableObject
    {
        [field: SerializeField, Tooltip("Whether to automatically validate the requirements in the editor.")]
        public bool SkipValidation { get; protected set; }
        
        [field: SerializeField, Tooltip("Whether this requirement should be hidden from the player / user. Some events might not fire if this is set to true.")]
        public bool IsHidden { get; protected set; }
        
        [field: SerializeField, Tooltip("Whether this requirement should be hidden from the player / user. Some events might not fire if this is set to true.")]
        public bool IsCompleted { get; protected set; }

        /// Whether it is possible for the requirements to be achieved.
        public bool CanBeAchieved => HandleValidation().IsSuccess;

        public bool CanBeCompleted => CheckCompletion();

        // TODO: Maybe make event such that += is enforced and no children call invoke it directly and are instead forced to call Complete();
        [NotNull] internal UnityEvent OnComplete = new ();
        
        [NotNull]
        protected abstract QuestValidationInformation HandleValidation();

        [NotNull]
        internal QuestValidationInformation Validate()
        {
            if (!SkipValidation)
                return HandleValidation();
            return QuestValidationInformation.Success();
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

        public void Reset()
        {
            SkipValidation = false;
            IsHidden = false;
            IsCompleted = false;
            ResetRequirements();
        }

        public abstract string GetDescription();

        protected virtual bool CheckCompletion() { return true; }

        /// <summary>
        /// Resets the quest requirement to its starting state. All variables which track progress should be reset.
        /// </summary>
        public virtual void ResetRequirements()
        {
            
        }
    }
}