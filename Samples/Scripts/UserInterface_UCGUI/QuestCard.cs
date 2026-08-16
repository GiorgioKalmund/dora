using giorgiokalmund.Dora.Questing;
using UCGUI;
using UnityEngine;

namespace giorgiokalmund.Dora
{
    public class QuestCard : VStackComponent
    {
        private TextComponent _state;
        private TextComponent _identifier;
        private TextComponent _title;
        private TextComponent _description;
        private TextComponent _currentStep;
        private TextComponent _questStepCount;

        private const int Width = 500;
        private const int FontSize = 32;
        private const int Padding = 20;

        private Quest _quest;

        protected override void Awake()
        {
            base.Awake();
            
            ChildAlignment(TextAnchor.MiddleLeft);


            _state = UI.Text("<color=red><STATE></color>").AutoSize(10, FontSize * 0.5f).Size(Width, FontSize * 0.5f).Parent(this).Bold();
            _identifier = UI.Text("<color=red><IDENTIFIER></color>").AutoSize(10, FontSize * 0.5f).Color(UnityEngine.Color.gray).Size(Width, FontSize * 0.5f).Parent(this);
            _title = UI.Text("<color=red><TITLE></color>").AutoSize(10, maxSize:FontSize).Color(UnityEngine.Color.white).Size(Width, FontSize).Parent(this);
            _description = UI.Text("<color=red><DESCRIPTION></color>").AutoSize(10, maxSize:FontSize * 0.5f).Color(UnityEngine.Color.gray).Size(Width, FontSize * 0.5f).Parent(this);
            _currentStep = UI.Text("<i>?Current Step?</i>").AutoSize(10, maxSize:FontSize * 0.75f).Color(UnityEngine.Color.gray8).Size(Width, FontSize * 0.75f).Parent(this);
            _questStepCount = UI.Text("<color=red><STEPS></color>").AutoSize(10, maxSize:FontSize).Color(UnityEngine.Color.white).Size(Width, FontSize).Parent(this);

            this.Padding(Padding);
            Color(UnityEngine.Color.gray1);
        }

        public QuestCard Init(Quest quest)
        {
            _quest = quest;
            _state.Text(_quest.State.ToString()).Color(_quest.State.GetColor());
            _identifier.Text(_quest.Information.identifier);
            _title.Text(_quest.Information.Title);
            _description.Text(_quest.Information.Description);
            _currentStep.Text(_quest.CurrentStep?.ToString());
            _questStepCount.Text($"{_quest.CurrentStepIdx}/{_quest.Steps.Length}");

            _quest.onStepStarted.AddListener(HandleNewStep);
            _quest.onStepUpdated.AddListener(HandleStepUpdated);
            _quest.onUpdate.AddListener(UpdateStepCount);
            _quest.onStateChanged.AddListener(UpdateState);
            _quest.onBotch.AddListener(HandleBotch);
            
            if (_quest.CurrentStep)
                HandleStepUpdated(_quest.CurrentStep);
            return this;
        }

        private void HandleStepUpdated(AbstractQuestStep step)
        {
            _currentStep.Text(step.GetDescription());
        }

        private void HandleNewStep(AbstractQuestStep step) => HandleStepUpdated(step);

        private void UpdateStepCount(Quest quest)
        {
            _questStepCount.Text($"{quest.CurrentStepIdx}/{quest.Steps.Length}");
        }

        private void HandleBotch(Quest quest)
        {
            _state.Text("<color=red>!!!BOTCHED!!!</color>");
        }

        public void UpdateState(QuestState state)
        {
            _state
                .Text(state.ToString())
                .Color(state.GetColor());
            if (state <= QuestState.MENTIONED)
            {
                UpdateStepCount(_quest);
                _currentStep.Text("<i>?Current Step</i>");
            }
            if (state >= QuestState.ACHIEVED)
                _currentStep.Text("<i>All Steps Completed</i>");
        }
    }
}
