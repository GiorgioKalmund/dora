using System;
using UnityEngine;

namespace giorgiokalmund.Dora.Questing
{
    /// <summary>
    /// State of a quest. Only moves forward unless quest is reset or similar.
    /// <see cref="UNKNOWN"/> -> <see cref="MENTIONED"/> -> <see cref="ACCEPTED"/> -> <see cref="ACHIEVED"/> -> <see cref="COMPLETED"/>
    /// </summary>
    [Serializable] 
    public enum QuestPhase
    {
        /// Quest is unknown
        UNKNOWN     = 0,
        /// Quest is known, however no action has been taken to accept it.
        MENTIONED   = 1,
        /// Quest has been accepted and can now be worked towards being completed.
        ACCEPTED    = 2,
        /// Objective(s) are ALL completed. Can be tied together with turning int <see cref="COMPLETED"/> directly after.
        ACHIEVED    = 3, 
        /// Resulting rewards have been given and actions have been performed.
        COMPLETED   = 4,
    }

    public static class QuestPhaseHelper
    {
        public static QuestPhase? GetNext(this QuestPhase phase)
        {
            switch (phase)
            {
                case QuestPhase.UNKNOWN: return QuestPhase.MENTIONED;
                case QuestPhase.MENTIONED: return QuestPhase.ACCEPTED;
                case QuestPhase.ACCEPTED: return QuestPhase.ACHIEVED;
                case QuestPhase.ACHIEVED: return QuestPhase.COMPLETED;
                case QuestPhase.COMPLETED: return null;
                default: return QuestPhase.UNKNOWN;
            }
        }
        
        public static Color GetColor(this QuestPhase phase)
        {
            switch (phase)
            {
                case QuestPhase.UNKNOWN: return Color.gray;
                case QuestPhase.MENTIONED: return Color.hotPink;
                case QuestPhase.ACCEPTED: return Color.deepSkyBlue;
                case QuestPhase.ACHIEVED: return Color.yellow;
                case QuestPhase.COMPLETED: return Color.green;
                default: return Color.white;
            }
        }
    }
}