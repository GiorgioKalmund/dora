using giorgiokalmund.Dora.Samples.Scripts.UserInterface_UCGUI;

namespace giorgiokalmund.Dora.Samples.Scripts
{
    public class DoraPlayer : LocationMember
    {
        private void Start()
        {
            HUD.Current.Init(this);
            DoraPlayerManager.Current.RegisterMainActor(this);
        }

        private void OnDestroy()
        {
            HUD.Current.Deinit(this);
            DoraPlayerManager.Current.UnregisterMainActor(this);
        }
    }
}