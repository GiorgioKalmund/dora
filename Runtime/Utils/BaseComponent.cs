using JetBrains.Annotations;
using UnityEngine;

namespace giorgiokalmund.Dora.Utils
{
    public class BaseComponent<T> : ScriptableObject where T : IComponentOwner
    {
        /// <summary>
        /// RUNTIME-ONLY reference to the linked Manager
        /// </summary>
        [CanBeNull] public T Manager { get; protected set; }

        public void AddTo(T manager)
        {
            Manager = manager;
            manager.AddComponent(this);
        }
    }
}