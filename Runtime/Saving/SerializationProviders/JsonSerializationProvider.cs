using System;
using UnityEngine;

namespace giorgiokalmund.Dora.Saving.SerializationProviders
{
    public class JsonSerializationProvider : ISerializationProvider
    {
        public string SerializeData(ISerializableData data)
        {
            return JsonUtility.ToJson(data);
        }

        public T DeserializeData<T>(string source) where T : struct, ISerializableData
        {
            return JsonUtility.FromJson<T>(source);
        }

        public object DeserializeData(string source, Type type)
        {
            return JsonUtility.FromJson(source, type);
        }
    }
}