using giorgiokalmund.Dora.Questing;
using UnityEngine;
using UnityEngine.Assertions;

namespace giorgiokalmund.Dora.Samples.Scripts
{
    public class QuestMarker3 : MonoBehaviour
    {
        [Header("Quest")]
        [SerializeField] private Quest quest;

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
            var n = phase.GetNext();
            if (n.HasValue && n != QuestPhase.ACHIEVED)
                visuals.material.color = n.Value.GetColor();
            else
                visuals.material.color = Color.gray;
        }

        private void OnTriggerEnter(Collider _)
        {
            QuestManager.Current.AdvanceQuestPhase(quest);
        }
    }
}