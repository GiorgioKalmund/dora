namespace giorgiokalmund.Dora
{
    public class Quest
    {
        /// Cannot be recovered or completed. Can be set during every state except if already <see cref="QuestState.COMPLETED"/>.
        public bool IsBotched { get; protected set; }
        /// The state of the quest. Can only move forward. (Unless restarted / reset)
        public QuestState State { get; protected set; }

        public QuestInformation Information { get; protected set; }
        
        public QuestRequirements Requirements { get; protected set;  }
    }
}