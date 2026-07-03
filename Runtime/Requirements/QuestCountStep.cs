using UnityEngine;

namespace giorgiokalmund.Dora.Requirements
{
    [System.Serializable] 
    public enum CountMode
    {
        /// The requirement amount must be EXACTLY EQUAL to the amount of objects found.
        EXACT,
        /// The requirement amount must be AT LEAST EQUAL to the amount of objects found.
        MINIMUM
    }

    public abstract class QuestCountStep<T> : QuestStep, IDonator<T>
    {
        [field: SerializeField, Tooltip("The required amount of the type for the requirement to be true.")]
        public int RequiredAmount { get; private set; }
        [field: SerializeField, Tooltip("The mode in which the requirement has to be met.")]
        public CountMode Mode { get; private set; }

        /// The currently collected amount of units.
        public int CollectedAmount => GetCountOfCurrent();
        
        public const int MaxElementCount = int.MaxValue; // TODO: Friendlier API / Interface
        
        public QuestCountStep(int amount, CountMode mode = CountMode.MINIMUM)
        {
            RequiredAmount = Mathf.Min(amount, MaxElementCount);
            Mode = mode;
        }

        protected QuestCountStep() { }

        protected override QuestValidationInformation HandleValidation()
        {
            return QuestValidationInformation.Success();
        }

        protected abstract int GetCountOfCurrent();

        public override string GetDescription()
        {
            return $"{RequiredAmount} of {typeof(T)}";
        }

        public bool Receive(T receivedAnchorID)
        {
            if (CollectedAmount >= RequiredAmount || CollectedAmount > MaxElementCount)
                return false;
            
            var success = HandleReceive(receivedAnchorID);
            if (success && CanBeCompleted)
                Complete();
            return success;
        }

        protected override bool CheckCompletion()
        {
            return Mode == CountMode.EXACT ? CollectedAmount == RequiredAmount : CollectedAmount >= RequiredAmount;
        }

        protected abstract bool HandleReceive(T element);

        public bool Steal(T donation)
        {
            return HandleSteal(donation);
        }
        
        protected abstract bool HandleSteal(T element);
    }
}