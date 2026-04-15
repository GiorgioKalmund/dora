using System.Linq;
using giorgiokalmund.Dora.Requirements;

namespace giorgiokalmund.Dora
{
    /// <summary>
    /// Manages interactions with the questing system.
    /// </summary>
    public class QuestManager
    {
        public Quest[] All;

        #region Filtered Quests
        public Quest[] Unknown => All.Where(q => q.State == QuestState.UNKNOWN).ToArray();
        public Quest[] Mentioned => All.Where(q => q.State == QuestState.MENTIONED).ToArray();
        public Quest[] Accepted => All.Where(q => q.State == QuestState.ACCEPTED).ToArray();
        public Quest[] Achieved => All.Where(q => q.State == QuestState.ACHIEVED).ToArray();
        public Quest[] Completed => All.Where(q => q.State == QuestState.COMPLETED).ToArray();
        #endregion
    }
        

}