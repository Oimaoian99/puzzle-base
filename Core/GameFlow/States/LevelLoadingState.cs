using System;
using Puzzle.Core.Level;
using Puzzle.Core.Logging;

namespace Puzzle.Core.GameFlow.States
{
    /// <summary>
    /// Level loading state. Responsible for requesting the level runtime from ILevelLoader,
    /// configuring PlayState with the loaded runtime, and handling loading failures cleanly.
    /// </summary>
    public class LevelLoadingState : BaseState
    {
        public override GameStateId Id => GameStateId.LevelLoading;

        private readonly IGameStateMachine _stateMachine;
        private readonly ILevelLoader _levelLoader;
        private readonly Action _onLoadHook;

        public LevelId TargetLevelId { get; private set; }
        public ILevelRuntime LoadedRuntime { get; private set; }
        public string LastErrorMessage { get; private set; }
        public bool HasError => !string.IsNullOrEmpty(LastErrorMessage);

        public LevelLoadingState(IGameStateMachine stateMachine, ILevelLoader levelLoader)
            : this(stateMachine, levelLoader, null) { }

        public LevelLoadingState(
            IGameStateMachine stateMachine,
            ILevelLoader levelLoader,
            Action onLoadHook)
        {
            _stateMachine = stateMachine;
            _levelLoader = levelLoader;
            _onLoadHook = onLoadHook;
        }

        public LevelLoadingState(Action onLoadHook) : this(null, null, onLoadHook) { }

        public void SetTargetLevel(LevelId levelId)
        {
            TargetLevelId = levelId;
            LoadedRuntime = null;
            LastErrorMessage = null;
        }

        public override void Enter()
        {
            _onLoadHook?.Invoke();

            if (_levelLoader == null)
            {
                // Fallback / Standalone test mode without loader
                return;
            }

            if (TargetLevelId.IsEmpty)
            {
                CoreLogger.LogWarning("[GameFlow] LevelLoading: TargetLevelId is empty. Returning to Home.");
                _stateMachine?.ChangeState(GameStateId.Home);
                return;
            }

            CoreLogger.Log($"[GameFlow] Loading Level: {TargetLevelId}...");
            LastErrorMessage = null;

            _levelLoader.LoadLevel(
                TargetLevelId,
                onLoaded: OnLevelLoadedSuccessfully,
                onError: OnLevelLoadingFailed
            );
        }

        private void OnLevelLoadedSuccessfully(ILevelRuntime runtime)
        {
            LoadedRuntime = runtime;
            CoreLogger.Log($"[GameFlow] LevelLoaded: {runtime.Id} ready.");

            if (_stateMachine != null)
            {
                var playState = _stateMachine.GetState<PlayState>(GameStateId.Play);
                if (playState != null)
                {
                    playState.SetRuntime(runtime);
                }

                _stateMachine.ChangeState(GameStateId.Play);
            }
        }

        private void OnLevelLoadingFailed(string errorMessage)
        {
            LastErrorMessage = errorMessage;
            CoreLogger.LogError($"[GameFlow] LevelLoading Error: {errorMessage}");
            // State remains in LevelLoading so UI can present retry or cancel options
        }

        public void RetryLoading()
        {
            if (TargetLevelId.IsEmpty) return;
            Enter();
        }

        public void CancelToHome()
        {
            if (!TargetLevelId.IsEmpty && _levelLoader != null)
            {
                _levelLoader.UnloadLevel(TargetLevelId);
            }

            _stateMachine?.ChangeState(GameStateId.Home);
        }
    }
}
