namespace giorgiokalmund.Dora
{
    public interface IDonator
    {
        public bool CanDonate(object value);
        public bool Donate(object value);
    }
    
    public interface IDonator<T> : IDonator
    {
        bool IDonator.Donate(object value) => Receive((T)value);
        bool IDonator.CanDonate(object value) => value is T;
        public bool Receive(T donation);
        public bool Steal(T donation);
    }
}