using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace giorgiokalmund.Dora.Editor
{
    [CustomEditor(typeof(QuestManager))]
    public class QuestManagerEditor : UnityEditor.Editor
    {
        private List<string> failedQuests = new List<string>();
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            QuestManager manager = (QuestManager)target;


            if (GUILayout.Button("Donate 1"))
            {
                bool result = manager.TryUpdateQuest("TestInformation" , 16);
                Debug.Log("Update " + (result ? "successful" : "failed"));
            }

            if (GUILayout.Button("Validate All"))
            {
                failedQuests.Clear();
                foreach (var quest in manager.all)
                {
                    if (quest == null)
                        return;
                    var result = quest.Validate();
                    if (result.IsFailure)
                        failedQuests.Add(quest.name);
                }

            }

            if (failedQuests.Count == 0)
            {
                GUILayout.Label($"All {manager.all.Length} Quests OK", QuestDrawer.OkText);
            }
            else
            {
                StringBuilder resultBuilder = new StringBuilder();
                foreach (var failedQuest in failedQuests)
                {
                    resultBuilder.Append("\t" + failedQuest + "\n");
                }
                GUILayout.Label($"Error: {failedQuests.Count} Quests could not be validated!\n{resultBuilder}", QuestDrawer.ErrorText);
            }
        }
    }
}