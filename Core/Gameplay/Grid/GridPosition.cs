using System;
using UnityEngine;

namespace Puzzle.Core.Gameplay.Grid
{
    /// <summary>
    /// Lightweight, immutable 2D integer grid coordinate.
    /// Provides value equality and conversions to Vector2Int.
    /// </summary>
    [Serializable]
    public readonly struct GridPosition : IEquatable<GridPosition>
    {
        public int X { get; }
        public int Y { get; }

        public static readonly GridPosition Zero = new GridPosition(0, 0);

        public GridPosition(int x, int y)
        {
            X = x;
            Y = y;
        }

        public GridPosition Offset(int deltaX, int deltaY) => new GridPosition(X + deltaX, Y + deltaY);

        public int ManhattanDistance(GridPosition other) => Math.Abs(X - other.X) + Math.Abs(Y - other.Y);

        public bool Equals(GridPosition other) => X == other.X && Y == other.Y;

        public override bool Equals(object obj) => obj is GridPosition other && Equals(other);

        public override int GetHashCode() => (X * 397) ^ Y;

        public override string ToString() => $"({X}, {Y})";

        public static bool operator ==(GridPosition a, GridPosition b) => a.Equals(b);
        public static bool operator !=(GridPosition a, GridPosition b) => !a.Equals(b);

        public Vector2Int ToVector2Int() => new Vector2Int(X, Y);
        public static GridPosition operator +(GridPosition a, GridPosition b) => new GridPosition(a.X + b.X, a.Y + b.Y);
        public static GridPosition operator -(GridPosition a, GridPosition b) => new GridPosition(a.X - b.X, a.Y - b.Y);

        public static implicit operator Vector2Int(GridPosition pos) => new Vector2Int(pos.X, pos.Y);
        public static implicit operator GridPosition(Vector2Int v) => new GridPosition(v.x, v.y);
    }
}
