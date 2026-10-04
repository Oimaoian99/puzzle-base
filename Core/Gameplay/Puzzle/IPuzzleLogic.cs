using Puzzle.Core.Gameplay.Board;
using Puzzle.Core.Gameplay.Data;
using Puzzle.Core.Input;
using Puzzle.Core.Level;

namespace Puzzle.Core.Gameplay.Puzzle
{
    /// <summary>
    /// Boundary contract between generic Gameplay Core and puzzle-variant rules (Match-3, Line, Screw, etc.).
    /// Evaluates input commands against the Board and returns the active puzzle status.
    /// </summary>
    public interface IPuzzleLogic : IInputReceiver
    {
        void Initialize(Board.Board board, LevelData levelData, ILevelRuntime runtime);
        void Tick(float deltaTime);
        PuzzleResult CheckResult();
        void Reset();
    }
}
