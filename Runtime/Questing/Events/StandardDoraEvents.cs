using SpaceFoundationSystem;

namespace giorgiokalmund.Dora.Questing.Events
{
    public record EnteredLocationEvent(Anchor Location) : IGameplayEvent;
}