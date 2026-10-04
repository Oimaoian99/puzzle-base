using Puzzle.Core.GameFlow;
using UnityEngine;
using UnityEngine.UI;

namespace Puzzle.Presentation.UI
{
    /// <summary>
    /// Modal pause dialog. Offers options to resume gameplay, restart current level, or return home.
    /// Interacts strictly with the GameStateMachine.
    /// </summary>
    public class PausePopupView : UIPopup
    {
        [Header("Buttons")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button homeButton;

        private IGameStateMachine _fsm;

        protected override void Awake()
        {
            base.Awake();

            if (resumeButton != null)
                resumeButton.onClick.AddListener(OnResumeClicked);

            if (restartButton != null)
                restartButton.onClick.AddListener(OnRestartClicked);

            if (homeButton != null)
                homeButton.onClick.AddListener(OnHomeClicked);
        }

        public void Bind(IGameStateMachine fsm)
        {
            _fsm = fsm;
        }

        public void OnResumeClicked()
        {
            Hide();
            if (_fsm != null)
            {
                _fsm.ChangeState(GameStateId.Play);
            }
        }

        public void OnRestartClicked()
        {
            Hide();
            if (_fsm != null)
            {
                _fsm.ChangeState(GameStateId.LevelLoading);
            }
        }

        public void OnHomeClicked()
        {
            Hide();
            if (_fsm != null)
            {
                _fsm.ChangeState(GameStateId.Home);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (resumeButton != null)
                resumeButton.onClick.RemoveListener(OnResumeClicked);

            if (restartButton != null)
                restartButton.onClick.RemoveListener(OnRestartClicked);

            if (homeButton != null)
                homeButton.onClick.RemoveListener(OnHomeClicked);
        }
    }
}
