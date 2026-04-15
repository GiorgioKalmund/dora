using UnityEngine;

namespace giorgiokalmund.Dora.Requirements
{
    [CreateAssetMenu(fileName = "Coin", menuName = "Dora/Requirements/Coin")]
    public class CoinRequirement : QuestBehaviourCountRequirement<MonoBehaviour>
    {
        public override string GetDescription()
        {
            return base.GetDescription();
        }
    }
}