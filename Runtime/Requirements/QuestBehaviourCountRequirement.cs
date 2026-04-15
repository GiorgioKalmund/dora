using UnityEngine;
using Object = UnityEngine.Object;

namespace giorgiokalmund.Dora.Requirements
{
    public enum CountMode
    {
        /// The requirement amount must be EXACTLY EQUAL to the amount of objects found.
        EXACT,
        /// The requirement amount must be AT LEAST EQUAL to the amount of objects found.
        MINIMUM
    }

    public class QuestBehaviourCountRequirement<T> : QuestRequirements where T : MonoBehaviour
    {
        [field: SerializeField, Tooltip("The currently collected amount of units.")]
        [field: ReadOnly]
        public int CollectedAmount { get; private set; }
        [field: SerializeField, Tooltip("The required amount of the type for the requirement to be true.")]
        public int RequiredAmount { get; private set; }
        [field: SerializeField, Tooltip("The mode in which the requirement has to be met.")]
        public CountMode Mode { get; private set; }

        public QuestBehaviourCountRequirement(int amount, CountMode mode = CountMode.MINIMUM)
        {
            CollectedAmount = 0;
            RequiredAmount = amount;
            Mode = mode;
        }

        public void Add()
        {
            if (CollectedAmount >= RequiredAmount)
                return;
            CollectedAmount++;
        }
        
        public void Remove()
        {
            if (CollectedAmount <= 0)
                return;
            CollectedAmount--;
        }

        protected QuestBehaviourCountRequirement() { }

        public override QuestValidationInformation Validate()
        {
            int objectCount = FindObjectsByType<T>().Length;
            if (objectCount < RequiredAmount)
                return QuestValidationInformation.Failure($"Not enough {typeof(T)} present. Expected {RequiredAmount}, got {objectCount}");
            
            if (Mode == CountMode.EXACT && objectCount > RequiredAmount)
                return QuestValidationInformation.Failure($"Not exact amount of {typeof(T)} present. Expected {RequiredAmount}, got {objectCount}");

            return QuestValidationInformation.Success();
        }

        public override string GetDescription()
        {
            return $"{RequiredAmount} of {typeof(T)}";
        }
    }
}