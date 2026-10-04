using System;
using Puzzle.Core.Level;
using Puzzle.Core.Logging;
using Puzzle.Core.Services;

namespace Puzzle.Core.GameFlow.States
{
    /// <summary>
    /// Result state entered upon level completion or failure.
    /// Saves progress, dispatches analytics, and provides explicit user actions
    /// (Next Level, Retry, Return Home) that safely clean up the level lifetime scope.
    /// </summary>
    public class ResultState : BaseState
    {
        public override GameStateId Id => GameStateId.Result;

        private readonly IGameStateMachine _stateMachine;
        private readonly ILevelLoader _levelLoader;
        private readonly ISaveService _saveService;
        private readonly IAnalyticsService _analyticsService;
        private readonly Action _onResultHook;

        public LevelResultData ResultData { get; private set; }

        public ResultState(
            IGameStateMachine stateMachine,
            ILevelLoader levelLoader,
            ISaveService saveService = null,
            IAnalyticsService analyticsService = null)
            : this(stateMachine, levelLoader, saveService, analyticsService, null) { }

        public ResultState(
            IGameStateMachine stateMachine,
            ILevelLoader levelLoader,
            ISaveService saveService,
            IAnalyticsService analyticsService,
            Action onResultHook)
        {
            _stateMachine = stateMachine;
            _levelLoader = levelLoader;
            _saveService = saveService;
            _analyticsService = analyticsService;
            _onResultHook = onResultHook;
        }

        public ResultState(Action onResultHook) 
            : this(null, null, null, null, onResultHook) { }

        public void SetResult(LevelResultData resultData)
        {
            ResultData = resultData;
        }

        public override void Enter()
        {
            _onResultHook?.Invoke();

            if (ResultData == null)
            {
                CoreLogger.LogWarning("[GameFlow] Result: Entered without result data.");
                return;
            }

            var outcome = ResultData.IsSuccess ? "Win" : "Lose";
            CoreLogger.Log($"[GameFlow] Result {outcome}: Level={ResultData.LevelId}, Score={ResultData.Score}, Stars={ResultData.Stars}");

            if (ResultData.IsSuccess && _saveService != null)
            {
                _saveService.SetLevelCompleted(ResultData.LevelId, ResultData.Stars, ResultData.Score);
                _saveService.Save();
            }

            _analyticsService?.LogLevelCompleted(ResultData);
        }

        /// <summary>
        /// Player chose to proceed to the next level.
        /// Unloads the completed level scope and begins loading the next level.
        /// </summary>
        public void NextLevel(LevelId nextLevelId)
        {
            if (nextLevelId.IsEmpty)
            {
                throw new ArgumentException("Next LevelId cannot be empty.", nameof(nextLevelId));
            }

            // Clean up previous level lifetime scope
            UnloadCurrentLevel();

            if (_stateMachine != null)
            {
                var loadingState = _stateMachine.GetState<LevelLoadingState>(GameStateId.LevelLoading);
                if (loadingState != null)
                {
                    loadingState.SetTargetLevel(nextLevelId);
                }

                _stateMachine.ChangeState(GameStateId.LevelLoading);
            }
        }

        /// <summary>
        /// Player chose to replay the current level.
        /// Unloads the current session and re-initiates level loading.
        /// </summary>
        public void Retry()
        {
            var levelIdToRetry = ResultData?.LevelId ?? LevelId.Empty;
            if (levelIdToRetry.IsEmpty)
            {
                ReturnHome();
                return;
            }

            // Clean up previous level lifetime scope
            UnloadCurrentLevel();

            if (_stateMachine != null)
            {
                var loadingState = _stateMachine.GetState<LevelLoadingState>(GameStateId.LevelLoading);
                if (loadingState != null)
                {
                    loadingState.SetTargetLevel(levelIdToRetry);
                }

                _stateMachine.ChangeState(GameStateId.LevelLoading);
            }
        }

        /// <summary>
        /// Player chose to exit to the main menu.
        /// Cleans up the level lifetime scope and transitions to HomeState.
        /// </summary>
        public void ReturnHome()
        {
            UnloadCurrentLevel();

            _stateMachine?.ChangeState(GameStateId.Home);
        }

        private void UnloadCurrentLevel()
        {
            if (_levelLoader != null && ResultData != null && !ResultData.LevelId.IsEmpty)
            {
                _levelLoader.UnloadLevel(ResultData.LevelId);
            }
        }
    }
}
