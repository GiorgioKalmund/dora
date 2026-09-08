using giorgiokalmund.Dora.Editor;
using giorgiokalmund.Dora.Questing;
using giorgiokalmund.Dora.Questing.Events;
using NaughtyAttributes;
using SpaceFoundationSystem;
using UnityEngine;
using UnityEngine.Assertions;

namespace giorgiokalmund.Dora.Steps.StdLib
{
    [CreateAssetMenu(fileName = "Location", menuName = "Dora/Steps/Location")]
    public class LocationStep : QuestStep
    {
        [SerializeField] protected SpaceFoundationData spaceFoundationData;
        private bool HasSfsData => spaceFoundationData != null;
        
        [SerializeField, Location(nameof(spaceFoundationData)), ShowIf(nameof(HasSfsData))]
        protected string anchor;

        protected override ValidationResult HandleValidation(bool isRuntime)
        {
            if (!spaceFoundationData)
                return ValidationResult.Failure("No SpaceFoundation Data!");
            if (!spaceFoundationData.ContainsAnchor(anchor))
                return ValidationResult.Failure($"The anchorID '{anchor}' is not part of the provided SpaceFoundationData {spaceFoundationData.name}");
            return ValidationResult.Success();
        }

        public override string GetDescription()
        {
            return $"Visit {spaceFoundationData?.GetAnchorName(anchor) ?? "<NO SFS DATA>"}";
        }

        protected override bool CanProcess(IGameplayEvent e) => e is EnteredLocationEvent;

        protected override bool ProcessEvent(IGameplayEvent e, ref State _)
        {
            Assert.IsTrue(e is EnteredLocationEvent, $"LocationStep is processing invalid event type: {e.GetType()}");
            EnteredLocationEvent entered = (EnteredLocationEvent)e;
            if (entered.Location.Equals(anchor))
            {
                return TryComplete();
            }

            return false;
        }
    }
}