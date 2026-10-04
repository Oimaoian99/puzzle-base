using System;
using Puzzle.Core.Logging;

namespace Puzzle.Core.GameFlow.States
{
    /// <summary>
    /// Initial application state entered upon game launch.
    /// Performs engine baseline startup and transitions to InitState.
    /// </summary>
    public class BootState : BaseState
    {
        public override GameStateId Id => GameStateId.Boot;

        private readonly IGameStateMachine _stateMachine;
        private readonly Action _onBoot;
        private readonly bool _autoTransition;

        public BootState(IGameStateMachine stateMachine) 
            : this(stateMachine, null, true) { }

        public BootState(IGameStateMachine stateMachine, Action onBoot, bool autoTransition = true)
        {
            _stateMachine = stateMachine;
            _onBoot = onBoot;
            _autoTransition = autoTransition;
        }

        public BootState(Action onBoot) 
            : this(null, onBoot, false) { }

        public override void Enter()
        {
            CoreLogger.Log("[GameFlow] Boot: Application launched. Preparing engine baseline.");
            _onBoot?.Invoke();

            if (_autoTransition && _stateMachine != null)
            {
                _stateMachine.ChangeState(GameStateId.Init);
            }
        }
    }
}
