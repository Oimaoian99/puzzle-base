using Puzzle.Core.GameFlow;
using Puzzle.Core.Level;
using UnityEngine;
using UnityEngine.UI;

namespace Puzzle.Presentation.UI
{
    /// <summary>
    /// Displays in-game HUD indicators (remaining moves, score, target score progress)
    /// and dispatches the pause request to the GameStateMachine.
    /// Purely observational: does not modify score or moves itself.
    /// </summary>
    public class GameplayHUDView : UIWindow
    {
        [Header("UI Elements")]
        [SerializeField] private Text movesText;
        [SerializeField] private Text scoreText;
        [SerializeField] private Text targetScoreText;
        [SerializeField] private Slider scoreProgressBar;
        [SerializeField] private Button pauseButton;

        private ILevelRuntime _runtime;
        private IGameStateMachine _fsm;
        private int _targetScore;

        public ILevelRuntime Runtime => _runtime;

        protected override void Awake()
        {
            base.Awake();

            if (pauseButton != null)
            {
                pauseButton.onClick.AddListener(OnPauseClicked);
            }
        }

        public void Bind(ILevelRuntime runtime, IGameStateMachine fsm, int targetScore = 0)
        {
            Unbind();

            _runtime = runtime;
            _fsm = fsm;
            _targetScore = targetScore;

            if (_runtime != null)
            {
                _runtime.OnMovesChanged += HandleMovesChanged;
                _runtime.OnScoreChanged += HandleScoreChanged;

                UpdateMovesDisplay(_runtime.RemainingMoves);
                UpdateScoreDisplay(_runtime.CurrentScore);

                if (targetScoreText != null && _targetScore > 0)
                {
                    targetScoreText.text = $"/ {_targetScore}";
                }
            }
        }

        public void Unbind()
        {
            if (_runtime != null)
            {
                _runtime.OnMovesChanged -= HandleMovesChanged;
                _runtime.OnScoreChanged -= HandleScoreChanged;
                _runtime = null;
            }
            _fsm = null;
        }

        private void HandleMovesChanged(int remainingMoves)
        {
            UpdateMovesDisplay(remainingMoves);
        }

        private void HandleScoreChanged(int newScore)
        {
            UpdateScoreDisplay(newScore);
        }

        private void UpdateMovesDisplay(int moves)
        {
            if (movesText != null)
            {
                movesText.text = moves.ToString();
            }
        }

        private void UpdateScoreDisplay(int score)
        {
            if (scoreText != null)
            {
                scoreText.text = score.ToString();
            }

            if (scoreProgressBar != null && _targetScore > 0)
            {
                scoreProgressBar.value = Mathf.Clamp01((float)score / _targetScore);
            }
        }

        private void OnPauseClicked()
        {
            if (_fsm != null)
            {
                _fsm.ChangeState(GameStateId.Paused);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (pauseButton != null)
            {
                pauseButton.onClick.RemoveListener(OnPauseClicked);
            }

            Unbind();
        }
    }
}
