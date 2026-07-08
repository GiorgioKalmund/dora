using giorgiokalmund.Dora.Questing;
using UnityEngine;
using UnityEngine.Assertions;

namespace giorgiokalmund.Dora
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
            
#if DEBUG
            quest.ResetQuest(); // TODO: Temporary
#endif
            
            quest.onStateChanged.AddListener(HandleStateChanged);
            HandleStateChanged(quest.State);
        }

        private void HandleStateChanged(QuestState state)
        {
            var n = state.GetNext();
            if (n.HasValue && n != QuestState.ACHIEVED)
                visuals.material.color = n.Value.GetColor();
            else
                visuals.material.color = Color.gray;
        }

        private void OnTriggerEnter(Collider _)
        {
            QuestManager.Current.AdvanceQuest(quest);
        }
    }
}