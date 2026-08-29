using System;
using UnityEngine;

namespace giorgiokalmund.Dora.Questing.Events
{
    [Serializable]
    public struct RewardOption
    {
        /// <summary>
        /// Whether to hand out the rewards early in a separate 'Achieved' state.
        /// </summary>
        [Tooltip("Whether to hand out the rewards early in a separate 'Achieved' state.")]
        public bool handoutOnAchieved;
            
        /// <summary>
        /// The rewards to hand out.
        /// </summary>
        [Tooltip("The rewards to hand out.")]
        public QuestRewards rewards;
    }

}