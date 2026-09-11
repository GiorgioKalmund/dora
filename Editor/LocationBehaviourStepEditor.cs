using giorgiokalmund.Dora.Steps.StdLib;
using UnityEditor;
using UnityEngine;

namespace giorgiokalmund.Dora.Editor
{
    [CustomEditor(typeof(LocationBehaviourStep))]
    public class LocationBehaviourStepEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            LocationBehaviourStep behaviourStep = (LocationBehaviourStep)target;
            
            if (GUILayout.Button("Generate Solution"))
            {
                behaviourStep.Generate();
            }
        }
    }
}