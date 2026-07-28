using giorgiokalmund.Dora.Editor;
using giorgiokalmund.Dora.Questing;
using giorgiokalmund.Dora.Questing.Events;
using NaughtyAttributes;
using NUnit.Framework;
using SpaceFoundationSystem;
using UnityEngine;

namespace giorgiokalmund.Dora.Steps.StdLib
{
    [CreateAssetMenu(fileName = "Location", menuName = "Dora/Steps/Location")]
    public class LocationStep : QuestStep
    {
        [SerializeField] protected SpaceFoundationData spaceFoundationData;
        private bool HasSfsData => spaceFoundationData != null;
        
        [SerializeField, Location(nameof(spaceFoundationData)), ShowIf(nameof(HasSfsData))]
        protected string anchor;

        protected override QuestValidationInformation HandleValidation()
        {
            if (!spaceFoundationData)
                return QuestValidationInformation.Failure("No SpaceFoundation Data!");
            if (!spaceFoundationData.anchors.Contains(anchor))
                return QuestValidationInformation.Failure($"The anchorID '{anchor}' is not part of the provided SpaceFoundationData {spaceFoundationData.name}");
            return QuestValidationInformation.Success();
        }

        public override string GetDescription()
        {
            return $"Visit {SpaceFoundation.Current.GetAnchorName(anchor)}";
        }

        protected override bool CanProcess(IGameplayEvent e) => e is EnteredLocationEvent;

        protected override void ProcessEvent(IGameplayEvent e)
        {
            Assert.IsTrue(e is EnteredLocationEvent, $"LocationStep is processing invalid event type: {e.GetType()}");
            EnteredLocationEvent entered = (EnteredLocationEvent)e;
            if (entered.Location.Equals(anchor))
            {
                TryComplete();
            }
        }
    }
}