using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;

namespace giorgiokalmund.Dora
{
    [Serializable]
    public class QuestStep 
    {
        [field: ReadOnly]
        [field: SerializeField, Tooltip("Whether this step has been completed.")]
        public bool IsCompleted { get; protected set; }
        
        [field: SerializeField, Tooltip("Requirements to which need to be in place for the quest to be viable.")]
        public QuestRequirements Requirements { get; protected set; }

        internal UnityEvent OnComplete = new UnityEvent();

        private void OnValidate()
        {
            // TODO:
            /*
            if (Requirements == null)
                IsCompleted = false;
            else if (!IsCompleted && Requirements.OnComplete.GetPersistentEventCount() == 0)
                Requirements?.OnComplete.AddListener(HandleRequirementsCompleted); 
                */
        }

        private void HandleRequirementsCompleted()
        {
            Debug.Log("requirements completed!");
            IsCompleted = true;
            OnComplete.Invoke();
        }

        public bool Donate(object donation)
        {
            if (Requirements is IDonator donator)
                if (donator.CanDonate(donation))
                    return donator.Donate(donation);

            return false;
        }
        
        #if DEBUG
        public void DebugSetCompleted(bool c)
        {
            IsCompleted = c;
        }
        #endif
        
    }
}