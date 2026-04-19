using SpaceFoundationSystem;
using UnityEngine;

namespace giorgiokalmund.Dora.Requirements
{
    [CreateAssetMenu(fileName = "Location", menuName = "Dora/Requirements/Location")]
    public class LocationRequirement : QuestRequirements, IDonator<Anchor>
    {
        [SerializeField] private string forLocation;
        private SpaceFoundation _spaceFoundation;
        
        protected override QuestValidationInformation HandleValidation()
        {
            _spaceFoundation = FindAnyObjectByType<SpaceFoundation>();
            if (!_spaceFoundation)
                return QuestValidationInformation.Failure("No SFS found!");
            if (!_spaceFoundation.data)
                return QuestValidationInformation.Failure("No SFS Data found!");
            if(!_spaceFoundation.data.anchors.ToDictionary().TryGetValue(forLocation, out _))
                return QuestValidationInformation.Failure($"Cannot find Anchor '{forLocation}' in SFS!");

            return QuestValidationInformation.Success();
        }

        internal override string GetDescription()
        {
            return $"'{forLocation}' in {_spaceFoundation?.name ?? "<UNKNOWN-SFS>"}";
        }

        public bool Receive(Anchor donation)
        {
            if (donation.GetUniqueId().Equals(forLocation))
            {
                OnComplete.Invoke();
                return true;
            }
            return false;
        }

        public bool Steal(Anchor donation)
        {
            return false;
        }
    }
}