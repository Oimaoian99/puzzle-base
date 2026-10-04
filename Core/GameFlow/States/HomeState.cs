using System;
using Puzzle.Core.Level;
using Puzzle.Core.Logging;

namespace Puzzle.Core.GameFlow.States
{
    /// <summary>
    /// Main menu state. Waits for player interaction (e.g., selecting a level or pressing Play).
    /// Does not auto-transition.
    /// </summary>
    public class HomeState : BaseState
    {
        public override GameStateId Id => GameStateId.Home;

        private readonly IGameStateMachine _stateMachine;
        private readonly Action _onEnter;

        public HomeState(IGameStateMachine stateMachine) 
            : this(stateMachine, null) { }

        public HomeState(IGameStateMachine stateMachine, Action onEnter)
        {
            _stateMachine = stateMachine;
            _onEnter = onEnter;
        }

        public HomeState(Action onEnter) : this(null, onEnter) { }

        public override void Enter()
        {
            CoreLogger.Log("[GameFlow] Home: In Home menu, awaiting player action.");
            _onEnter?.Invoke();
        }

        /// <summary>
        /// Explicit trigger invoked when the player selects and starts a level from Home UI.
        /// </summary>
        public void PlayLevel(LevelId levelId)
        {
            if (levelId.IsEmpty)
            {
                throw new ArgumentException("LevelId cannot be empty.", nameof(levelId));
            }

            if (_stateMachine == null)
            {
                throw new InvalidOperationException("[HomeState] StateMachine reference is not set.");
            }

            var loadingState = _stateMachine.GetState<LevelLoadingState>(GameStateId.LevelLoading);
            if (loadingState != null)
            {
                loadingState.SetTargetLevel(levelId);
            }

            _stateMachine.ChangeState(GameStateId.LevelLoading);
        }
    }
}
