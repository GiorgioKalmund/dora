using UnityEngine;

namespace giorgiokalmund.Dora
{
    public static class QuestLogger
    {
        private static readonly Color LoggingColor = Color.cyan;
        
        public static void Log(object log)
        {
            Debug.Log($"<color=#{ColorUtility.ToHtmlStringRGBA(LoggingColor)}>Dora:</color> " + log);
        }

        public static void LogWarning(string log)
        {
            Debug.LogWarning($"<color=#{ColorUtility.ToHtmlStringRGBA(LoggingColor)}>Dora Warning:</color> " + log);
        }
        
        
        public static void LogError(string log)
        {
            Debug.LogError($"<color=#{ColorUtility.ToHtmlStringRGBA(LoggingColor)}>Dora Error:</color> " + log);
        } 
    }
}