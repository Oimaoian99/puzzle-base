using System;
using Puzzle.Core.Gameplay.Runtime;
using Puzzle.Core.Input;
using Puzzle.Core.Level;
using Puzzle.Core.Logging;
using Puzzle.Core.Services;

namespace Puzzle.Core.GameFlow.States
{
    /// <summary>
    /// Active gameplay state. Holds the active ILevelRuntime and PuzzleRuntime,
    /// ticks timers and game simulation, forwards player input, and listens
    /// for level completion/failure to transition to ResultState.
    /// Does NOT implement puzzle mechanics directly.
    /// </summary>
    public class PlayState : BaseState
    {
        public override GameStateId Id => GameStateId.Play;

        private readonly IGameStateMachine _stateMachine;
        private readonly IAnalyticsService _analyticsService;
        private readonly Action _onPlayHook;
        private readonly Action<float> _onUpdateHook;

        public ILevelRuntime ActiveRuntime { get; private set; }
        public PuzzleRuntime ActivePuzzleRuntime { get; private set; }

        public PlayState(IGameStateMachine stateMachine, IAnalyticsService analyticsService = null)
            : this(stateMachine, analyticsService, null, null) { }

        public PlayState(
            IGameStateMachine stateMachine,
            IAnalyticsService analyticsService,
            Action onPlayHook,
            Action<float> onUpdateHook = null)
        {
            _stateMachine = stateMachine;
            _analyticsService = analyticsService;
            _onPlayHook = onPlayHook;
            _onUpdateHook = onUpdateHook;
        }

        public PlayState(Action onPlayHook, Action<float> onUpdateHook = null) 
            : this(null, null, onPlayHook, onUpdateHook) { }

        public void SetRuntime(ILevelRuntime runtime)
        {
            ActiveRuntime = runtime;
        }

        public void SetPuzzleRuntime(PuzzleRuntime puzzleRuntime)
        {
            ActivePuzzleRuntime = puzzleRuntime;
            if (puzzleRuntime != null)
            {
                ActiveRuntime = puzzleRuntime.LevelRuntime;
            }
        }

        public override void Enter()
        {
            _onPlayHook?.Invoke();

            if (ActiveRuntime == null)
            {
                // Standalone test mode without runtime
                return;
            }

            CoreLogger.Log($"[GameFlow] Playing Level: {ActiveRuntime.Id} (Moves: {ActiveRuntime.RemainingMoves})");
            _analyticsService?.LogLevelStarted(ActiveRuntime.Id);

            ActiveRuntime.OnLevelFinished += HandleLevelFinished;
        }

        public override void Exit()
        {
            if (ActiveRuntime != null)
            {
                ActiveRuntime.OnLevelFinished -= HandleLevelFinished;
            }
            ActivePuzzleRuntime = null;
        }

        public override void Update(float deltaTime)
        {
            _onUpdateHook?.Invoke(deltaTime);
            if (ActivePuzzleRuntime != null)
            {
                ActivePuzzleRuntime.Tick(deltaTime);
            }
            else
            {
                ActiveRuntime?.UpdateTimer(deltaTime);
            }
        }

        public void ReceiveInput(IInputCommand command)
        {
            ActivePuzzleRuntime?.HandleInput(command);
        }

        public void PauseGame()
        {
            if (_stateMachine != null && _stateMachine.CanTransitionTo(GameStateId.Paused))
            {
                _stateMachine.ChangeState(GameStateId.Paused);
            }
        }

        private void HandleLevelFinished(LevelResultData result)
        {
            if (_stateMachine != null)
            {
                var resultState = _stateMachine.GetState<ResultState>(GameStateId.Result);
                if (resultState != null)
                {
                    resultState.SetResult(result);
                }

                _stateMachine.ChangeState(GameStateId.Result);
            }
        }
    }
}
