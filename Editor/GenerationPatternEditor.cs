using giorgiokalmund.Dora.Generation;
using UnityEditor;

namespace giorgiokalmund.Dora.Editor
{
    [CustomEditor(typeof(GenerationPattern), true)]
    public class GenerationPatternEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GenerationPattern pattern = (GenerationPattern)target;
            
            QuestDrawer.DrawGenerationPattern(pattern);
        }
    }
}