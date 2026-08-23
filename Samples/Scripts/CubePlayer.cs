using giorgiokalmund.Dora.Questing.Events;
using SpaceFoundationSystem;

namespace giorgiokalmund.Dora.Samples.Scripts
{
    public class CubePlayer : LocationMember
    {
        public void Awake()
        {
            onLocationChanged.AddListener(TellQuestSystemAboutNewLocation);
        }

        private void TellQuestSystemAboutNewLocation(Anchor anchor)
        {
            QuestManager.EventBus.Publish(new MemberEnteredLocationEvent(this, anchor));
        }
    }
}