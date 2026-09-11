using System.Collections.Generic;
using giorgiokalmund.Dora.Questing;
using giorgiokalmund.Dora.Saving;
using giorgiokalmund.Dora.Saving.SerializationProviders;
using giorgiokalmund.Dora.Saving.StorageProviders;
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
        private bool validatedAtLeastOnce = false;
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
                    foreach (var questState in EnumUtils.GetAll<QuestPhase>())
                    {
                        EditorGUI.BeginDisabledGroup(questState.Equals(quest.Phase));
                        if (GUILayout.Button($"{questState}"))
                        {
                            quest.TrySetPhase(questState);
                        }
                        EditorGUI.EndDisabledGroup();
                    }
                    GUILayout.EndHorizontal();
                }
                
                
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Validate All Steps"))
                {
                    ValidationErrorMessages.Clear();
                    var res = QuestValidator.ValidateAllQuestSteps(quest, Application.isPlaying);
                    if (res != null)
                        ValidationErrorMessages.AddRange(res);
                    validatedAtLeastOnce = true;
                }
                if (GUILayout.Button("Validate"))
                {
                    ValidationErrorMessages.Clear();
                    var res = QuestValidator.Validate(quest, Application.isPlaying);
                    if (res != null)
                        ValidationErrorMessages.Add(res);
                    validatedAtLeastOnce = true;
                }
                GUILayout.EndHorizontal();
                
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("DEBUG: Reset"))
                {
                    quest.ResetQuest();
                }
                GUILayout.EndHorizontal();
                
                GUILayout.Space(10);
                GUILayout.Label("Debug Saving & Loading (JSON & FILE)", EditorStyles.boldLabel);
                GUILayout.BeginHorizontal();
                /*
                 * TODO: Remove in favor of more flexible solution where the user can define the
                 * serializer and storage method
                 */ 
                if (GUILayout.Button(">> Save"))
                {
                    quest.Save(new JsonSerializationProvider(), new JsonFileStorageProvider(Application.persistentDataPath));
                }
                if (GUILayout.Button("<< Load"))
                {
                    quest.Load(new JsonSerializationProvider(), new JsonFileStorageProvider(Application.persistentDataPath));
                }
                GUILayout.EndHorizontal();
                
                if (quest.Steps?.Length > 0)
                {
                    EditorGUILayout.Separator();
                    GUILayout.Label("<b>STEP OVERVIEW</b>", QuestDrawer.RichText);
                    for (var i = 0; i < quest.Steps.Length; i++)
                    {
                        if (quest.Steps[i] == null)
                            continue;
                        
                        if (!quest.Steps[i].IsCompleted)
                            QuestDrawer.RichText.normal.textColor = disabledColor;
                        GUILayout.Label($"{i+1})\t{quest.Steps[i].GetDescription() ?? "<color=red><NO REQUIREMENTS></color>"}", QuestDrawer.RichText);
                        QuestDrawer.RichText.normal.textColor = defaultColor;
                    }
                }
            }
            else if (InternalErrorMessages?.Length > 0)
            {
                EditorGUILayout.Separator();
                GUILayout.Label("<b>INTERNAL ERRORS</b>", QuestDrawer.RichText);
                foreach (var currentErrorMessage in InternalErrorMessages)
                    GUILayout.Label($"- {currentErrorMessage}", QuestDrawer.ErrorText);
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
            else  if (validatedAtLeastOnce)
            {
                GUILayout.Label("All Requirements Validated", QuestDrawer.OkText);
            }
        }
    }
}