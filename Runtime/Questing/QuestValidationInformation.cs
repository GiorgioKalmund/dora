namespace giorgiokalmund.Dora.Questing
{
    public abstract class QuestValidationInformation
    {
        protected QuestValidationInformation() { }

        public bool IsFailure => this is QuestValidationFailure;
        public bool IsSuccess => this is QuestValidationSuccess;

        public static QuestValidationSuccess Success()
        {
            return new QuestValidationSuccess();
        }
        
        public static QuestValidationFailure Failure(string reason)
        {
            var information = new QuestValidationFailure();
            information.Reason = reason;
            return information;
        }
    }

    public class QuestValidationFailure : QuestValidationInformation
    {
        public string Reason { get; internal set;  }
        internal QuestValidationFailure() { }
    }
    
    public class QuestValidationSuccess : QuestValidationInformation
    {
        public string Reason { get; internal set;  }
        internal QuestValidationSuccess() { }
    }
}