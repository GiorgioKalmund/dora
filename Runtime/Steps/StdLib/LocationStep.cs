using System;
using System.Linq;
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
        private bool HasData => spaceFoundationData != null;
        
        [Dropdown(nameof(AvailableAnchors))]
        [ShowIf(nameof(HasData))]
        [SerializeField] protected string anchorID;
        internal string[] AvailableAnchors => spaceFoundationData?.anchors.entries.Select(e => e.Key).ToArray() ?? new string[]{};

        protected override QuestValidationInformation HandleValidation()
        {
            if (!spaceFoundationData)
                return QuestValidationInformation.Failure("No SpaceFoundation Data!");
            if (!spaceFoundationData.anchors.Contains(anchorID))
                return QuestValidationInformation.Failure($"The anchorID '{anchorID}' is not part of the provided SpaceFoundationData {spaceFoundationData.name}");
            return QuestValidationInformation.Success();
        }

        public override string GetDescription()
        {
            return $"Visit {SpaceFoundation.Current.GetAnchorName(anchorID)}";
        }

        protected override bool CanProcess(IGameplayEvent e) => e is EnteredLocationEvent;

        protected override void ProcessEvent(IGameplayEvent e)
        {
            Assert.IsTrue(e is EnteredLocationEvent, $"LocationStep is processing invalid event type: {e.GetType()}");
            EnteredLocationEvent entered = (EnteredLocationEvent)e;
            if (entered.Location.Equals(anchorID))
            {
                Complete();
            }
        }
    }
}