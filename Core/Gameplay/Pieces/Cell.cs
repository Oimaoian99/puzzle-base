using System;
using Puzzle.Core.Gameplay.Grid;

namespace Puzzle.Core.Gameplay.Pieces
{
    /// <summary>
    /// Represents an individual cell slot on the board.
    /// Manages placed piece reference and blocked/obstacle state.
    /// </summary>
    [Serializable]
    public class Cell
    {
        public GridPosition Position { get; }
        public Piece CurrentPiece { get; private set; }
        public bool IsBlocked { get; set; }

        public bool HasPiece => CurrentPiece != null;
        public bool IsEmpty => CurrentPiece == null && !IsBlocked;

        public Cell(GridPosition position, bool isBlocked = false)
        {
            Position = position;
            IsBlocked = isBlocked;
            CurrentPiece = null;
        }

        public bool PlacePiece(Piece piece)
        {
            if (IsBlocked || CurrentPiece != null) return false;
            CurrentPiece = piece;
            return true;
        }

        public Piece RemovePiece()
        {
            var piece = CurrentPiece;
            CurrentPiece = null;
            return piece;
        }

        public void Clear()
        {
            CurrentPiece = null;
            IsBlocked = false;
        }

        public override string ToString() => $"Cell({Position}, HasPiece={HasPiece}, Blocked={IsBlocked})";
    }
}
