using System;
using UnityEngine;

namespace giorgiokalmund.Dora
{
    [Serializable]
    [CreateAssetMenu(fileName = "QuestStep", menuName = "Dora/QuestStep", order = 3)]
    public class QuestStep : ScriptableObject
    {
        [field: ReadOnly]
        [field: SerializeField, Tooltip("Whether this step has been completed.")]
        public bool IsCompleted { get; protected set; }
        
        [field: SerializeField, Tooltip("Requirements to which need to be in place for the quest to be viable.")]
        public QuestRequirements Requirements { get; protected set; }
        
    }
}