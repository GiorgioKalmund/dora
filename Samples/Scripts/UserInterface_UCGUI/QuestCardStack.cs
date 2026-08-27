using System.Collections.Generic;
using System.Linq;
using giorgiokalmund.Dora.Questing;
using UCGUI;
using UnityEngine.Assertions;

namespace giorgiokalmund.Dora.Samples.Scripts.UserInterface_UCGUI
{
    public class QuestCardStack : VStackComponent
    {
        private Dictionary<Quest, QuestCard> _currentQuests = new();

        private void HandleQuestUpdate(Quest quest, QuestPhase newPhase)
        {
            if (newPhase == QuestPhase.UNKNOWN)
            {
                if (_currentQuests.Remove(quest, out QuestCard card))
                {
                    Destroy(card.gameObject);
                }

                return;
            }

            if (!_currentQuests.ContainsKey(quest))
            {
                CreateQuestCard(quest);
            }
        }

        private void CreateQuestCard(Quest quest)
        {
            Assert.IsTrue(!_currentQuests.ContainsKey(quest));
            var newCard = UI.N<QuestCard>().Init(quest);
            _currentQuests.Add(quest, newCard);
            Add(newCard);
        }

        protected override void Awake()
        {
            base.Awake();
            QuestManager.Current.onQuestStateChanged.AddListener(HandleQuestUpdate);
            DisplayName = "Quest Stack";
            Spacing(20);

            foreach (var quest in QuestManager.Current.all.Where(q => q.Phase != QuestPhase.UNKNOWN))
            {
                HandleQuestUpdate(quest, quest.Phase);
            }

            /* TODO: Test fully integrated initialization on startup. 
            foreach (var quest in QuestManager.Current.all.Where(s => s.State != QuestState.UNKNOWN))
            {
                CreateQuestCard(quest);
                QuestManager.Current.Register(quest);
            }
            */
        }

        private void OnDestroy()
        {
            QuestManager.Current.onQuestStateChanged.RemoveListener(HandleQuestUpdate);
        }
    }
}
