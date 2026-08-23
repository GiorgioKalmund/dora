using giorgiokalmund.Dora.Questing;
using UnityEngine;
using UnityEngine.Assertions;

namespace giorgiokalmund.Dora.Samples.Scripts
{
    public class QuestResetter : MonoBehaviour
    {
        [Header("Quest")]
        [SerializeField] private Quest quest;

        [Header("World")] 
        [SerializeField] private Collider markerArea;
        [SerializeField] private MeshRenderer visuals;

        private void Awake()
        {
            Assert.IsNotNull(quest, "QuestMarker needs a quest to work!");
            
            visuals.material.color = Color.red;
        }

        private void OnTriggerEnter(Collider _)
        {
            quest.ResetQuest();
        }
    }
}