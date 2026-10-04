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

namespace Puzzle.Variants.Match3.Logic
{
    /// <summary>
    /// Pure C# gameplay logic for the Match-3 puzzle variant.
    /// Manages orthogonal swaps, match detection (>= 3 horizontal or vertical),
    /// invalid swap rollbacks, removal cascades, gravity, and refills.
    /// Completely independent of Unity GameObjects and presentation.
    /// </summary>
    public class Match3PuzzleLogic : IPuzzleLogic
    {
        public Board Board { get; private set; }
        public LevelData LevelData { get; private set; }
        public ILevelRuntime LevelRuntime { get; private set; }

        public GridPosition? SelectedPosition { get; private set; }
        public int BaseScorePerPiece { get; set; } = 100;
        public bool RefillOnClear { get; set; } = true;
        public int MaxCascadeIterations { get; set; } = 20;

        public event Action<GridPosition?> OnSelectionChanged;
        public event Action<GridPosition, GridPosition> OnSwapExecuted;
        public event Action<GridPosition, GridPosition> OnSwapFailed;
        public event Action<IReadOnlyList<GridPosition>, int> OnMatchResolved;

        private int _nextPieceId = 2000;
        private Random _rng = new Random(1337);

        private static readonly PieceType[] RefillPalette = new[]
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

            SelectedPosition = null;
            _nextPieceId = 2000;
        }

        public void ReceiveCommand(IInputCommand command)
        {
            if (command == null || (LevelRuntime != null && LevelRuntime.IsFinished))
                return;

            if (command is SwapCommand swap)
            {
                TrySwap(swap.From, swap.To);
            }
            else if (command is TapCommand tap)
            {
                var pos = new GridPosition(tap.GridCoordinate.x, tap.GridCoordinate.y);
                HandleTap(pos);
            }
            else if (command is SwipeCommand swipe)
            {
                var from = new GridPosition(swipe.OriginGridCoordinate.x, swipe.OriginGridCoordinate.y);
                var to = GetSwipeDestination(from, swipe.Direction);
                TrySwap(from, to);
            }
        }

        public void HandleTap(GridPosition pos)
        {
            if (Board == null || !Board.IsInside(pos)) return;

            var piece = Board.GetPiece(pos);
            if (piece == null || piece.Type == PieceType.None || piece.Type == PieceType.Obstacle)
            {
                ClearSelection();
                return;
            }

            if (!SelectedPosition.HasValue)
            {
                // First piece selected
                SelectedPosition = pos;
                OnSelectionChanged?.Invoke(SelectedPosition);
            }
            else
            {
                var previous = SelectedPosition.Value;

                if (previous == pos)
                {
                    // Tapped the same piece again -> deselect
                    ClearSelection();
                }
                else if (IsOrthogonallyAdjacent(previous, pos))
                {
                    // Adjacent piece tapped -> execute swap
                    ClearSelection();
                    TrySwap(previous, pos);
                }
                else
                {
                    // Non-adjacent piece tapped -> switch selection to new piece
                    SelectedPosition = pos;
                    OnSelectionChanged?.Invoke(SelectedPosition);
                }
            }
        }

        public void ClearSelection()
        {
            if (SelectedPosition.HasValue)
            {
                SelectedPosition = null;
                OnSelectionChanged?.Invoke(null);
            }
        }

        public bool TrySwap(GridPosition a, GridPosition b)
        {
            if (Board == null) return false;

            // 1. Validate bounds and adjacency
            if (!Board.IsInside(a) || !Board.IsInside(b))
            {
                OnSwapFailed?.Invoke(a, b);
                return false;
            }

            if (!IsOrthogonallyAdjacent(a, b))
            {
                OnSwapFailed?.Invoke(a, b);
                return false;
            }

            var cellA = Board.GetCell(a);
            var cellB = Board.GetCell(b);
            if (cellA == null || cellB == null || cellA.IsBlocked || cellB.IsBlocked)
            {
                OnSwapFailed?.Invoke(a, b);
                return false;
            }

            if (!cellA.HasPiece || !cellB.HasPiece)
            {
                OnSwapFailed?.Invoke(a, b);
                return false;
            }

            // 2. Perform tentative swap on model
            Board.SwapPieces(a, b);

            // 3. Check if swap formed any matches
            var matches = FindMatches();
            if (matches.Count == 0)
            {
                // No match created -> Invalid move! Rollback swap
                Board.SwapPieces(b, a);
                OnSwapFailed?.Invoke(a, b);
                return false;
            }

            // 4. Valid move! Consume 1 move and execute cascade loop
            if (LevelRuntime != null)
            {
                LevelRuntime.TryConsumeMoves(1);
            }

            OnSwapExecuted?.Invoke(a, b);

            ProcessMatchesAndCascades();

            return true;
        }

        public HashSet<GridPosition> FindMatches()
        {
            var matchSet = new HashSet<GridPosition>();
            if (Board == null) return matchSet;

            // 1. Horizontal match scan (rows)
            for (int y = 0; y < Board.Height; y++)
            {
                int matchStart = 0;
                PieceType currentType = PieceType.None;
                int currentRun = 0;

                for (int x = 0; x < Board.Width; x++)
                {
                    var pos = new GridPosition(x, y);
                    var piece = Board.GetPiece(pos);
                    var type = (piece != null && piece.Type != PieceType.Obstacle) ? piece.Type : PieceType.None;

                    if (type != PieceType.None && type == currentType)
                    {
                        currentRun++;
                    }
                    else
                    {
                        if (currentRun >= 3 && currentType != PieceType.None)
                        {
                            for (int i = 0; i < currentRun; i++)
                            {
                                matchSet.Add(new GridPosition(matchStart + i, y));
                            }
                        }

                        currentType = type;
                        matchStart = x;
                        currentRun = (type != PieceType.None) ? 1 : 0;
                    }
                }

                if (currentRun >= 3 && currentType != PieceType.None)
                {
                    for (int i = 0; i < currentRun; i++)
                    {
                        matchSet.Add(new GridPosition(matchStart + i, y));
                    }
                }
            }

            // 2. Vertical match scan (columns)
            for (int x = 0; x < Board.Width; x++)
            {
                int matchStart = 0;
                PieceType currentType = PieceType.None;
                int currentRun = 0;

                for (int y = 0; y < Board.Height; y++)
                {
                    var pos = new GridPosition(x, y);
                    var piece = Board.GetPiece(pos);
                    var type = (piece != null && piece.Type != PieceType.Obstacle) ? piece.Type : PieceType.None;

                    if (type != PieceType.None && type == currentType)
                    {
                        currentRun++;
                    }
                    else
                    {
                        if (currentRun >= 3 && currentType != PieceType.None)
                        {
                            for (int i = 0; i < currentRun; i++)
                            {
                                matchSet.Add(new GridPosition(x, matchStart + i));
                            }
                        }

                        currentType = type;
                        matchStart = y;
                        currentRun = (type != PieceType.None) ? 1 : 0;
                    }
                }

                if (currentRun >= 3 && currentType != PieceType.None)
                {
                    for (int i = 0; i < currentRun; i++)
                    {
                        matchSet.Add(new GridPosition(x, matchStart + i));
                    }
                }
            }

            return matchSet;
        }

        public void ProcessMatchesAndCascades()
        {
            int iterations = 0;

            while (iterations++ < MaxCascadeIterations)
            {
                var matches = FindMatches();
                if (matches.Count == 0) break;

                // Calculate score
                int points = matches.Count * BaseScorePerPiece;
                if (LevelRuntime != null)
                {
                    LevelRuntime.AddScore(points);
                }

                // Remove matched pieces from model
                foreach (var pos in matches)
                {
                    Board.RemovePiece(pos);
                }

                OnMatchResolved?.Invoke(new List<GridPosition>(matches), points);

                // Apply gravity
                ApplyGravity();

                // Refill vacated cells
                if (RefillOnClear)
                {
                    ApplyRefill();
                }
            }
        }

        public void ApplyGravity()
        {
            if (Board == null) return;

            for (int x = 0; x < Board.Width; x++)
            {
                for (int y = 0; y < Board.Height; y++)
                {
                    var targetPos = new GridPosition(x, y);
                    var targetCell = Board.GetCell(targetPos);
                    if (targetCell == null || targetCell.IsBlocked || targetCell.HasPiece)
                        continue;

                    // Find lowest available piece above
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
            }
        }

        public void ApplyRefill()
        {
            if (Board == null) return;

            for (int x = 0; x < Board.Width; x++)
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

        protected virtual PieceType GetNextRefillPieceType()
        {
            int index = _rng.Next(RefillPalette.Length);
            return RefillPalette[index];
        }

        public bool IsOrthogonallyAdjacent(GridPosition a, GridPosition b)
        {
            int dx = Math.Abs(a.X - b.X);
            int dy = Math.Abs(a.Y - b.Y);
            return (dx + dy) == 1;
        }

        private static GridPosition GetSwipeDestination(GridPosition origin, SwipeDirection direction)
        {
            switch (direction)
            {
                case SwipeDirection.Up: return new GridPosition(origin.X, origin.Y + 1);
                case SwipeDirection.Down: return new GridPosition(origin.X, origin.Y - 1);
                case SwipeDirection.Left: return new GridPosition(origin.X - 1, origin.Y);
                case SwipeDirection.Right: return new GridPosition(origin.X + 1, origin.Y);
                default: return origin;
            }
        }

        public void Tick(float deltaTime)
        {
            // Transient animation updates if needed
        }

        public PuzzleResult CheckResult()
        {
            if (LevelRuntime == null || LevelData == null)
            {
                return PuzzleResult.Playing;
            }

            // Win condition: Target score achieved
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

                return PuzzleResult.CreateWin(stars, $"Match-3 Target score of {LevelData.TargetScore} reached! (Final: {LevelRuntime.CurrentScore})");
            }

            // Lose condition: Out of moves before target score reached
            if (LevelRuntime.RemainingMoves <= 0)
            {
                return PuzzleResult.CreateLose($"Out of moves! Score: {LevelRuntime.CurrentScore}/{LevelData.TargetScore}");
            }

            return PuzzleResult.Playing;
        }

        public void Reset()
        {
            ClearSelection();
            _nextPieceId = 2000;
        }
    }
}
