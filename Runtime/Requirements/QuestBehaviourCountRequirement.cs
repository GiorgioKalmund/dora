using System.Collections.Generic;
using giorgiokalmund.Dora.Generation;
using JetBrains.Annotations;
using NaughtyAttributes;
using UnityEngine;

namespace giorgiokalmund.Dora.Requirements
{
    public class QuestBehaviourCountRequirement<T> : QuestCountRequirement<T>, IPatternGenerator  where T : MonoBehaviour
    {
        [field: SerializeField, Tooltip("The currently collected units.")]
        [field: ReadOnly]
        public List<T> Collected { get; private set; }

        [SerializeField] [CanBeNull] protected GenerationPattern generationPattern;
        
        public QuestBehaviourCountRequirement(int amount, CountMode mode = CountMode.MINIMUM) : base(amount, mode) { Collected = new List<T>(); }

        public QuestBehaviourCountRequirement() { Collected = new List<T>(); }

        protected override QuestValidationInformation HandleValidation()
        {
            var baseResult = base.HandleValidation();
            if (baseResult.IsFailure)
                return baseResult;

            int objectCount = FindObjectsByType<T>().Length;
            if (objectCount < RequiredAmount)
                return QuestValidationInformation.Failure($"Not enough {typeof(T)} present. Expected {RequiredAmount}, got {objectCount}");
            
            if (Mode == CountMode.EXACT && objectCount > RequiredAmount)
                return QuestValidationInformation.Failure($"Not exact amount of {typeof(T)} present. Expected {RequiredAmount}, got {objectCount}");

            return QuestValidationInformation.Success();
        }

        protected override int GetCountOfCurrent()
        {
            return Collected?.Count ?? 0;
        }

        protected override bool HandleReceive(T element)
        {
            Collected.Add(element);
            return true;
        }

        protected override bool HandleSteal(T element)
        {
            return Collected.Remove(element);
        }

        public override void ResetRequirements()
        {
            base.ResetRequirements();
            Collected?.Clear();
        }

        GenerationPattern IPatternGenerator.GetCurrentPattern()
        {
            return generationPattern;
        }
    }
}