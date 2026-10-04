using Puzzle.Core.GameFlow;
using Puzzle.Core.Level;
using UnityEngine;
using UnityEngine.UI;

namespace Puzzle.Presentation.UI
{
    /// <summary>
    /// Modal popup displayed upon level defeat (moves depleted without reaching target score).
    /// Offers retry and home navigation.
    /// </summary>
    public class LoseResultPopupView : UIPopup
    {
        [Header("Display")]
        [SerializeField] private Text finalScoreText;

        [Header("Buttons")]
        [SerializeField] private Button retryButton;
        [SerializeField] private Button homeButton;

        private IGameStateMachine _fsm;

        protected override void Awake()
        {
            base.Awake();

            if (retryButton != null)
                retryButton.onClick.AddListener(OnRetryClicked);

            if (homeButton != null)
                homeButton.onClick.AddListener(OnHomeClicked);
        }

        public void Bind(LevelResultData result, IGameStateMachine fsm)
        {
            _fsm = fsm;

            if (finalScoreText != null)
            {
                finalScoreText.text = result.Score.ToString();
            }

            Show();
        }

        public void OnRetryClicked()
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

            if (retryButton != null)
                retryButton.onClick.RemoveListener(OnRetryClicked);

            if (homeButton != null)
                homeButton.onClick.RemoveListener(OnHomeClicked);
        }
    }
}
