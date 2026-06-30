using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace giorgiokalmund.Dora.Editor
{
    [CustomEditor(typeof(QuestManager))]
    public class QuestManagerEditor : UnityEditor.Editor
    {
        private readonly List<string> _failedQuests = new List<string>();
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            QuestManager manager = (QuestManager)target;

            if (GUILayout.Button("Validate All"))
            {
                _failedQuests.Clear();
                foreach (var quest in manager.all)
                {
                    if (quest == null)
                        return;
                    var result = quest.Validate();
                    if (result.IsFailure)
                        _failedQuests.Add(quest.name);
                }

            }

            if (_failedQuests.Count == 0)
            {
                GUILayout.Label($"All {manager.all.Length} Quests OK", QuestDrawer.OkText);
            }
            else
            {
                StringBuilder resultBuilder = new StringBuilder();
                foreach (var failedQuest in _failedQuests)
                {
                    resultBuilder.Append("\t" + failedQuest + "\n");
                }
                GUILayout.Label($"Error: {_failedQuests.Count} Quests could not be validated!\n{resultBuilder}", QuestDrawer.ErrorText);
            }
        }
    }
}