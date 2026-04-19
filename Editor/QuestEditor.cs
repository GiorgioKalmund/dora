using System.Collections.Generic;
using giorgiokalmund.Dora.Utils;
using UnityEditor;
using UnityEngine;

namespace giorgiokalmund.Dora.Editor
{
    [CustomEditor(typeof(Quest), true)]
    public class QuestEditor : UnityEditor.Editor
    {
        protected string[] InternalErrorMessages;
        protected List<string> ValidationErrorMessages = new List<string>();
        
        private bool _foldOutStateQuickswap;
        private static readonly Color defaultColor = Color.gray8;
        private static readonly Color disabledColor = Color.gray5;
        public override void OnInspectorGUI()
        {
            Quest quest = (Quest)target;
            DrawDefaultInspector();

            
            InternalErrorMessages = quest.GetInternalValidationResult();

            if (InternalErrorMessages?.Length == 0)
            {
                _foldOutStateQuickswap = EditorGUILayout.Foldout(_foldOutStateQuickswap, "State Override");
                if (_foldOutStateQuickswap)
                {
                    GUILayout.BeginHorizontal();
                    foreach (var questState in EnumUtils.GetAll<QuestState>())
                    {
                        EditorGUI.BeginDisabledGroup(questState.Equals(quest.State));
                        if (GUILayout.Button($"{questState}"))
                        {
                            quest.State = questState;
                        }
                        EditorGUI.EndDisabledGroup();
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Try Advance State"))
                {
                    quest.TryAdvanceState();
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Validate All Steps"))
                {
                    ValidationErrorMessages.Clear();
                    var res = QuestValidator.ValidateAllQuestSteps(quest);
                    if (res != null)
                        ValidationErrorMessages.AddRange(res);
                }
                EditorGUI.BeginDisabledGroup(!quest.BaseRequirements);
                if (GUILayout.Button("Validate Base + Steps"))
                {
                    ValidationErrorMessages.Clear();
                    var res = QuestValidator.Validate(quest);
                    if (res != null)
                        ValidationErrorMessages.Add(res);
                }
                EditorGUI.EndDisabledGroup();
                GUILayout.EndHorizontal();

                if (quest.BaseRequirements)
                {
                    EditorGUILayout.Separator();
                    GUILayout.Label("<b>BASE REQUIREMENT</b>", QuestDrawer.RichText);
                    if (!quest.BaseRequirements.CanBeAchieved)
                        QuestDrawer.RichText.normal.textColor = disabledColor;
                    GUILayout.Label($"{quest.BaseRequirements.GetDescription()}", QuestDrawer.RichText);
                    QuestDrawer.RichText.normal.textColor = defaultColor;
                }
                
                if (quest.Steps?.Length > 0)
                {
                    EditorGUILayout.Separator();
                    GUILayout.Label("<b>STEP OVERVIEW</b>", QuestDrawer.RichText);
                    for (var i = 0; i < quest.Steps.Length; i++)
                    {
                        if (!quest.Steps[i].IsCompleted)
                            QuestDrawer.RichText.normal.textColor = disabledColor;
                        GUILayout.Label($"{i+1}) {quest.Steps[i].Requirements.GetDescription()}", QuestDrawer.RichText);
                        QuestDrawer.RichText.normal.textColor = defaultColor;
                    }
                }
            }
            else if (InternalErrorMessages?.Length > 0)
            {
                EditorGUILayout.Separator();
                GUILayout.Label("<b>INTERNAL ERRORS</b>", QuestDrawer.RichText);
                foreach (var currentErrorMessage in InternalErrorMessages)
                    GUILayout.Label(currentErrorMessage, QuestDrawer.ErrorText);
            }

            
            if (ValidationErrorMessages.Count > 0)
            {
                EditorGUILayout.Separator();
                GUILayout.Label("<b>VALIDATION ERRORS</b>", QuestDrawer.RichText);
                foreach (var currentErrorMessage in ValidationErrorMessages)
                    GUILayout.Label(currentErrorMessage, QuestDrawer.ErrorText);
                
                
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Clear Validation", GUILayout.ExpandWidth(false)))
                {
                    ValidationErrorMessages.Clear();
                }
                GUILayout.EndHorizontal();
            }
        }
    }
}