using System;
using UnityEngine;

namespace giorgiokalmund.Dora
{
    [Serializable]
    [CreateAssetMenu(fileName = "QuestInformation", menuName = "Dora/QuestInformation", order = 2)]
    public class QuestInformation : ScriptableObject
    {
        [field: ReadOnly]
        [field: SerializeField, Tooltip("Unique identifier for a Quest")]
        public string Id { get; protected set; }
        
        [field: SerializeField, Tooltip("Representative title for a Quest")]
        public string Title { get; protected set; }

        [field: SerializeField, Tooltip("Representative description for a Quest")]
        public string Description { get; protected set; }

        private void Awake()
        {
            Id = name;
        }
    }
}