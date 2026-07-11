using SpaceFoundationSystem;
using UCGUI;
using UnityEngine.Assertions;

namespace giorgiokalmund.Dora
{
    public class PlayerLocationText : LabelComponent, IHUDComponent
    {
        protected override void Awake()
        {
            base.Awake();
            Color(UnityEngine.Color.gray1, 0.7f);
            text.Text(" ").Color(UnityEngine.Color.white);
            this.PaddingAdd(PaddingSide.Horizontal, 10);
            DisplayName = "PlayerLocation";
        }

        private void HandleLocationChanged(Anchor newLocation)
        {
            Assert.IsNotNull(newLocation, "??");
            //Text($"{newLocation.name} ({newLocation.GetUniqueId()})");
            Text($"{newLocation.name}");
        }

        public void Init(LocationMember mainMember)
        {
            mainMember.onLocationChanged.AddListener(HandleLocationChanged);
        }
        
        public void Deinit(LocationMember mainMember)
        {
            mainMember.onLocationChanged.RemoveListener(HandleLocationChanged);
        }
    }
}
