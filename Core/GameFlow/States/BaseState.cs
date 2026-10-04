namespace Puzzle.Core.GameFlow.States
{
    public abstract class BaseState : IState
    {
        public abstract GameStateId Id { get; }
        
        public virtual void Enter() { }
        public virtual void Exit() { }
        public virtual void Update(float deltaTime) { }
    }
}
