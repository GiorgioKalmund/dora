using giorgiokalmund.Dora.Generation;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace giorgiokalmund.Dora.Editor
{
    public static class QuestDrawer
    {
        
        #region Styles
        private static GUIStyle _todoButton;
        public static GUIStyle TodoButton
        {
            get
            {
                if (_todoButton == null)
                {
                    _todoButton = new GUIStyle(EditorStyles.miniButton);
                    _todoButton.richText = true;
                    _todoButton.normal.textColor = Color.cyan;
                }
                return _todoButton;
            }
        }
        public static GUIStyle RichText = new GUIStyle(EditorStyles.boldLabel) { richText = true, wordWrap = true};
        
        private static GUIStyle _okText;
        public static GUIStyle OkText
        {
            get
            {
                if (_okText == null)
                {
                    _okText = new GUIStyle(RichText);
                    _okText.normal.textColor = Color.green;
                }
                return _okText;
            }
        }
        private static GUIStyle _warningText;
        public static GUIStyle WarningText
        {
            get
            {
                if (_warningText == null)
                {
                    _warningText = new GUIStyle(RichText);
                    _warningText.normal.textColor = Color.orange;
                }
                return _warningText;
            }
        }
        private static GUIStyle _errorText;
        public static GUIStyle ErrorText
        {
            get
            {
                if (_errorText == null)
                {
                    _errorText = new GUIStyle(RichText);
                    _errorText.normal.textColor = Color.red;
                }
                return _errorText;
            }
        }
        #endregion
        
        public static void DrawGenerationTester([CanBeNull] GenerationPattern patternToGenerate)
        {
            if (!patternToGenerate)
                return;
            
            DrawGenerationPattern(patternToGenerate);
        }

        public static void DrawGenerationPattern(GenerationPattern patternToGenerate)
        {
            if (GUILayout.Button("Generate"))
                patternToGenerate.Generate();
            if (GUILayout.Button("Update"))
                patternToGenerate.Update();
            if ( GUILayout.Button("Clear"))
                patternToGenerate.Clear();
        }

        public static void DrawIPatternGenerator(IPatternGenerator generator)
        {
            if (GUILayout.Button("Generate Solution"))
            {
                generator.GenerateSolution();
            }
        }
    }
}