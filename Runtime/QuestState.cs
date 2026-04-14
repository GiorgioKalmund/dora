namespace giorgiokalmund.Dora
{
    /// <summary>
    /// State of a quest. Only moves forward unless quest is reset or similar.
    ///
    /// <see cref="UNKNOWN"/> -> <see cref="MENTIONED"/> -> <see cref="ACCEPTED"/> -> <see cref="ACHIEVED"/> -> <see cref="COMPLETED"/>
    /// </summary>
    public enum QuestState
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
}