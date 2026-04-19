using UnityEditor;

namespace giorgiokalmund.Dora.Editor
{
    [CustomEditor(typeof(QuestStep))]
    public class QuestStepEditor : UnityEditor.Editor
    {
        private bool _showDebug = true;
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            QuestStep step = (QuestStep)target;

            QuestDrawer.DrawQuestStep(step, ref _showDebug);
        }
    }
}