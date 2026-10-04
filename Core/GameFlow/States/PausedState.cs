using System;
using Puzzle.Core.Level;
using Puzzle.Core.Logging;

namespace Puzzle.Core.GameFlow.States
{
    /// <summary>
    /// Paused state entered during active gameplay. Freezes game updates
    /// and allows the user to resume, retry, or exit to home.
    /// </summary>
    public class PausedState : BaseState
    {
        public override GameStateId Id => GameStateId.Paused;

        private readonly IGameStateMachine _stateMachine;
        private readonly ILevelLoader _levelLoader;
        private LevelId _activeLevelId;

        public PausedState(IGameStateMachine stateMachine, ILevelLoader levelLoader = null)
        {
            _stateMachine = stateMachine;
            _levelLoader = levelLoader;
        }

        public void SetActiveLevel(LevelId levelId)
        {
            _activeLevelId = levelId;
        }

        public override void Enter()
        {
            CoreLogger.Log("[GameFlow] Paused: Gameplay paused.");
        }

        public void Resume()
        {
            _stateMachine?.ChangeState(GameStateId.Play);
        }

        public void Retry()
        {
            if (!_activeLevelId.IsEmpty && _levelLoader != null)
            {
                _levelLoader.UnloadLevel(_activeLevelId);
            }

            if (_stateMachine != null)
            {
                var loadingState = _stateMachine.GetState<LevelLoadingState>(GameStateId.LevelLoading);
                if (loadingState != null)
                {
                    loadingState.SetTargetLevel(_activeLevelId);
                }

                _stateMachine.ChangeState(GameStateId.LevelLoading);
            }
        }

        public void ReturnHome()
        {
            if (!_activeLevelId.IsEmpty && _levelLoader != null)
            {
                _levelLoader.UnloadLevel(_activeLevelId);
            }

            _stateMachine?.ChangeState(GameStateId.Home);
        }
    }
}
