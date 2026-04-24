using UnityEngine;

namespace giorgiokalmund.Dora.Generation
{
    public abstract class GenerationPattern : ScriptableObject
    {
        protected bool ShouldClearOnGenerate = true;
        public abstract bool CanGenerate();
        public abstract void Generate();
        public abstract void Update();
        public abstract void Clear();
    }
}