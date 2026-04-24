using giorgiokalmund.Dora.Requirements;
using UnityEditor;
using UnityEngine;

namespace giorgiokalmund.Dora.Editor
{
    [CustomEditor(typeof(LocationRequirement), true)]
    public class LocationRequirementEditor : UnityEditor.Editor
    {
        private string latestString = null;
        private string latestName = null;
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            LocationRequirement loc = (LocationRequirement)target;

            if (!loc.forLocation.Equals(latestString))
            {
                latestString = loc.forLocation;
                latestName = loc.SpaceFoundation?.TryGetAnchor(latestString)?.gameObject.name;
            }
            
            if (!string.IsNullOrEmpty(latestName))
                GUILayout.Label($"Connected to Location: {latestName}", EditorStyles.boldLabel);
            else 
                GUILayout.Label($"No location connected.", EditorStyles.boldLabel);
        }
    }
}