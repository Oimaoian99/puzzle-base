using Puzzle.Core.GameFlow;
using Puzzle.Core.Level;
using UnityEngine;
using UnityEngine.UI;

namespace Puzzle.Presentation.UI
{
    /// <summary>
    /// Modal popup displayed upon level victory.
    /// Shows final score, earned stars, and controls to advance or return home.
    /// </summary>
    public class WinResultPopupView : UIPopup
    {
        [Header("Score & Stars Display")]
        [SerializeField] private Text finalScoreText;
        [SerializeField] private GameObject[] starIcons = new GameObject[3];

        [Header("Buttons")]
        [SerializeField] private Button nextLevelButton;
        [SerializeField] private Button homeButton;

        private IGameStateMachine _fsm;

        protected override void Awake()
        {
            base.Awake();

            if (nextLevelButton != null)
                nextLevelButton.onClick.AddListener(OnNextLevelClicked);

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

            int earnedStars = Mathf.Clamp(result.Stars, 0, 3);
            for (int i = 0; i < starIcons.Length; i++)
            {
                if (starIcons[i] != null)
                {
                    starIcons[i].SetActive(i < earnedStars);
                }
            }

            Show();
        }

        public void OnNextLevelClicked()
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

            if (nextLevelButton != null)
                nextLevelButton.onClick.RemoveListener(OnNextLevelClicked);

            if (homeButton != null)
                homeButton.onClick.RemoveListener(OnHomeClicked);
        }
    }
}
