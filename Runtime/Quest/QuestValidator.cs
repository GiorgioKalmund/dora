using System.Collections.Generic;
using JetBrains.Annotations;

namespace giorgiokalmund.Dora
{
    /// <summary>
    /// Cheks and verified the viability of quests. This is done via <see cref="QuestConstraint"/>s.
    /// </summary>
    public static class QuestValidator
    {
        [CanBeNull]
        public static string Validate(Quest quest)
        {
            var result = quest.Validate();
            if (result is QuestValidationFailure failure)
                return LogFailure(quest, failure);
            return null;
        }
        
        [CanBeNull]
        public static string ValidateQuestSteps(Quest quest)
        {
            var result = quest.ValidateQuestSteps();
            if (result is QuestValidationFailure failure)
                return LogFailure(quest, failure, "steps");
            return null;
        }
        
        public static string[] ValidateAllQuestSteps(Quest quest)
        {
            var results = quest.ValidateAllQuestSteps();
            List<string> failureMessages = new List<string>(results.Length);
            
            foreach (var result in results)
            {
                if (result is QuestValidationFailure failure)
                    failureMessages.Add(LogFailure(quest, failure, "step"));
            }

            return failureMessages.ToArray();
        }

        private static string LogFailure(Quest quest, QuestValidationFailure failure, string context = null)
        {
            string failureString = $"Quest '{quest.Information.Title}' ({quest.Information.Id}) {context}{(context != null ? " " : "")}cannot be validated: {failure.Reason}";
            QuestLogger.LogError(failureString);
            return failureString;
        }

    }
}