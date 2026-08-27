using System;
using giorgiokalmund.Dora.Questing;
using UnityEngine;
using UnityEngine.Assertions;

namespace giorgiokalmund.Dora.Samples.Scripts
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
        [SerializeField] private MarkerAction phaseToAdvance;

        [Header("World")] 
        [SerializeField] private Collider markerArea;
        [SerializeField] private MeshRenderer visuals;

        private void Awake()
        {
            Assert.IsNotNull(quest, "QuestMarker needs a quest to work!");
            
            QuestPhase relatedPhase = phaseToAdvance switch
            {
                MarkerAction.MENTION => QuestPhase.MENTIONED,
                MarkerAction.START => QuestPhase.ACCEPTED,
                MarkerAction.TRY_COMPLETE => QuestPhase.COMPLETED,
                _ => throw new Exception("Unhandled MarkerAction!")
            };
            visuals.material.color = relatedPhase.GetColor();
        }

        private void OnTriggerEnter(Collider _)
        {
            switch (phaseToAdvance)
            {
                case MarkerAction.MENTION:
                {
                    visuals.material.color = QuestPhase.MENTIONED.GetColor();
                    QuestManager.Current.MentionQuest(quest);
                    break;
                }
                case MarkerAction.START:
                {
                    visuals.material.color = QuestPhase.ACCEPTED.GetColor();
                    QuestManager.Current.StartQuest(quest);
                    break;
                }
                case MarkerAction.TRY_COMPLETE:
                {
                    visuals.material.color = QuestPhase.COMPLETED.GetColor();
                    QuestManager.Current.CompleteQuest(quest);
                    break;
                }
            }
        }
    }
}