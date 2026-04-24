using UnityEngine;

namespace giorgiokalmund.Dora.Requirements
{
    [CreateAssetMenu(fileName = "IntCount", menuName = "Dora/Requirements/IntCount")]
    public class IntCountRequirement : QuestCountRequirement<int>, IQuickDonator<int, IntCountRequirement>
    {
        [field: SerializeField, Tooltip("The currently collected units.")]
        [field: ReadOnly]
        public int Collected { get; private set; }

        protected override int GetCountOfCurrent()
        {
            return Collected;
        }

        protected override bool HandleReceive(int element)
        {
            Collected += element;
            return true;
        }

        protected override bool HandleSteal(int element)
        {
            if (Collected - element < 0)
                return false;
            Collected -= element;
            return true;
        }

        public override void ResetRequirements()
        {
            base.ResetRequirements();
            Collected = 0;
        }

        public bool Receive() { return Receive(1); }
        public int GetQuickDonation()
        {
            return 1;
        }
    }
}