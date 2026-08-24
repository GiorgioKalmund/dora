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

        public bool DeserializeData<T>(string source, out T result) where T : struct, ISerializableData
        {
            result = new T();
            try
            {
                result = JsonUtility.FromJson<T>(source);
                return true;
            }
            catch (ArgumentException e)
            {
                DoraLogger.LogError(e.Message);
                return false;
            }
        }

        public object DeserializeData(string source, Type type)
        {
            try
            {
                return JsonUtility.FromJson(source, type);
            }
            catch (ArgumentException e)
            {
                DoraLogger.LogError(e.Message);
                return null;
            }
        }
    }
}