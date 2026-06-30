using NaughtyAttributes;
using UnityEngine;

namespace giorgiokalmund.Dora.Generation
{
    public abstract class GenerationPattern : ScriptableObject
    {
        protected bool ShouldClearOnGenerate = true;
        public abstract bool CanGenerate();
        [Button]
        public abstract void Generate();
        [Button]
        public abstract void Update();
        [Button]
        public abstract void Clear();
    }
}