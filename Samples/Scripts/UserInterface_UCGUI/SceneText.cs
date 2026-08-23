using UCGUI;
using UnityEngine.SceneManagement;

namespace giorgiokalmund.Dora.Samples.Scripts.UserInterface_UCGUI
{
    public class SceneText : LabelComponent, IHUDComponent
    {
        protected override void Awake()
        {
            base.Awake();
            Color(UnityEngine.Color.gray1, 0.7f);
            text.Text(" ").Color(UnityEngine.Color.white);
            this.PaddingAdd(PaddingSide.Horizontal, 10);
            DisplayName = "SceneText";
            text.FontSize(24);
        }

        private void SetScene()
        {
            Text(SceneManager.GetActiveScene().name);
        }

        public void Init(LocationMember mainMember)
        {
            SetScene();
        }
        
        public void Deinit(LocationMember mainMember)
        {
            
        }
    }
}
