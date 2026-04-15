using System;
using UnityEngine;

namespace giorgiokalmund.Dora
{
    [Serializable]
    public abstract class QuestRewards : ScriptableObject
    {
        public abstract void HandOut();
    }
}