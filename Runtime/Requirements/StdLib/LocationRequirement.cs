using System;
using SpaceFoundationSystem;
using UnityEngine;

namespace giorgiokalmund.Dora.Requirements
{
    [CreateAssetMenu(fileName = "Location", menuName = "Dora/Requirements/Location")]
    public class LocationRequirement : QuestRequirements, IDonator<Anchor>
    {
        [SerializeField] protected internal string forLocation;
        protected internal SpaceFoundation SpaceFoundation;

        private void OnEnable()
        {
            SpaceFoundation = FindAnyObjectByType<SpaceFoundation>();
        }

        protected override QuestValidationInformation HandleValidation()
        {
            SpaceFoundation = FindAnyObjectByType<SpaceFoundation>();
            if (!SpaceFoundation)
                return QuestValidationInformation.Failure("No SFS found!");
            if (!SpaceFoundation.data)
                return QuestValidationInformation.Failure("No SFS Data found!");
            if (string.IsNullOrEmpty(forLocation))
                return QuestValidationInformation.Failure("No Location provided!");
            if (!SpaceFoundation.data.anchors.ToDictionary().TryGetValue(forLocation, out _))
                return QuestValidationInformation.Failure($"Cannot find Anchor '{forLocation}' in {SpaceFoundation.name}!");

            return QuestValidationInformation.Success();
        }

        internal override string GetDescription()
        {
            return $"'{SpaceFoundation?.TryGetAnchor(forLocation)?.name ?? "<UNKNOWN>"}' in {SpaceFoundation?.name ?? "<UNKNOWN-SFS>"}";
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