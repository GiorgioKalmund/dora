using UnityEditor;
using UnityEngine;

namespace giorgiokalmund.Dora.Editor
{
    [CustomEditor(typeof(LocationMember))]
    public class LocationMemberEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            LocationMember member = (LocationMember)target;
            if (GUILayout.Button("Find Closest Anchor"))
            {
                member.FindClosestAnchor();
            }
        }
    }
}