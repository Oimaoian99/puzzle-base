using System;
using System.Collections.Generic;
using Puzzle.Core.Gameplay.Board;
using Puzzle.Core.Gameplay.Grid;
using Puzzle.Core.Gameplay.Pieces;
using Puzzle.Core.Input;
using Puzzle.Core.Input.Commands;
using Puzzle.Presentation.Board;
using Puzzle.Presentation.Pieces;
using Puzzle.Presentation.Feedback;
using UnityEngine;

namespace Puzzle.Tests.Presentation.Mocks
{
    /// <summary>
    /// Deterministic offline mock implementation of IBoardView for headless unit and integration testing.
    /// Simulates visual board synchronization and user clicks without requiring Unity's C++ GameObject engine.
    /// </summary>
    public class MockBoardView : IBoardView
    {
        public Vector2 CellSpacing { get; set; } = Vector2.one;
        public Vector2 Origin { get; set; } = Vector2.zero;

        public Board Board { get; private set; }

        private readonly Dictionary<int, IPieceView> _pieceViews = new Dictionary<int, IPieceView>();

        public int ActivePieceViewCount => _pieceViews.Count;
        public IFeedbackPresenter FeedbackPresenter { get; set; }

        public event Action<IInputCommand> OnInputProduced;

        public void Bind(Board board, Action<IInputCommand> inputReceiver = null)
        {
            Unbind();
            Board = board ?? throw new ArgumentNullException(nameof(board));

            Board.OnPiecePlaced += HandlePiecePlaced;
            Board.OnPieceRemoved += HandlePieceRemoved;
            Board.OnPieceMoved += HandlePieceMoved;
            Board.OnBoardCleared += HandleBoardCleared;

            if (inputReceiver != null)
            {
                OnInputProduced += inputReceiver;
            }

            Refresh();
        }

        public void Unbind()
        {
            if (Board != null)
            {
                Board.OnPiecePlaced -= HandlePiecePlaced;
                Board.OnPieceRemoved -= HandlePieceRemoved;
                Board.OnPieceMoved -= HandlePieceMoved;
                Board.OnBoardCleared -= HandleBoardCleared;
                Board = null;
            }

            _pieceViews.Clear();
        }

        public void Refresh()
        {
            _pieceViews.Clear();
            if (Board == null) return;

            for (int x = 0; x < Board.Width; x++)
            {
                for (int y = 0; y < Board.Height; y++)
                {
                    var pos = new GridPosition(x, y);
                    var piece = Board.GetPiece(pos);
                    if (piece != null)
                    {
                        CreatePieceView(pos, piece);
                    }
                }
            }
        }

        public bool HasPieceView(int pieceId) => _pieceViews.ContainsKey(pieceId);

        public IPieceView GetPieceView(int pieceId)
        {
            _pieceViews.TryGetValue(pieceId, out var view);
            return view;
        }

        public Vector3 GridToWorldPosition(GridPosition pos)
        {
            return new Vector3(Origin.x + pos.X * CellSpacing.x, Origin.y + pos.Y * CellSpacing.y, 0f);
        }

        private void HandlePiecePlaced(GridPosition pos, Piece piece)
        {
            CreatePieceView(pos, piece);
        }

        private void HandlePieceRemoved(GridPosition pos, Piece piece)
        {
            _pieceViews.Remove(piece.Id);
            FeedbackPresenter?.HandleFeedback(FeedbackEvent.PieceRemoved(pos, piece.Type));
        }

        private void HandlePieceMoved(GridPosition from, GridPosition to, Piece piece)
        {
            if (_pieceViews.TryGetValue(piece.Id, out var view))
            {
                view.SetGridPosition(to);
                view.MoveTo(GridToWorldPosition(to), animate: true);
                FeedbackPresenter?.HandleFeedback(FeedbackEvent.PieceMoved(to, piece.Type));
            }
        }

        private void HandleBoardCleared()
        {
            _pieceViews.Clear();
        }

        private IPieceView CreatePieceView(GridPosition pos, Piece piece)
        {
            var view = new MockPieceView();
            view.Bind(piece, pos, GridToWorldPosition(pos));
            view.OnClicked += HandlePieceViewClicked;
            _pieceViews[piece.Id] = view;
            return view;
        }

        private void HandlePieceViewClicked(IPieceView view)
        {
            if (view == null) return;
            var command = new TapCommand(Vector2.zero, view.GridPosition.ToVector2Int());
            OnInputProduced?.Invoke(command);
        }
    }
}
