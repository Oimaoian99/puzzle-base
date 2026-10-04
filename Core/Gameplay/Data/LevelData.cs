using System;
using System.Collections.Generic;
using Puzzle.Core.Gameplay.Grid;
using Puzzle.Core.Gameplay.Pieces;
using Puzzle.Core.Level;

namespace Puzzle.Core.Gameplay.Data
{
    /// <summary>
    /// Static placement descriptor for a piece during level initialization.
    /// </summary>
    [Serializable]
    public class InitialPieceSetup
    {
        public GridPosition Position { get; set; }
        public PieceType Type { get; set; }
        public int PieceId { get; set; }
        public string Tag { get; set; }

        public InitialPieceSetup() { }

        public InitialPieceSetup(GridPosition position, PieceType type, int pieceId = 0, string tag = null)
        {
            Position = position;
            Type = type;
            PieceId = pieceId;
            Tag = tag ?? string.Empty;
        }
    }

    /// <summary>
    /// Static level configuration defining board dimensions, starting pieces, and move limits.
    /// Completely immutable during active gameplay sessions.
    /// Distinct from mutable runtime state (Board and PuzzleRuntime).
    /// </summary>
    [Serializable]
    public class LevelData
    {
        public LevelId Id { get; set; }
        public string VariantId { get; set; } = string.Empty;
        public int SchemaVersion { get; set; } = 1;
        public int Width { get; set; }
        public int Height { get; set; }
        public int StartingMoves { get; set; }
        public int TargetScore { get; set; }
        public List<GridPosition> BlockedCells { get; set; } = new List<GridPosition>();
        public List<InitialPieceSetup> InitialPieces { get; set; } = new List<InitialPieceSetup>();

        public LevelData() { }

        public LevelData(LevelId id, int width, int height, int startingMoves = 25, int targetScore = 1000, string variantId = "")
        {
            Id = id;
            Width = width;
            Height = height;
            StartingMoves = startingMoves;
            TargetScore = targetScore;
            VariantId = variantId ?? string.Empty;
        }

        public static LevelData CreateDefault(LevelId id, int width = 8, int height = 8, int startingMoves = 25, int targetScore = 1000, string variantId = "")
        {
            return new LevelData(id, width, height, startingMoves, targetScore, variantId);
        }
    }
}
