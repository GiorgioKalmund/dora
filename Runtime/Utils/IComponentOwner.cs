namespace giorgiokalmund.Dora.Utils
{
    public interface IComponentOwner
    {
        public void AddComponent<T>(BaseComponent<T> component) where T : IComponentOwner;
    }
}