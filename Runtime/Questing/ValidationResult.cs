namespace giorgiokalmund.Dora.Questing
{
    public abstract class ValidationResult
    {
        protected ValidationResult() { }

        public bool IsFailure => this is ValidationFailure;
        public bool IsSuccess => this is ValidationSuccess;

        public static ValidationSuccess Success()
        {
            return new ValidationSuccess();
        }
        
        public static ValidationFailure Failure(string reason)
        {
            var information = new ValidationFailure();
            information.Reason = reason;
            return information;
        }
    }

    public class ValidationFailure : ValidationResult
    {
        public string Reason { get; internal set;  }
        internal ValidationFailure() { }
    }
    
    public class ValidationSuccess : ValidationResult
    {
        internal ValidationSuccess() { }
    }
}