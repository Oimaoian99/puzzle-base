namespace Puzzle.Core.GameFlow
{
    public interface IState
    {
        GameStateId Id { get; }
        void Enter();
        void Exit();
        void Update(float deltaTime);
    }
}
