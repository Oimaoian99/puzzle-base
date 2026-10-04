using System;
using Puzzle.Core.Logging;
using Puzzle.Core.Services;

namespace Puzzle.Core.GameFlow.States
{
    /// <summary>
    /// Initialization state that loads persistent user data and readies core application services.
    /// Once initialization is complete, transitions to HomeState.
    /// </summary>
    public class InitState : BaseState
    {
        public override GameStateId Id => GameStateId.Init;

        private readonly IGameStateMachine _stateMachine;
        private readonly ISaveService _saveService;
        private readonly Action _onInit;
        private readonly bool _autoTransition;

        public InitState(IGameStateMachine stateMachine, ISaveService saveService = null)
            : this(stateMachine, saveService, null, true) { }

        public InitState(
            IGameStateMachine stateMachine,
            ISaveService saveService,
            Action onInit,
            bool autoTransition = true)
        {
            _stateMachine = stateMachine;
            _saveService = saveService;
            _onInit = onInit;
            _autoTransition = autoTransition;
        }

        public InitState(Action onInit) : this(null, null, onInit, false) { }

        public override void Enter()
        {
            CoreLogger.Log("[GameFlow] Init: Initializing core services and user profile.");
            
            // Load user profile & saved progress if available
            _saveService?.Load();
            _onInit?.Invoke();

            if (_autoTransition && _stateMachine != null)
            {
                _stateMachine.ChangeState(GameStateId.Home);
            }
        }
    }
}
