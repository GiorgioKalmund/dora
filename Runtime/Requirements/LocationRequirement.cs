using SpaceFoundationSystem;
using UnityEngine;

namespace giorgiokalmund.Dora.Requirements
{
    [CreateAssetMenu(fileName = "Location", menuName = "Dora/Requirements/Location")]
    public class LocationRequirement : QuestRequirements
    {
        [SerializeField] private string forLocation;
        private SpaceFoundation _spaceFoundation;
        public override QuestValidationInformation Validate()
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

        public override string GetDescription()
        {
            return $"'{forLocation}' in {_spaceFoundation?.name ?? "<UNKNOWN-SFS>"}";
        }
    }
}