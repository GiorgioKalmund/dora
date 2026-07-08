using System;
using UnityEngine;

namespace giorgiokalmund.Dora.Questing
{
    [Serializable]
    public abstract class QuestRewards : ScriptableObject
    {
        public abstract void HandOut();
    }
}