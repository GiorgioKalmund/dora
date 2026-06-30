namespace giorgiokalmund.Dora.Generation
{
    public interface IPatternGenerator
    {
        internal GenerationPattern GetCurrentPattern();
        public void GenerateSolution() => TryGenerate(GetCurrentPattern());
        virtual void TryGenerate(GenerationPattern pattern)
        {
            if (!pattern)
            {
                QuestLogger.LogWarning("Please provide a pattern to generate.");
                return;
            }
            pattern.Generate();
        }
    }
}