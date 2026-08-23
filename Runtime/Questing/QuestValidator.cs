using System.Collections.Generic;
using JetBrains.Annotations;

namespace giorgiokalmund.Dora.Questing
{
    // TODO: @Refa
    /// <summary>
    /// Cheks and verified the viability of quests. This is done via <see cref="QuestConstraint"/>s.
    /// </summary>
    public static class QuestValidator
    {
        [CanBeNull]
        public static string Validate(Quest quest, bool isRuntime)
        {
            var result = quest.Validate(isRuntime);
            if (result is QuestValidationFailure failure)
                return LogFailure(quest, failure);
            return null;
        }
        
        [CanBeNull]
        public static string ValidateQuestSteps(Quest quest, bool isRuntime)
        {
            var result = quest.ValidateQuestSteps(isRuntime);
            if (result is QuestValidationFailure failure)
                return LogFailure(quest, failure, "steps");
            return null;
        }
        
        public static string[] ValidateAllQuestSteps(Quest quest, bool isRuntime)
        {
            var results = quest.ValidateAllQuestSteps(isRuntime);
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
            string failureString = $"Quest '{quest.Information.Title}' ({quest.Information.identifier}) {context}{(context != null ? " " : "")}cannot be validated: {failure.Reason}";
            DoraLogger.LogError(failureString);
            return failureString;
        }

    }
}