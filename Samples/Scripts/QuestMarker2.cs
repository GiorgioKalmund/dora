using System;
using giorgiokalmund.Dora.Questing;
using UnityEngine;
using UnityEngine.Assertions;

namespace giorgiokalmund.Dora
{
    public class QuestMarker2 : MonoBehaviour
    {
        [Header("Quest")]
        [SerializeField] private Quest quest;
        [SerializeField] private MarkerAction stateToMatch;

        [Header("World")] 
        [SerializeField] private Collider markerArea;
        [SerializeField] private MeshRenderer visuals;

        private void Awake()
        {
            Assert.IsNotNull(quest, "QuestMarker needs a quest to work!");
            
            quest.onStateChanged.AddListener(HandleStateChanged);
            HandleStateChanged(quest.State);
        }

        private void HandleStateChanged(QuestState state)
        {
            visuals.material.color = StateColor;
        }

        private QuestState ReactionState => stateToMatch switch
        {
            MarkerAction.MENTION => QuestState.UNKNOWN,
            MarkerAction.START => QuestState.MENTIONED,
            MarkerAction.TRY_COMPLETE => QuestState.ACHIEVED,
            _ => throw new Exception("Unhandled MarkerAction!")
        };
        
        private Color StateColor => ReactionState == quest.State ? ReactionState.GetNext()!.Value.GetColor() : Color.gray;

        private void OnTriggerEnter(Collider _)
        {
            switch (stateToMatch)
            {
                case MarkerAction.MENTION:
                {
                    QuestManager.Current.MentionQuest(quest);
                    break;
                }
                case MarkerAction.START:
                {
                    QuestManager.Current.StartQuest(quest);
                    break;
                }
                case MarkerAction.TRY_COMPLETE:
                {
                    QuestManager.Current.CompleteQuest(quest);
                    break;
                }
            }
        }
    }
}