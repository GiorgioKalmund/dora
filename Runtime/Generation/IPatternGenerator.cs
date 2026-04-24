namespace giorgiokalmund.Dora.Generation
{
    public interface IPatternGenerator
    {
        internal GenerationPattern GetCurrentPattern();
        protected void GenerateSolution() => TryGenerate(GetCurrentPattern());
        virtual void TryGenerate(GenerationPattern pattern)
        {
            if (!pattern)
                return;
            pattern.Generate();
        }
    }
}