using System.Collections.Generic;
using System.Linq;
using giorgiokalmund.Dora.Requirements;
using giorgiokalmund.Dora.Utils;
using SpaceFoundationSystem;
using UnityEngine;
using UnityEngine.Events;

namespace giorgiokalmund.Dora
{
    /// <summary>
    /// Manages interactions with the questing system.
    /// </summary>
    public class QuestManager : MonoBehaviour, IComponentOwner
    {
        public static QuestManager Current { get; private set; }
        
        public UnityEvent<Quest, QuestState> onQuestStateChanged;

        private LocationMember _mainActor;
        
        [SerializeField] public Quest[] all;
        
        #region Filtered Quests
        private Quest[] Unknown => all?.Where(q => q.State == QuestState.UNKNOWN).ToArray();
        private Quest[] Mentioned => all?.Where(q => q.State == QuestState.MENTIONED).ToArray();
        private Quest[] Accepted => all?.Where(q => q.State == QuestState.ACCEPTED).ToArray();
        private Quest[] Achieved => all?.Where(q => q.State == QuestState.ACHIEVED).ToArray();
        private Quest[] Completed => all?.Where(q => q.State == QuestState.COMPLETED).ToArray();
        #endregion


        protected void Awake()
        {
            if (Current)
            {
                DoraLogger.LogError("There is already a quest manager registered in the scene.", this);
                return;
            }
            Current = this;
        }
        
        private void Start()
        {
            if (all == null)
                return;
            foreach (var quest in all)
                quest?.AddTo(this);
            
            foreach (var quest in all)
                quest.InitForScene();
        }

        public void RegisterMainActor(LocationMember locationMember)
        {
            _mainActor = locationMember;
            _mainActor.onLocationChanged.AddListener(HandleMainActorLocationChanged);
        }
        
        public void UnregisterMainActor(LocationMember locationMember)
        {
            if (locationMember != _mainActor)
            {
                Debug.LogError($"Trying to deinitialize QuestManager with different main actor ('{locationMember.gameObject.name}') than what it was initialized with ('{_mainActor.gameObject.name}')");
                return;
            }
            locationMember.onLocationChanged.RemoveListener(HandleMainActorLocationChanged);
        }

        private void HandleMainActorLocationChanged(Anchor newLocation)
        {
            var currentLocations = Accepted.Select(s => s.CurrentStep).Where(r => r is LocationStep);

            foreach (var req in currentLocations)
            {
                if (req is IDonator<string> donator)
                    donator.Receive(newLocation.GetUniqueId());
            }
        }

        public bool MentionQuest(Quest quest) => SetQuestStateInternal(quest, QuestState.MENTIONED);
        public bool StartQuest(Quest quest) => SetQuestStateInternal(quest, QuestState.ACCEPTED);
        public bool CompleteQuest(Quest quest) => SetQuestStateInternal(quest, QuestState.COMPLETED);
      

        private bool SetQuestStateInternal(Quest quest, QuestState state)
        {
            if (!all.Contains(quest))
            {
                DoraLogger.LogError($"Cannot move {quest.Information} to state {state}. Not tracked by this QuestManager.", this);
                return false;
            }

            return quest.TrySetState(state);
        }
        
        public void AddComponent<T>(BaseComponent<T> component) where T : IComponentOwner
        {
            var quest = component as Quest;
            if (quest is null)
            {
                DoraLogger.LogError($"[{GetType()}]: Cannot add {component} as component. Incompatible controller type.");
                return;
            }

            var result = quest.GetInternalValidationResult();
            if (result.Length != 0)
                DoraLogger.LogError($"[{quest}]: {result.Length} Validation issues.");
        }
    }
        

}