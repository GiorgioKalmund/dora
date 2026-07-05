namespace giorgiokalmund.Dora.Generation
{
    public interface IPatternGenerator
    {
        public GenerationPattern GetCurrentPattern();
        public void GenerateSolution() => TryGenerate(GetCurrentPattern());
        virtual void TryGenerate(GenerationPattern pattern)
        {
            if (!pattern)
            {
                DoraLogger.LogWarning("Please provide a pattern to generate.");
                return;
            }
            pattern.Generate();
        }
    }
}