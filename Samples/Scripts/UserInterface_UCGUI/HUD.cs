using System.Collections.Generic;
using UCGUI;
using UnityEngine;

namespace giorgiokalmund.Dora
{
    public interface IHUDComponent
    {
        public void Init(LocationMember mainMember);
        public void Deinit(LocationMember mainMember);
    }

    public class HUD : SimpleScreen
    {
        public static HUD Current { get; private set; }
        private LocationMember _mainMember;
        private readonly List<IHUDComponent> _components = new ();

        [HideInInspector] public PlayerLocationText playerLocation;
        [HideInInspector] public SceneText sceneText;
        [HideInInspector] public QuestCardStack questCardStack;
        
        public override void Create()
        {
            Current = this;
            
            playerLocation = UI.N<PlayerLocationText>().Pivot(PivotPosition.UpperCenter, true).Parent(this)
                .OffsetY(-20);
            
            sceneText = UI.N<SceneText>().Pivot(PivotPosition.UpperRight, true).Parent(this)
                .Offset(-20, -20);
            
            questCardStack = UI.N<QuestCardStack>().Pivot(PivotPosition.UpperLeft, true).Parent(this)
                .Offset(20, -20);
            
            _components.AddRange(new List<IHUDComponent>{playerLocation, sceneText});
        }

        public override void Initialize() { }

        public void Init(LocationMember mainMember)
        {
            _mainMember = mainMember;
            foreach (var hudComponent in _components)
                hudComponent.Init(_mainMember);
        }

        public void Deinit(LocationMember mainMember)
        {
            if (mainMember != _mainMember)
            {
                Debug.LogError($"Trying to deinitialize HUD with different location member ('{mainMember.gameObject.name}') than what it was initialized with ('{_mainMember.gameObject.name}')");
                return;
            }
            
            
            foreach (var hudComponent in _components)
                hudComponent.Deinit(_mainMember);
            _mainMember = null;
        }

        public override Canvas GetCanvas() => GetComponentInParent<Canvas>();
    }
}
