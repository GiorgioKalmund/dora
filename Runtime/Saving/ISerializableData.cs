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
        // Force implementors to handle the equality
    }
}