using System;
using NaughtyAttributes;
using UnityEngine;

namespace giorgiokalmund.Dora
{
    [Serializable]
    public class QuestInformation 
    {
        public static int NextId { get; protected set; }
        
        [field: ReadOnly]
        [field: SerializeField, Tooltip("Unique identifier for a Quest")]
        public string Id { get; protected set; }
        
        [field: SerializeField, Tooltip("Representative title for a Quest")]
        public string Title { get; internal set; }

        [field: SerializeField, Tooltip("Representative description for a Quest")]
        public string Description { get; internal set; }

        public QuestInformation()
        {
            Id = "q" + GetNextID();
        }
        
        protected int GetNextID()
        {
            return NextId++;
        }
    }
}