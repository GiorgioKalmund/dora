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
        
        [Dropdown(nameof(AvailableAnchors))]
        [ShowIf(nameof(HasData))]
        [SerializeField] protected string anchorID;
        private string[] AvailableAnchors => spaceFoundationData?.anchors.entries.Select(e => e.Key).ToArray() ?? new string[]{};

        protected override QuestValidationInformation HandleValidation()
        {
            if (!spaceFoundationData)
                return QuestValidationInformation.Failure("No SpaceFoundation Data!");
            if (!spaceFoundationData.anchors.Contains(anchorID))
                return QuestValidationInformation.Failure($"The anchorsID '{anchorID}' is not part of the provided SpaceFoundationData {spaceFoundationData.name}");
            return QuestValidationInformation.Success();
        }

        public override string GetDescription()
        {
            return $"Visit {GetAnchorNameOrWarning()}";
        }

        private string GetAnchorNameOrWarning()
        {
            if (SpaceFoundation.Current.TryGetAnchor(anchorID, out var anchor))
            {
                return anchor.name;
            }

            return $"<color=red>{anchorID} not part of the SFS!</color>";
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