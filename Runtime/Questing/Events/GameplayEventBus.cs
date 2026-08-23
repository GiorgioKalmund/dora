using UnityEngine.Events;

namespace giorgiokalmund.Dora.Questing.Events
{
    public class GameplayEventBus
    {
        public UnityEvent<IGameplayEvent> OnPublished = new UnityEvent<IGameplayEvent>();
        
        public void Publish(IGameplayEvent e)
        {
            OnPublished.Invoke(e);
        }
    }
}