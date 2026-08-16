using System.Collections.Generic;
using System.Linq;
using giorgiokalmund.Dora.Questing;
using giorgiokalmund.Dora.Questing.Events;
using giorgiokalmund.Dora.Utils;
using SpaceFoundationSystem;
using UnityEngine;
using UnityEngine.Events;
using Assert = UnityEngine.Assertions.Assert;

namespace giorgiokalmund.Dora
{
    /// <summary>
    /// Manages interactions with the questing system.
    /// </summary>
    public class QuestManager : MonoBehaviour, IComponentOwner
    {
        [Header("Events")]
        public UnityEvent<Quest, QuestState> onQuestStateChanged = new UnityEvent<Quest, QuestState>();
        
        [Header("Singleton")]
        [SerializeField, Tooltip("Whether to initialize the QuestManager using the Singleton pattern.")]
        private bool makeSingleton = true;
        public static QuestManager Current { get; private set; }
        
        [Header("Quests")]
        [SerializeField] public Quest[] all;

        private HashSet<Quest> _currentlyRegistered = new HashSet<Quest>();

        #region EventBus

        private GameplayEventBus _eventBus;
        public static GameplayEventBus EventBus => Current?._eventBus;

        #endregion

        #region MainActor

        private LocationMember _mainActor;

        #endregion
        
        
        protected void Awake()
        {
            if (makeSingleton)
            {
                if (Current)
                {
                    DoraLogger.LogError("There is already a quest manager registered in the scene.", this);
                    Destroy(gameObject);
                    return;
                } 
                
                Current = this;
            }
            
            _eventBus = new GameplayEventBus();
            
            // Prepare gameplay hooks early
            foreach (var quest in all.Where(s => s.State == QuestState.ACCEPTED))
            {
                Register(quest);
            }
            
            onQuestStateChanged.AddListener(HandleQuestStateChanged);
        }
        
        private void Start()
        {
            if (all == null)
                return;
            foreach (var quest in all)
                quest.AddTo(this);
            
            foreach (var quest in all)
                quest.OnQuestManagerInit();
        }

        private void OnDestroy()
        {
            onQuestStateChanged.RemoveListener(HandleQuestStateChanged);
            
            foreach (var quest in all)
                quest.OnQuestManagerDeinit();
        }
        
        private void HandleQuestStateChanged(Quest quest, QuestState newState)
        {
            // if a quest has been reset to a non-accepted state via rollback we need to unregister it again manually here
            if (newState < QuestState.ACCEPTED && _currentlyRegistered.Contains(quest))
            {
                Unregister(quest);
            }
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
            _eventBus.Publish(new EnteredLocationEvent(newLocation));
        }

        public bool MentionQuest(Quest quest) => SetQuestStateInternal(quest, QuestState.MENTIONED);

        public bool StartQuest(Quest quest)
        {
            var success = SetQuestStateInternal(quest, QuestState.ACCEPTED);
            if (!success)
                return false;
            
            Register(quest);
            // TODO: Maybe boolean which checks if we should auto check the location etc on quest start / step start...
            HandleMainActorLocationChanged(_mainActor.currentLocation);
            return true;
        }

        public bool CompleteQuest(Quest quest)
        {
            var success = SetQuestStateInternal(quest, QuestState.COMPLETED);
            if (!success)
                return false;
            // TODO: Maybe more here, else just lambda (like MentionQuest)
            return true;
        } 
        
        public bool AdvanceQuest(Quest quest) => AdvanceQuestStateInternal(quest);

        private bool AdvanceQuestStateInternal(Quest quest)
        {
            if (!all.Contains(quest))
            {
                DoraLogger.LogError($"Cannot advance {quest.Information}. Not tracked by this QuestManager.", this);
                return false;
            }

            // Explicitly handle start flow
            if (quest.State == QuestState.MENTIONED)
                return StartQuest(quest);
            
            if (quest.State == QuestState.ACHIEVED)
                return CompleteQuest(quest);
            
            return quest.TryAdvanceState();
        }

        private bool SetQuestStateInternal(Quest quest, QuestState state)
        {
            if (!all.Contains(quest))
            {
                DoraLogger.LogError($"Cannot move {quest.Information} to state {state}. Not tracked by this QuestManager.", this);
                return false;
            }

            return quest.TrySetState(state);
        }
        
        public bool BotchQuest(Quest quest)
        {
            if (!all.Contains(quest))
            {
                DoraLogger.LogError($"Cannot botch {quest.Information}. Not tracked by this QuestManager.", this);
                return false;
            }

            return quest.Botch();
        }

        public void ResetQuest(Quest quest)
        {
            if (!all.Contains(quest))
            {
                DoraLogger.LogError($"Cannot reset {quest.Information}. Not tracked by this QuestManager.", this);
                return ;
            }
            
            quest.ResetQuest();
        }
        
        public void Register(Quest quest)
        {
            if (_currentlyRegistered.Contains(quest))
            {
                DoraLogger.LogError($"The quest '{quest.Information}' has already been registered by the manager. Please refrain from re-registering the same quest twice.");
                return;
            }
            
            // TODO: Instead of listening to all, maybe filter out quest first or something or make the 
            Assert.IsNotNull(_eventBus, "QuestManager should have an event bus!");
            _eventBus.OnPublished.AddListener(quest.Process);
            
            quest.onComplete.AddListener(Unregister);
            quest.onReset.AddListener(Unregister);
            quest.onBotch.AddListener(Unregister);
            
            _currentlyRegistered.Add(quest);
        }

        private void Unregister(Quest quest)
        {
            if (!_currentlyRegistered.Contains(quest))
            {
                DoraLogger.LogError($"The quest '{quest.Information}' has not yet been registered by the manager. Please refrain from unregistering an untracked quest.");
                return;
            }
            
            _currentlyRegistered.Remove(quest);
            
            quest.onBotch.RemoveListener(Unregister);
            quest.onReset.RemoveListener(Unregister);
            quest.onComplete.RemoveListener(Unregister);
            
            Assert.IsNotNull(_eventBus, "QuestManager should have an event bus!");
            _eventBus.OnPublished.RemoveListener(quest.Process);
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