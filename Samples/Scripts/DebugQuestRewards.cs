using giorgiokalmund.Dora.Questing;
using UnityEngine;

namespace giorgiokalmund.Dora.Testing
{
    [CreateAssetMenu(fileName = "DebugQuestRewards", menuName = "Dora/Rewards/DebugQuestRewards")]
    public class DebugQuestRewards : QuestRewards
    {
        public override void HandOut()
        {
            DoraLogger.Log($"{name} has handed out rewards!");
        }
    }
}