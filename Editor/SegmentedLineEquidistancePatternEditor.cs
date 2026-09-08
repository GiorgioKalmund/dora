using giorgiokalmund.Dora.Generation.StdLib;
using giorgiokalmund.Dora.Generation.StdLib.Line;
using UnityEditor;
using UnityEngine;

namespace giorgiokalmund.Dora.Editor
{
    [CustomEditor(typeof(SegmentedLineEquidistancePattern), true)]
    public class SegmentedLineEquidistancePatternEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            SegmentedLineEquidistancePattern equidistancePattern = (SegmentedLineEquidistancePattern)target;
            
            GUILayout.Space(20);
            GUILayout.Label("SCENE HANDLES", EditorStyles.boldLabel);
            
            
            equidistancePattern.LatestHandle1 = (Transform)EditorGUILayout.ObjectField("Point 1", equidistancePattern.LatestHandle1, typeof(Transform), true);
            equidistancePattern.LatestHandle2 = (Transform)EditorGUILayout.ObjectField("Point 2", equidistancePattern.LatestHandle2, typeof(Transform), true);
            if (!equidistancePattern.LatestHandle1 || !equidistancePattern.LatestHandle2)
            {
                GUILayout.Label("<i>To modify the line ends, drag transforms into the fields to edit them quickly.</i>", QuestDrawer.RichText);
                return;
            }
            if (equidistancePattern.LatestHandle1)
                equidistancePattern.SetPos1(equidistancePattern.LatestHandle1.transform.position);
            if (equidistancePattern.LatestHandle2)
                equidistancePattern.SetPos2(equidistancePattern.LatestHandle2.transform.position);
            if (!equidistancePattern.CanGenerate())
            {
                GUILayout.Label("Pos1 and Pos2 cannot be the same!", QuestDrawer.WarningText);
                return;
            }

            if (GUILayout.Button("Generate"))
            {
                equidistancePattern.Generate();
            }
            
            if (GUILayout.Button("Update"))
            {
                equidistancePattern.Update();
            }
            
            if (GUILayout.Button("Clear"))
            {
                equidistancePattern.Clear();
            }
        }
    }
}