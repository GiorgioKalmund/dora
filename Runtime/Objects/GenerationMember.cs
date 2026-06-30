using System.Linq;
using JetBrains.Annotations;
using NaughtyAttributes;
using UnityEngine;

namespace giorgiokalmund.Dora
{
    public class GenerationMember : MonoBehaviour
    {
        /// <summary>
        /// The next available id for a member.
        /// </summary>
        public static int NextId { get; protected set; }
        
        [field: ReadOnly]
        [field: SerializeField, Tooltip("Unique identifier for each GenerationMember")]
        public string Id { get; protected set; }
        
        [field: SerializeField, Tooltip("Whether this member should NOT be re-generated during a generation step and instead maintain its original position.")]
        public bool IsFixed { get; protected set; }

        private void OnEnable()
        {
            // reassign all existing Ids and properly reflect the new next id in NextId
            if (int.TryParse(Id.Substring(1), out int result))
            {
                if (result > NextId)
                    NextId = result;
            }
        }

        GenerationMember()
        {
            Id = "g" + GetNextID();
        }
        
        /// <summary>
        /// Returns the next available id.
        /// </summary>
        protected int GetNextID()
        {
            return NextId++;
        }

        /// <summary>
        /// Attempts to find a <see cref="GenerationMember"/> with the given id. If none is found, null is returned.
        /// </summary>
        /// <param name="genId">The <see cref="Id"/> of the member to find.</param>
        [CanBeNull]
        public static GenerationMember Find(string genId)
        {
            return FindObjectsByType<GenerationMember>().FirstOrDefault(m => m.Id.Equals(genId));
        }
    }
}