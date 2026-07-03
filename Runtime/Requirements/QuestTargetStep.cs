using System;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;

namespace giorgiokalmund.Dora.Requirements
{
    public abstract class QuestTargetStep<T> : QuestStep, IDonator<T> where T : IComparable
    {
        [field: SerializeField, Tooltip("")]
        public T[] Targets { get; protected set; }
        
        [field: SerializeField, Tooltip("")]
        [field: ReadOnly]
        public List<T> Collected { get; protected set; }

        protected override QuestValidationInformation HandleValidation()
        {
            return new QuestValidationSuccess();
        }

        internal override string GetDescription()
        {
            return $"{Targets.Length} {typeof(T)}";
        }

        protected override bool CheckCompletion()
        {
            bool match = true;
            foreach (var monoBehaviour in Targets)
            {
                if (!Collected.Contains(monoBehaviour))
                    match = false;
            }
            return match;
        }

        public bool Receive(T receivedAnchorID)
        {
            if (!Targets.Contains(receivedAnchorID))
                return false;
            if (Collected.Contains(receivedAnchorID))
                return false;
            
            Collected.Add(receivedAnchorID);
            
            if (CanBeCompleted)
                Complete();
            
            return true;
        }

        public bool Steal(T donation)
        {
            return Collected.Remove(donation);
        }
    }
}