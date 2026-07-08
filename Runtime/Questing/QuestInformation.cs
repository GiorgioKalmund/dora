using System;
using UnityEngine;

namespace giorgiokalmund.Dora.Questing
{
    [Serializable]
    public class QuestInformation : IEquatable<QuestInformation>, IComparable<QuestInformation>
    {
        [field: SerializeField, Tooltip("Shorthand / unique identifier for a Quest.")]
        public string identifier;
        
        [field: SerializeField, Tooltip("Representative title for a Quest")]
        public string Title { get; internal set; }

        [field: SerializeField, Tooltip("Representative description for a Quest")]
        public string Description { get; internal set; }

        public override string ToString()
        {
            return $"Quest '{Title}'";
        }

        public bool Equals(QuestInformation other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return identifier == other.identifier && Title == other.Title && Description == other.Description;
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((QuestInformation)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(identifier, Title, Description);
        }

        public int CompareTo(QuestInformation other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (other is null) return 1;
            var identifierComparison = string.Compare(identifier, other.identifier, StringComparison.Ordinal);
            if (identifierComparison != 0) return identifierComparison;
            var titleComparison = string.Compare(Title, other.Title, StringComparison.Ordinal);
            if (titleComparison != 0) return titleComparison;
            return string.Compare(Description, other.Description, StringComparison.Ordinal);
        }
    }
}