using giorgiokalmund.Dora.Questing;
using UnityEngine;

namespace giorgiokalmund.Dora.Samples.Scripts
{
    [CreateAssetMenu(fileName = "DebugQuestRewards", menuName = "Dora/Rewards/DebugQuestRewards")]
    public class DebugQuestRewards : QuestRewards
    {
        public override void HandOut()
        {
            DoraLogger.Log($"{name} has handed out rewards!");
        }

        public override void Retract()
        {
            DoraLogger.Log($"{name} has retracted their rewards!");
        }
    }
}