using System.Collections.Generic;
using giorgiokalmund.Dora.Questing;
using UCGUI;
using UnityEngine;

namespace giorgiokalmund.Dora
{
    public class QuestCardStack : VStackComponent
    {
        private Dictionary<Quest, QuestCard> _currentQuests = new();

        private void HandleQuestUpdate(Quest quest, QuestState newState)
        {
            if (newState == QuestState.UNKNOWN)
            {
                if (_currentQuests.Remove(quest, out QuestCard card))
                {
                    Destroy(card.gameObject);
                }

                return;
            }

            if (!_currentQuests.ContainsKey(quest))
            {
                var newCard = UI.N<QuestCard>().Init(quest);
                _currentQuests.Add(quest, newCard);
                Add(newCard);
            }
        }

        protected override void Awake()
        {
            base.Awake();
            QuestManager.Current.onQuestStateChanged.AddListener(HandleQuestUpdate);
            DisplayName = "Quest Stack";
            Spacing(20);
        }

        private void OnDestroy()
        {
            QuestManager.Current.onQuestStateChanged.RemoveListener(HandleQuestUpdate);
        }
    }
}
