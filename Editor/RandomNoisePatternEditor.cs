using giorgiokalmund.Dora.Generation.StdLib;
using UnityEditor;
using UnityEngine;

namespace giorgiokalmund.Dora.Editor
{
    [CustomEditor(typeof(RandomNoisePattern))]
    public class RandomNoisePatternEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            RandomNoisePattern randomNoise = (RandomNoisePattern)target;
            GUILayout.Label($"Usage Mode: {(randomNoise.templateBounds ? "BOUNDS" : "SPHERE")}", QuestDrawer.OkText);
            
            base.OnInspectorGUI();
        }
    }
}