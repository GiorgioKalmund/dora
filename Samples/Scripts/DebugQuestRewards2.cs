using giorgiokalmund.Dora.Questing;
using UnityEngine;

namespace giorgiokalmund.Dora.Samples.Scripts
{
    [CreateAssetMenu(fileName = "DebugQuestRewards2", menuName = "Dora/Rewards/DebugQuestRewards2")]
    public class DebugQuestRewards2 : QuestRewards
    {
        public override void HandOut()
        {
            DoraLogger.Log($"{name} (2) has handed out rewards!");
        }

        public override void Retract()
        {
            DoraLogger.Log($"{name} (2) has retracted their rewards!");
        }
    }
}