using giorgiokalmund.Dora.Saving;

namespace giorgiokalmund.Dora.Questing
{
    public abstract class QuestStep : QuestStep<QuestStep.State>
    {
        public struct State : ISerializableData<State>
        {
            public void Dispose()
            {
                
            }

            public override string ToString()
            {
                return "<EMPTY STATE>";
            }

            public bool Equals(State other) => true;

            public override bool Equals(object obj)
            {
                return obj is State other && Equals(other);
            }

            public override int GetHashCode() => 0;
        }

        protected override State GetInitialState()
        {
            return new State()
            {

            };
        }
    }
}