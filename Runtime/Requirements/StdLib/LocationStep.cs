using System.Linq;
using NaughtyAttributes;
using SpaceFoundationSystem;
using UnityEngine;

namespace giorgiokalmund.Dora.Requirements
{
    [CreateAssetMenu(fileName = "Location", menuName = "Dora/Requirements/Location")]
    public class LocationStep : QuestStep, IDonator<string>
    {
        [SerializeField] protected SpaceFoundationData spaceFoundationData;
        private bool HasData => spaceFoundationData != null;
        
        [Dropdown(nameof(availableAnchors))]
        [ShowIf(nameof(HasData))]
        [SerializeField] protected string anchorID;
        private string[] availableAnchors => spaceFoundationData?.anchors.ToDictionary().Keys.ToArray() ?? new string[]{};

        protected override QuestValidationInformation HandleValidation()
        {
            if (!spaceFoundationData)
                return QuestValidationInformation.Failure("No SpaceFoundation Data!");
            if (!spaceFoundationData.anchors.Contains(anchorID))
                return QuestValidationInformation.Failure($"The anchorsID '{anchorID}' is not part of the provided SpaceFoundationData {spaceFoundationData.name}");
            return QuestValidationInformation.Success();
        }

        internal override string GetDescription()
        {
            return anchorID;
        }

        public bool Receive(string receivedAnchorID)
        {
            if (receivedAnchorID.Equals(anchorID))
            {
                Complete();
                return true;
            }
            return false;
        }

        public bool Steal(string donation)
        {
            return false;
        }
    }
}