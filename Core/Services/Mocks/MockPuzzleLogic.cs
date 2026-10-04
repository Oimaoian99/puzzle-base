using System.Collections.Generic;
using Puzzle.Core.Gameplay.Board;
using Puzzle.Core.Gameplay.Data;
using Puzzle.Core.Gameplay.Puzzle;
using Puzzle.Core.Input;
using Puzzle.Core.Level;

namespace Puzzle.Core.Services.Mocks
{
    /// <summary>
    /// Deterministic offline mock implementation of IPuzzleLogic for unit testing and baseline game flow.
    /// Simulates puzzle mechanics and allows direct triggering of Win/Loss conditions.
    /// </summary>
    public class MockPuzzleLogic : IPuzzleLogic
    {
        public Board Board { get; private set; }
        public LevelData LevelData { get; private set; }
        public ILevelRuntime LevelRuntime { get; private set; }

        public List<IInputCommand> ReceivedCommands { get; } = new List<IInputCommand>();
        public int TickCount { get; private set; }
        public bool TriggerWinOnInput { get; set; }
        public bool TriggerLoseOnInput { get; set; }
        public int WinStars { get; set; } = 3;
        public int ScorePerInput { get; set; } = 100;

        private PuzzleResult _forcedResult = PuzzleResult.Playing;

        public void Initialize(Board board, LevelData levelData, ILevelRuntime runtime)
        {
            Board = board;
            LevelData = levelData;
            LevelRuntime = runtime;
            ReceivedCommands.Clear();
            TickCount = 0;
            _forcedResult = PuzzleResult.Playing;
        }

        public void ReceiveCommand(IInputCommand command)
        {
            if (command == null) return;
            ReceivedCommands.Add(command);

            if (ScorePerInput > 0 && LevelRuntime != null)
            {
                LevelRuntime.AddScore(ScorePerInput);
                LevelRuntime.TryConsumeMoves(1);
            }

            if (TriggerWinOnInput)
            {
                _forcedResult = PuzzleResult.CreateWin(WinStars, "Mock win condition triggered.");
            }
            else if (TriggerLoseOnInput)
            {
                _forcedResult = PuzzleResult.CreateLose("Mock lose condition triggered.");
            }
        }

        public void Tick(float deltaTime)
        {
            TickCount++;
        }

        public PuzzleResult CheckResult()
        {
            if (_forcedResult.IsFinished)
            {
                return _forcedResult;
            }

            if (LevelRuntime != null && LevelRuntime.RemainingMoves <= 0)
            {
                return PuzzleResult.CreateLose("Out of moves.");
            }

            return PuzzleResult.Playing;
        }

        public void ForceWin(int stars = 3, string message = "Forced win")
        {
            _forcedResult = PuzzleResult.CreateWin(stars, message);
        }

        public void ForceLose(string message = "Forced lose")
        {
            _forcedResult = PuzzleResult.CreateLose(message);
        }

        public void Reset()
        {
            ReceivedCommands.Clear();
            TickCount = 0;
            _forcedResult = PuzzleResult.Playing;
        }
    }
}
