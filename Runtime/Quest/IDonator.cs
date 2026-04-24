using System;

namespace giorgiokalmund.Dora
{
    public interface IDonator
    {
        public bool CanDonate(object value);
        public bool Donate(object value);
    }

    public interface IQuickDonator
    {
        public bool QuickDonate();
    }
    public interface IQuickDonator<TDonation, VTarget> : IQuickDonator where VTarget : IDonator<TDonation>
    {
        public TDonation GetQuickDonation();

        bool IQuickDonator.QuickDonate()
        {
            if (this is VTarget target)
                return target.Donate(GetQuickDonation());
            QuestLogger.LogWarning($"Quick donation to {GetType().Name} failed. Please ensure that the quick donation interface is set up correctly: 'IQuickDonator<{typeof(TDonation).Name},{GetType().Name}>'");
            return false;
        }
    }
    
    public interface IDonator<T> : IDonator
    {
        bool IDonator.Donate(object value) => Receive((T)value);
        bool IDonator.CanDonate(object value) => value is T;
        public bool Receive(T donation);
        public bool Steal(T donation);
    }
}