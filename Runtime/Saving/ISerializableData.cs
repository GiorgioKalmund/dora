using System;

namespace giorgiokalmund.Dora.Saving
{
    public interface ISerializableData : IDisposable
    {
        
    }

    public interface ISerializableData<T> :
        IEquatable<T>,
        ISerializableData where T : struct, ISerializableData
    {
        bool IEquatable<T>.Equals(T other)
        {
            // TODO: Maybe false incorrect!
            return false;
        }
    }
}