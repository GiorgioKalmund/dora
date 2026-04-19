using UnityEditor;
using UnityEngine;

namespace giorgiokalmund.Dora.Editor
{
    [CustomEditor(typeof(QuestManager))]
    public class QuestManagerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            QuestManager manager = (QuestManager)target;


            if (GUILayout.Button("Donate 1"))
            {
                bool result = manager.TryUpdateQuest("TestInformation" , 16);
                Debug.Log("Update " + (result ? "successful" : "failed"));
            }
        }
    }
}