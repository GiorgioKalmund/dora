using System;
using giorgiokalmund.Dora.Questing;
using UnityEngine;
using UnityEngine.Assertions;

namespace giorgiokalmund.Dora.Samples.Scripts
{
    public class QuestMarker2 : MonoBehaviour
    {
        [Header("Quest")]
        [SerializeField] private Quest quest;
        [SerializeField] private MarkerAction phaseToMatch;

        [Header("World")] 
        [SerializeField] private Collider markerArea;
        [SerializeField] private MeshRenderer visuals;

        private void Awake()
        {
            Assert.IsNotNull(quest, "QuestMarker needs a quest to work!");
            
            quest.onPhaseChanged.AddListener(HandleStateChanged);
            HandleStateChanged(quest.Phase);
        }

        private void HandleStateChanged(QuestPhase phase)
        {
            visuals.material.color = PhaseColor;
        }

        private QuestPhase ReactionPhase => phaseToMatch switch
        {
            MarkerAction.MENTION => QuestPhase.UNKNOWN,
            MarkerAction.START => QuestPhase.MENTIONED,
            MarkerAction.TRY_COMPLETE => QuestPhase.ACHIEVED,
            _ => throw new Exception("Unhandled MarkerAction!")
        };
        
        private Color PhaseColor => ReactionPhase == quest.Phase ? ReactionPhase.GetNext()!.Value.GetColor() : Color.gray;

        private void OnTriggerEnter(Collider _)
        {
            switch (phaseToMatch)
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