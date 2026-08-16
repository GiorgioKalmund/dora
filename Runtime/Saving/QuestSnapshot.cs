using System;
using giorgiokalmund.Dora.Questing;

namespace giorgiokalmund.Dora.Saving
{
    [Serializable]
    public struct QuestSnapshot : ISerializableData
    {
        public QuestState state;
        public bool isBotched;
        public int currentStep;
        public QuestStepSnapshot[] stepSnapshots;

        public void Dispose()
        {
            
        }
    }
}