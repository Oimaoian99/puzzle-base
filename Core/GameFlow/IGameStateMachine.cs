using System;

namespace Puzzle.Core.GameFlow
{
    public interface IGameStateMachine
    {
        IState CurrentState { get; }
        GameStateId CurrentStateId { get; }
        
        event Action<GameStateId, GameStateId> OnStateChanged;
        
        void RegisterState(IState state);
        void ChangeState(GameStateId nextStateId);
        bool CanTransitionTo(GameStateId nextStateId);
        void Update(float deltaTime);
        T GetState<T>(GameStateId stateId) where T : class, IState;
    }
}
