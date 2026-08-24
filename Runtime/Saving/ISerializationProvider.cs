using System;

namespace giorgiokalmund.Dora.Saving
{
    public interface ISerializationProvider
    {
        public string SerializeData(ISerializableData data);
        public bool DeserializeData<T>(string source, out T result) where T : struct, ISerializableData;
        public object DeserializeData(string source, Type type);
    }
}