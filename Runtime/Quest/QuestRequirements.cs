using System;
using JetBrains.Annotations;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

namespace giorgiokalmund.Dora
{
    [Serializable]
    public abstract class QuestRequirements : ScriptableObject
    {
        [field: SerializeField, Tooltip("Whether to automatically validate the requirements in the editor.")]
        public bool SkipValidation { get; protected set; }
        
        [field: SerializeField, Tooltip("Whether this requirement should be hidden from the player / user. Some events might not fire if this is set to true.")]
        [field: ReadOnly]
        public bool IsHidden { get; protected set; }

        /// Whether it is possible for the requirements to be achieved.
        public bool CanBeAchieved => HandleValidation().IsSuccess;

        public bool CanBeCompleted => CheckCompletion();

        [NotNull] internal UnityEvent OnComplete = new ();
        [NotNull] internal UnityEvent<QuestState> OnStateChanged = new ();
        
        [NotNull]
        protected abstract QuestValidationInformation HandleValidation();

        [NotNull]
        internal QuestValidationInformation Validate()
        {
            if (!SkipValidation)
                return HandleValidation();
            return QuestValidationInformation.Success();
        }

        internal abstract string GetDescription();

        protected virtual bool CheckCompletion() { return true; }

        /// <summary>
        /// Resets the quest requirement to its starting state. All variables which track progress should be reset.
        /// </summary>
        public virtual void ResetRequirements()
        {
            
        }
    }
}