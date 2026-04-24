using System;
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

        public bool TryDonate(object donation)
        {
            if (Requirements is IDonator donator)
                if (donator.CanDonate(donation))
                    return donator.Donate(donation);
                else
                    QuestLogger.LogWarning($"QuestStep {Requirements.name} currently does not take any donations.");
            else 
                QuestLogger.LogWarning($"QuestStep {Requirements.name} cannot be donated to.");

            return false;
        }
        
        public bool TryDonateQuick()
        {
            if (Requirements is IQuickDonator donator)
                return donator.QuickDonate();
            
            QuestLogger.LogWarning($"QuestStep {Requirements.name} cannot be donated to.");
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