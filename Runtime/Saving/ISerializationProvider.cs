using System;

namespace giorgiokalmund.Dora.Saving
{
    public interface ISerializationProvider
    {
        public string SerializeData(ISerializableData data);
        public T DeserializeData<T>(string source) where T : struct, ISerializableData;
        public object DeserializeData(string source, Type type);
    }
}