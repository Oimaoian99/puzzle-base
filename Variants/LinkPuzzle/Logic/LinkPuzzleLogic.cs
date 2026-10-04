using System;
using System.Collections.Generic;
using Puzzle.Core.Gameplay.Board;
using Puzzle.Core.Gameplay.Data;
using Puzzle.Core.Gameplay.Grid;
using Puzzle.Core.Gameplay.Pieces;
using Puzzle.Core.Gameplay.Puzzle;
using Puzzle.Core.Input;
using Puzzle.Core.Input.Commands;
using Puzzle.Core.Level;

namespace Puzzle.Variants.Link.Logic
{
    /// <summary>
    /// Pure C# gameplay logic for the Link/Line puzzle variant.
    /// Handles chain validation, adjacency, score calculation, gravity/refill,
    /// and win/loss determination.
    /// Contains ZERO Unity/GameObject dependencies.
    /// </summary>
    public class LinkPuzzleLogic : IPuzzleLogic
    {
        public Board Board { get; private set; }
        public LevelData LevelData { get; private set; }
        public ILevelRuntime LevelRuntime { get; private set; }

        public int MinChainLength { get; set; } = 3;
        public int BaseScorePerPiece { get; set; } = 100;
        public int BonusPerExtraPiece { get; set; } = 50;
        public bool AllowDiagonals { get; set; } = true;
        public bool RefillOnClear { get; set; } = true;

        private readonly List<GridPosition> _currentPath = new List<GridPosition>();
        public IReadOnlyList<GridPosition> CurrentPath => _currentPath;

        public event Action<IReadOnlyList<GridPosition>> OnPathUpdated;
        public event Action<IReadOnlyList<GridPosition>, int> OnLinkCompleted;
        public event Action<string> OnLinkFailed;

        private int _nextPieceId = 1000;
        private Random _rng = new Random(42);

        private static readonly PieceType[] RefillTypes = new[]
        {
            PieceType.ColorA,
            PieceType.ColorB,
            PieceType.ColorC,
            PieceType.ColorD
        };

        public void SetRandomSeed(int seed)
        {
            _rng = new Random(seed);
        }

        public void Initialize(Board board, LevelData levelData, ILevelRuntime runtime)
        {
            Board = board ?? throw new ArgumentNullException(nameof(board));
            LevelData = levelData ?? throw new ArgumentNullException(nameof(levelData));
            LevelRuntime = runtime ?? throw new ArgumentNullException(nameof(runtime));

            _currentPath.Clear();
            _nextPieceId = 1000;
        }

        public void ReceiveCommand(IInputCommand command)
        {
            if (command == null || (LevelRuntime != null && LevelRuntime.IsFinished))
                return;

            if (command is LinkCommand linkCmd)
            {
                TryExecuteLink(linkCmd.Path);
            }
            else if (command is TapCommand tapCmd)
            {
                var gridPos = new GridPosition(tapCmd.GridCoordinate.x, tapCmd.GridCoordinate.y);
                HandleTap(gridPos);
            }
        }

        public void HandleTap(GridPosition pos)
        {
            if (Board == null || !Board.IsInside(pos)) return;

            var piece = Board.GetPiece(pos);
            if (piece == null || piece.Type == PieceType.None || piece.Type == PieceType.Obstacle)
            {
                ClearPath();
                return;
            }

            if (_currentPath.Count == 0)
            {
                _currentPath.Add(pos);
                OnPathUpdated?.Invoke(_currentPath);
                return;
            }

            // Tapping the last selected piece confirms the chain
            if (pos == _currentPath[_currentPath.Count - 1])
            {
                if (_currentPath.Count >= MinChainLength)
                {
                    TryExecuteLink(new List<GridPosition>(_currentPath));
                }
                else
                {
                    OnLinkFailed?.Invoke($"Chain too short (needs at least {MinChainLength} pieces).");
                    ClearPath();
                }
                return;
            }

            // Tapping the previous piece backtracks one step
            if (_currentPath.Count > 1 && pos == _currentPath[_currentPath.Count - 2])
            {
                _currentPath.RemoveAt(_currentPath.Count - 1);
                OnPathUpdated?.Invoke(_currentPath);
                return;
            }

            // Tapping an adjacent piece of matching type extends the chain
            if (CanExtendPath(pos))
            {
                _currentPath.Add(pos);
                OnPathUpdated?.Invoke(_currentPath);
                return;
            }

            // Otherwise, invalid extension: reset selection and start fresh with tapped piece
            _currentPath.Clear();
            _currentPath.Add(pos);
            OnPathUpdated?.Invoke(_currentPath);
        }

        public bool CanExtendPath(GridPosition pos)
        {
            if (_currentPath.Count == 0) return true;
            if (_currentPath.Contains(pos)) return false;

            var lastPos = _currentPath[_currentPath.Count - 1];
            if (!IsAdjacent(lastPos, pos)) return false;

            var firstPiece = Board.GetPiece(_currentPath[0]);
            var nextPiece = Board.GetPiece(pos);
            if (firstPiece == null || nextPiece == null) return false;

            return firstPiece.Type == nextPiece.Type;
        }

        public bool CanLink(IReadOnlyList<GridPosition> path, out string failureReason)
        {
            if (path == null || path.Count < MinChainLength)
            {
                failureReason = $"Chain too short. Minimum required: {MinChainLength}.";
                return false;
            }

            if (Board == null)
            {
                failureReason = "Board is null.";
                return false;
            }

            var visited = new HashSet<GridPosition>();
            PieceType expectedType = PieceType.None;

            for (int i = 0; i < path.Count; i++)
            {
                var pos = path[i];

                if (!Board.IsInside(pos))
                {
                    failureReason = $"Position {pos} is out of board bounds.";
                    return false;
                }

                if (visited.Contains(pos))
                {
                    failureReason = $"Duplicate cell in path: {pos}.";
                    return false;
                }
                visited.Add(pos);

                var piece = Board.GetPiece(pos);
                if (piece == null || piece.Type == PieceType.None || piece.Type == PieceType.Obstacle)
                {
                    failureReason = $"Cell at {pos} does not contain a linkable piece.";
                    return false;
                }

                if (i == 0)
                {
                    expectedType = piece.Type;
                }
                else
                {
                    if (piece.Type != expectedType)
                    {
                        failureReason = $"Piece type mismatch at {pos} (expected {expectedType}, got {piece.Type}).";
                        return false;
                    }

                    var prevPos = path[i - 1];
                    if (!IsAdjacent(prevPos, pos))
                    {
                        failureReason = $"Cells {prevPos} and {pos} are not adjacent.";
                        return false;
                    }
                }
            }

            failureReason = string.Empty;
            return true;
        }

        public bool TryExecuteLink(IReadOnlyList<GridPosition> path)
        {
            if (!CanLink(path, out var failureReason))
            {
                OnLinkFailed?.Invoke(failureReason);
                ClearPath();
                return false;
            }

            // Calculate score
            int count = path.Count;
            int score = (count * BaseScorePerPiece) + (Math.Max(0, count - MinChainLength) * BonusPerExtraPiece);

            // Remove pieces from model
            foreach (var pos in path)
            {
                Board.RemovePiece(pos);
            }

            // Update level runtime metrics
            if (LevelRuntime != null)
            {
                LevelRuntime.AddScore(score);
                LevelRuntime.TryConsumeMoves(1);
            }

            OnLinkCompleted?.Invoke(path, score);
            ClearPath();

            // Apply gravity and refill
            ApplyGravityAndRefill();

            return true;
        }

        public void ClearPath()
        {
            if (_currentPath.Count > 0)
            {
                _currentPath.Clear();
                OnPathUpdated?.Invoke(_currentPath);
            }
        }

        public bool IsAdjacent(GridPosition a, GridPosition b)
        {
            int dx = Math.Abs(a.X - b.X);
            int dy = Math.Abs(a.Y - b.Y);

            if (dx == 0 && dy == 0) return false;

            if (AllowDiagonals)
            {
                return dx <= 1 && dy <= 1;
            }

            return (dx + dy) == 1;
        }

        public void ApplyGravityAndRefill()
        {
            if (Board == null) return;

            // 1. Shift pieces down column by column
            for (int x = 0; x < Board.Width; x++)
            {
                for (int y = 0; y < Board.Height; y++)
                {
                    var targetPos = new GridPosition(x, y);
                    var targetCell = Board.GetCell(targetPos);
                    if (targetCell == null || targetCell.IsBlocked || targetCell.HasPiece)
                        continue;

                    // Search above for the lowest available piece
                    for (int aboveY = y + 1; aboveY < Board.Height; aboveY++)
                    {
                        var sourcePos = new GridPosition(x, aboveY);
                        var sourceCell = Board.GetCell(sourcePos);
                        if (sourceCell != null && !sourceCell.IsBlocked && sourceCell.HasPiece)
                        {
                            Board.MovePiece(sourcePos, targetPos);
                            break;
                        }
                    }
                }

                // 2. Refill empty cells at the top of the column
                if (RefillOnClear)
                {
                    for (int y = 0; y < Board.Height; y++)
                    {
                        var pos = new GridPosition(x, y);
                        var cell = Board.GetCell(pos);
                        if (cell != null && !cell.IsBlocked && !cell.HasPiece)
                        {
                            var type = GetNextRefillPieceType();
                            var piece = new Piece(_nextPieceId++, type);
                            Board.SetPiece(pos, piece);
                        }
                    }
                }
            }
        }

        protected virtual PieceType GetNextRefillPieceType()
        {
            int index = _rng.Next(RefillTypes.Length);
            return RefillTypes[index];
        }

        public void Tick(float deltaTime)
        {
            // Variant animation/idle timers if needed
        }

        public PuzzleResult CheckResult()
        {
            if (LevelRuntime == null || LevelData == null)
            {
                return PuzzleResult.Playing;
            }

            // Win condition: Target score reached
            if (LevelRuntime.CurrentScore >= LevelData.TargetScore)
            {
                int stars = 1;
                if (LevelRuntime.CurrentScore >= LevelData.TargetScore * 2)
                {
                    stars = 3;
                }
                else if (LevelRuntime.CurrentScore >= (int)(LevelData.TargetScore * 1.5f))
                {
                    stars = 2;
                }

                return PuzzleResult.CreateWin(stars, $"Target score of {LevelData.TargetScore} achieved! (Final: {LevelRuntime.CurrentScore})");
            }

            // Lose condition: Moves exhausted before target score reached
            if (LevelRuntime.RemainingMoves <= 0)
            {
                return PuzzleResult.CreateLose($"Out of moves! Score: {LevelRuntime.CurrentScore}/{LevelData.TargetScore}");
            }

            return PuzzleResult.Playing;
        }

        public void Reset()
        {
            _currentPath.Clear();
            _nextPieceId = 1000;
        }
    }
}
