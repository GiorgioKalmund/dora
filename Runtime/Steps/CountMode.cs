namespace giorgiokalmund.Dora.Steps
{
    [System.Serializable] 
    public enum CountMode
    {
        /// The requirement amount must be EXACTLY EQUAL to the amount of objects found.
        EXACT,
        /// The requirement amount must be AT LEAST EQUAL to the amount of objects found.
        MINIMUM,
        /// The requirement amount must be NO MORE THAN the amount of objects found.
        MAXIMUM 
    }
}