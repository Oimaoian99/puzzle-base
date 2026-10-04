using System;
using Puzzle.Core.Gameplay.Grid;
using Puzzle.Core.Gameplay.Pieces;

namespace Puzzle.Core.Gameplay.Board
{
    /// <summary>
    /// Deterministic 2D grid board representing cell slots and placed pieces.
    /// Does NOT determine win/lose, handle UI, input, audio, or animations.
    /// </summary>
    public class Board
    {
        private readonly Cell[,] _cells;

        public int Width { get; }
        public int Height { get; }
        public int TotalCells => Width * Height;

        public Board(int width, int height)
        {
            if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width), "Width must be positive.");
            if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height), "Height must be positive.");

            Width = width;
            Height = height;
            _cells = new Cell[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    _cells[x, y] = new Cell(new GridPosition(x, y));
                }
            }
        }

        public bool IsInside(GridPosition pos)
        {
            return pos.X >= 0 && pos.X < Width && pos.Y >= 0 && pos.Y < Height;
        }

        public bool IsInside(int x, int y)
        {
            return x >= 0 && x < Width && y >= 0 && y < Height;
        }

        public Cell GetCell(GridPosition pos)
        {
            if (!IsInside(pos)) return null;
            return _cells[pos.X, pos.Y];
        }

        public Cell GetCell(int x, int y) => GetCell(new GridPosition(x, y));

        public event Action<GridPosition, Piece> OnPiecePlaced;
        public event Action<GridPosition, Piece> OnPieceRemoved;
        public event Action<GridPosition, GridPosition, Piece> OnPieceMoved;
        public event Action OnBoardCleared;

        public Piece GetPiece(GridPosition pos)
        {
            var cell = GetCell(pos);
            return cell?.CurrentPiece;
        }

        public bool SetPiece(GridPosition pos, Piece piece)
        {
            var cell = GetCell(pos);
            if (cell == null || cell.IsBlocked) return false;

            if (!cell.PlacePiece(piece)) return false;
            OnPiecePlaced?.Invoke(pos, piece);
            return true;
        }

        public Piece RemovePiece(GridPosition pos)
        {
            var cell = GetCell(pos);
            if (cell == null) return null;

            var piece = cell.RemovePiece();
            if (piece != null)
            {
                OnPieceRemoved?.Invoke(pos, piece);
            }
            return piece;
        }

        public bool MovePiece(GridPosition from, GridPosition to)
        {
            if (!IsInside(from) || !IsInside(to)) return false;

            var fromCell = GetCell(from);
            var toCell = GetCell(to);
            if (fromCell == null || toCell == null) return false;
            if (toCell.IsBlocked || toCell.HasPiece) return false;

            var piece = fromCell.RemovePiece();
            if (piece == null) return false;

            toCell.PlacePiece(piece);
            OnPieceMoved?.Invoke(from, to, piece);
            return true;
        }

        public bool SwapPieces(GridPosition a, GridPosition b)
        {
            if (!IsInside(a) || !IsInside(b)) return false;

            var cellA = GetCell(a);
            var cellB = GetCell(b);
            if (cellA == null || cellB == null) return false;
            if (cellA.IsBlocked || cellB.IsBlocked) return false;

            var pieceA = cellA.RemovePiece();
            var pieceB = cellB.RemovePiece();

            if (pieceA == null || pieceB == null)
            {
                if (pieceA != null) cellA.PlacePiece(pieceA);
                if (pieceB != null) cellB.PlacePiece(pieceB);
                return false;
            }

            cellA.PlacePiece(pieceB);
            cellB.PlacePiece(pieceA);

            OnPieceMoved?.Invoke(a, b, pieceA);
            OnPieceMoved?.Invoke(b, a, pieceB);
            return true;
        }

        public void Clear()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    _cells[x, y].Clear();
                }
            }
            OnBoardCleared?.Invoke();
        }
    }
}
