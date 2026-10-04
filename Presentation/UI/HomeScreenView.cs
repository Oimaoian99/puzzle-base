using Puzzle.Core.GameFlow;
using UnityEngine;
using UnityEngine.UI;

namespace Puzzle.Presentation.UI
{
    /// <summary>
    /// Main menu screen displaying primary play entry point and game navigation.
    /// Interacts strictly with GameStateMachine to advance to LevelLoadingState.
    /// </summary>
    public class HomeScreenView : UIWindow
    {
        [Header("Menu Buttons")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button settingsButton;

        private IGameStateMachine _fsm;

        protected override void Awake()
        {
            base.Awake();

            if (playButton != null)
                playButton.onClick.AddListener(OnPlayClicked);
        }

        public void Bind(IGameStateMachine fsm)
        {
            _fsm = fsm;
        }

        public void OnPlayClicked()
        {
            if (_fsm != null)
            {
                _fsm.ChangeState(GameStateId.LevelLoading);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (playButton != null)
                playButton.onClick.RemoveListener(OnPlayClicked);
        }
    }
}
