using giorgiokalmund.Dora.Generation.StdLib;
using UnityEditor;
using UnityEngine;

namespace giorgiokalmund.Dora.Editor
{
    [CustomEditor(typeof(LinePattern), true)]
    public class LinePatternEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            LinePattern pattern = (LinePattern)target;
            
            GUILayout.Space(20);
            GUILayout.Label("SCENE HANDLES", EditorStyles.boldLabel);
            
            
            pattern.latestHandle1 = (Transform)EditorGUILayout.ObjectField("Point 1", pattern.latestHandle1, typeof(Transform), true);
            pattern.latestHandle2 = (Transform)EditorGUILayout.ObjectField("Point 2", pattern.latestHandle2, typeof(Transform), true);
            if (!pattern.latestHandle1 || !pattern.latestHandle2)
            {
                GUILayout.Label("<i>To modify the line ends, drag transforms into the fields to quickly edit them.</i>", QuestDrawer.RichText);
            }
            if (pattern.latestHandle1)
                pattern.SetPos1(pattern.latestHandle1.transform.position);
            if (pattern.latestHandle2)
                pattern.SetPos2(pattern.latestHandle2.transform.position);
            if (!pattern.CanGenerate())
            {
                GUILayout.Label("Pos1 and Pos2 cannot be the same!", QuestDrawer.WarningText);
                return;
            }
        }
    }
}