namespace giorgiokalmund.Dora.Questing
{
    public enum ParentCompletionAction
    {
        /// <summary>
        /// No action is performed.
        /// </summary>
        NONE,
        /// <summary>
        /// Sets the state of the current quest to 'Mentioned'.
        /// </summary>
        MENTION,
        /// <summary>
        /// Sets the state of the current quest to 'Accepted'.
        /// </summary>
        ACCEPT
    }
}