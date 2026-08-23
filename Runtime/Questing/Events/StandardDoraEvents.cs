using SpaceFoundationSystem;

namespace giorgiokalmund.Dora.Questing.Events
{
    public record EnteredLocationEvent(Anchor Location) : IGameplayEvent;
    public record MemberEnteredLocationEvent(LocationMember Member, Anchor Location) : EnteredLocationEvent(Location);
}