using System;
using System.Collections.Generic;

namespace Puzzle.Core.GameFlow
{
    public class GameStateMachine : IGameStateMachine
    {
        private readonly Dictionary<GameStateId, IState> _states = new Dictionary<GameStateId, IState>();
        private IState _currentState;

        public IState CurrentState => _currentState;
        public GameStateId CurrentStateId => _currentState?.Id ?? GameStateId.None;

        public event Action<GameStateId, GameStateId> OnStateChanged;

        public void RegisterState(IState state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            _states[state.Id] = state;
        }

        public bool CanTransitionTo(GameStateId nextStateId)
        {
            if (!_states.ContainsKey(nextStateId)) return false;

            var current = CurrentStateId;
            if (current == GameStateId.None) return true; // Initial transition
            if (current == nextStateId) return false;      // No self-transition

            switch (current)
            {
                case GameStateId.Boot:
                    return nextStateId == GameStateId.Init;
                case GameStateId.Init:
                    return nextStateId == GameStateId.Home || nextStateId == GameStateId.LevelLoading;
                case GameStateId.Home:
                    return nextStateId == GameStateId.LevelLoading || nextStateId == GameStateId.Init;
                case GameStateId.LevelLoading:
                    return nextStateId == GameStateId.Play || nextStateId == GameStateId.Home;
                case GameStateId.Play:
                    return nextStateId == GameStateId.Paused || nextStateId == GameStateId.Result || nextStateId == GameStateId.Home;
                case GameStateId.Paused:
                    return nextStateId == GameStateId.Play || nextStateId == GameStateId.Home || nextStateId == GameStateId.LevelLoading;
                case GameStateId.Result:
                    return nextStateId == GameStateId.LevelLoading || nextStateId == GameStateId.Home;
                default:
                    return false;
            }
        }

        public void ChangeState(GameStateId nextStateId)
        {
            if (!_states.TryGetValue(nextStateId, out var nextState))
            {
                throw new KeyNotFoundException($"[GameStateMachine] State '{nextStateId}' is not registered.");
            }

            if (!CanTransitionTo(nextStateId))
            {
                throw new InvalidOperationException($"[GameStateMachine] Illegal state transition from '{CurrentStateId}' to '{nextStateId}'.");
            }

            var previousStateId = CurrentStateId;
            _currentState?.Exit();
            _currentState = nextState;
            _currentState.Enter();

            OnStateChanged?.Invoke(previousStateId, nextStateId);
        }

        public void Update(float deltaTime)
        {
            _currentState?.Update(deltaTime);
        }

        public T GetState<T>(GameStateId stateId) where T : class, IState
        {
            if (_states.TryGetValue(stateId, out var state))
            {
                return state as T;
            }
            return null;
        }
    }
}
