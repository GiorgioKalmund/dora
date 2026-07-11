namespace giorgiokalmund.Dora
{
    public class DoraPlayer : LocationMember
    {
        private void Start()
        {
            HUD.Current.Init(this);
            QuestManager.Current.RegisterMainActor(this);
        }

        private void OnDestroy()
        {
            HUD.Current.Deinit(this);
            QuestManager.Current.UnregisterMainActor(this);
        }
    }
}