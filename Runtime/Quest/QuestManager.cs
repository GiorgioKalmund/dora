using System.Linq;
using giorgiokalmund.Dora.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace giorgiokalmund.Dora
{
    /// <summary>
    /// Manages interactions with the questing system.
    /// </summary>
    [CreateAssetMenu(fileName = "QuestManager", menuName = "Dora/QuestManager")] 
    public class QuestManager : ScriptableObject, IComponentOwner
    {
        [field: SerializeField]
        public Quest[] all;

        public UnityEvent<Quest, QuestState> onQuestStateChanged;

        #region Filtered Quests
        public Quest[] Unknown => all.Where(q => q.State == QuestState.UNKNOWN).ToArray();
        public Quest[] Mentioned => all.Where(q => q.State == QuestState.MENTIONED).ToArray();
        public Quest[] Accepted => all.Where(q => q.State == QuestState.ACCEPTED).ToArray();
        public Quest[] Achieved => all.Where(q => q.State == QuestState.ACHIEVED).ToArray();
        public Quest[] Completed => all.Where(q => q.State == QuestState.COMPLETED).ToArray();
        #endregion
        
        public bool TryUpdateQuest(string id, object value)
        {
            return all
                       .FirstOrDefault(q => q.Information?.Id.Equals(id) ?? false)
                       ?.TryDonate(value) 
                   ?? false;
        }

        private void OnValidate()
        {
            foreach (var quest in all)
                quest.AddTo(this);
        }

        public bool TryUpdateAnyQuest(object value)
        {
            if (all.Length == 0)
            {
                QuestLogger.LogWarning($"[{GetType()}]: QuestManager has no quests!");
                return false;
            }

            bool success = false;
            foreach (var q in all)
                if (q.TryDonate(value))
                    success = true;

            return success;
        }

        public void AddComponent<T>(BaseComponent<T> component) where T : IComponentOwner
        {
            var casted = component as Quest;
            if (casted is null)
            {
                QuestLogger.LogError($"[{GetType()}]: Cannot add {component} as component. Incompatible controller type.");
            }
        }
    }
        

}