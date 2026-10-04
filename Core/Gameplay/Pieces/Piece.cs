using System;

namespace Puzzle.Core.Gameplay.Pieces
{
    /// <summary>
    /// Represents an individual piece instance placed on the board.
    /// Uses simple data and composition without inheritance chains.
    /// </summary>
    [Serializable]
    public class Piece
    {
        public int Id { get; }
        public PieceType Type { get; }
        public string Tag { get; }

        public Piece(int id, PieceType type, string tag = null)
        {
            Id = id;
            Type = type;
            Tag = tag ?? string.Empty;
        }

        public override string ToString() => $"Piece(Id={Id}, Type={Type})";
    }
}
