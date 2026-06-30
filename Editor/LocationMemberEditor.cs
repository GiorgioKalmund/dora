using UnityEditor;
using UnityEngine;

namespace giorgiokalmund.Dora.Editor
{
    [CustomEditor(typeof(LocationMember), true)]
    [CanEditMultipleObjects]
    public class LocationMemberEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Find Closest Anchor"))
            {
                foreach (var t in targets)
                {
                    LocationMember member = (LocationMember)t;
                    {
                        member.FindClosestAnchor();
                    }
                }
            }
        }
    }
}