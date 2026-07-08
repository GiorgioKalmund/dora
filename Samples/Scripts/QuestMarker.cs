using System;
using giorgiokalmund.Dora.Questing;
using UnityEngine;
using UnityEngine.Assertions;

namespace giorgiokalmund.Dora
{
    internal enum MarkerAction
    {
        MENTION,
        START,
        TRY_COMPLETE
    }
    
    public class QuestMarker : MonoBehaviour
    {
        [Header("Quest")]
        [SerializeField] private Quest quest;
        [SerializeField] private MarkerAction stateToAdvance;

        [Header("World")] 
        [SerializeField] private Collider markerArea;
        [SerializeField] private MeshRenderer visuals;

        private void Awake()
        {
            Assert.IsNotNull(quest, "QuestMarker needs a quest to work!");
            
#if DEBUG
            quest.ResetQuest(); // TODO: Temporary
#endif


            QuestState relatedState = stateToAdvance switch
            {
                MarkerAction.MENTION => QuestState.MENTIONED,
                MarkerAction.START => QuestState.ACCEPTED,
                MarkerAction.TRY_COMPLETE => QuestState.COMPLETED,
                _ => throw new Exception("Unhandled MarkerAction!")
            };
            visuals.material.color = relatedState.GetColor();
        }

        private void OnTriggerEnter(Collider _)
        {
            switch (stateToAdvance)
            {
                case MarkerAction.MENTION:
                {
                    visuals.material.color = QuestState.MENTIONED.GetColor();
                    QuestManager.Current.MentionQuest(quest);
                    break;
                }
                case MarkerAction.START:
                {
                    visuals.material.color = QuestState.ACCEPTED.GetColor();
                    QuestManager.Current.StartQuest(quest);
                    break;
                }
                case MarkerAction.TRY_COMPLETE:
                {
                    visuals.material.color = QuestState.COMPLETED.GetColor();
                    QuestManager.Current.CompleteQuest(quest);
                    break;
                }
            }
        }
    }
}