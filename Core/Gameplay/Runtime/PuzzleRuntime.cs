using System;
using Puzzle.Core.Gameplay.Board;
using Puzzle.Core.Gameplay.Data;
using Puzzle.Core.Gameplay.Pieces;
using Puzzle.Core.Gameplay.Puzzle;
using Puzzle.Core.Input;
using Puzzle.Core.Level;
using Puzzle.Core.Logging;

namespace Puzzle.Core.Gameplay.Runtime
{
    /// <summary>
    /// Active puzzle session runtime.
    /// Owns the active Board and IPuzzleLogic, evaluates inputs,
    /// and reports completion or failure back to ILevelRuntime.
    /// Does NOT own UI, Ads, Analytics, or GameStateMachine directly.
    /// </summary>
    public class PuzzleRuntime : IDisposable
    {
        public LevelData Data { get; }
        public Board.Board Board { get; }
        public IPuzzleLogic Logic { get; }
        public ILevelRuntime LevelRuntime { get; }
        public PuzzleResult CurrentResult { get; private set; }

        public bool IsFinished => CurrentResult.IsFinished;

        public PuzzleRuntime(LevelData data, IPuzzleLogic logic, ILevelRuntime levelRuntime)
        {
            Data = data ?? throw new ArgumentNullException(nameof(data));
            Logic = logic ?? throw new ArgumentNullException(nameof(logic));
            LevelRuntime = levelRuntime ?? throw new ArgumentNullException(nameof(levelRuntime));

            // Construct Board from LevelData configuration
            Board = new Board.Board(data.Width, data.Height);
            ApplyLevelDataToBoard(data, Board);

            CurrentResult = PuzzleResult.Playing;

            // Initialize puzzle variant logic
            Logic.Initialize(Board, data, LevelRuntime);
        }

        private static void ApplyLevelDataToBoard(LevelData data, Board.Board board)
        {
            // Apply blocked cells
            if (data.BlockedCells != null)
            {
                foreach (var blocked in data.BlockedCells)
                {
                    var cell = board.GetCell(blocked);
                    if (cell != null)
                    {
                        cell.IsBlocked = true;
                    }
                }
            }

            // Apply starting pieces
            if (data.InitialPieces != null)
            {
                foreach (var setup in data.InitialPieces)
                {
                    if (setup.Type != PieceType.None)
                    {
                        var piece = new Piece(setup.PieceId, setup.Type, setup.Tag);
                        board.SetPiece(setup.Position, piece);
                    }
                }
            }
        }

        public void HandleInput(IInputCommand command)
        {
            if (IsFinished || command == null) return;

            Logic.ReceiveCommand(command);
            EvaluateResult();
        }

        public void Tick(float deltaTime)
        {
            if (IsFinished) return;

            LevelRuntime.UpdateTimer(deltaTime);
            Logic.Tick(deltaTime);
            EvaluateResult();
        }

        public void EvaluateResult()
        {
            if (IsFinished) return;

            var result = Logic.CheckResult();
            if (result.IsFinished)
            {
                CurrentResult = result;
                if (result.IsWin)
                {
                    CoreLogger.Log($"[PuzzleRuntime] Win condition reached: {result.Message} (Stars: {result.Stars})");
                    LevelRuntime.CompleteLevel(result.Stars);
                }
                else if (result.IsLose)
                {
                    CoreLogger.Log($"[PuzzleRuntime] Lose condition reached: {result.Message}");
                    LevelRuntime.FailLevel();
                }
            }
        }

        public void Dispose()
        {
            Logic.Reset();
            Board.Clear();
        }
    }
}
