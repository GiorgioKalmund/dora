using System;
using JetBrains.Annotations;
using UnityEngine;

namespace giorgiokalmund.Dora
{
    [Serializable]
    public abstract class QuestRequirements : ScriptableObject
    {
        [NotNull]
        public abstract QuestValidationInformation Validate();

        /// Whether it is possible for the requirements to be achieved.
        public bool CanBeAchieved => Validate().IsSuccess;

        public abstract string GetDescription();
    }
}