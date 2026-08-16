using System;

namespace giorgiokalmund.Dora.Saving
{
    [Serializable]
    public struct QuestStepSnapshot : ISerializableData<QuestStepSnapshot>
    {
        public bool isCompleted;
        public string serializedData;

        public void Dispose()
        {
            
        }

        public bool Equals(QuestStepSnapshot other)
        {
            return isCompleted == other.isCompleted && serializedData == other.serializedData;
        }

        public override bool Equals(object obj)
        {
            return obj is QuestStepSnapshot other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(isCompleted, serializedData);
        }
    }
}