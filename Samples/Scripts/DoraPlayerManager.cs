using giorgiokalmund.Dora.Questing.Events;
using SpaceFoundationSystem;
using UnityEngine;

namespace giorgiokalmund.Dora.Samples.Scripts
{
    public class DoraPlayerManager : MonoBehaviour
    {
        #region MainActor
        [SerializeField] public LocationMember mainActor;
        #endregion
        
        public static DoraPlayerManager Current { get; private set; }

        private void Awake()
        {
            if (Current && Current != this)
                Destroy(this);
            else
                Current = this;
        }

        public void RegisterMainActor(LocationMember locationMember)
        {
            mainActor = locationMember;
            mainActor.onLocationChanged.AddListener(HandleMainActorLocationChanged);
        }
        
        public void UnregisterMainActor(LocationMember locationMember)
        {
            if (locationMember != mainActor)
            {
                Debug.LogError($"Trying to deinitialize QuestManager with different main actor ('{locationMember.gameObject.name}') than what it was initialized with ('{mainActor.gameObject.name}')");
                return;
            }
            locationMember.onLocationChanged.RemoveListener(HandleMainActorLocationChanged);
        }

        private void HandleMainActorLocationChanged(Anchor newLocation)
        {
            QuestManager.EventBus.Publish(new MemberEnteredLocationEvent(mainActor, newLocation));
        }

    }
}