using System;
using System.Collections.Generic;
using Puzzle.Core.Gameplay.Board;
using Puzzle.Core.Gameplay.Grid;
using Puzzle.Core.Gameplay.Pieces;
using Puzzle.Core.Input;
using Puzzle.Core.Input.Commands;
using Puzzle.Presentation.Pieces;
using UnityEngine;

namespace Puzzle.Presentation.Board
{
    /// <summary>
    /// Visual presentation representation of a Board.
    /// Observes the pure C# Board model and synchronizes PieceView instances.
    /// Does NOT determine win/lose, validate puzzle rules, or modify LevelData.
    /// </summary>
    public class BoardView : MonoBehaviour, IBoardView
    {
        [SerializeField] private Vector2 cellSpacing = Vector2.one;
        [SerializeField] private Vector2 origin = Vector2.zero;
        [SerializeField] private PieceView piecePrefab;

        public Vector2 CellSpacing
        {
            get => cellSpacing;
            set => cellSpacing = value;
        }

        public Vector2 Origin
        {
            get => origin;
            set => origin = value;
        }

        public PieceView PiecePrefab
        {
            get => piecePrefab;
            set => piecePrefab = value;
        }

        public Core.Gameplay.Board.Board Board { get; private set; }

        protected readonly Dictionary<int, PieceView> _pieceViews = new Dictionary<int, PieceView>();

        public int ActivePieceViewCount => _pieceViews.Count;
        public Feedback.IFeedbackPresenter FeedbackPresenter { get; set; }

        public event Action<IInputCommand> OnInputProduced;

        protected void EmitInput(IInputCommand command)
        {
            OnInputProduced?.Invoke(command);
        }

        public void Bind(Core.Gameplay.Board.Board board, Action<IInputCommand> inputReceiver = null)
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

            ClearVisualPieces();
        }

        public void Refresh()
        {
            ClearVisualPieces();

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

        public Vector3 GridToWorldPosition(GridPosition pos)
        {
            return new Vector3(origin.x + pos.X * cellSpacing.x, origin.y + pos.Y * cellSpacing.y, 0f);
        }

        public IPieceView GetPieceView(int pieceId)
        {
            _pieceViews.TryGetValue(pieceId, out var view);
            return view;
        }

        public bool HasPieceView(int pieceId)
        {
            return _pieceViews.ContainsKey(pieceId);
        }

        private void HandlePiecePlaced(GridPosition pos, Piece piece)
        {
            if (_pieceViews.ContainsKey(piece.Id))
            {
                RemovePieceView(piece.Id);
            }

            CreatePieceView(pos, piece);
        }

        private void HandlePieceRemoved(GridPosition pos, Piece piece)
        {
            RemovePieceView(piece.Id);
            FeedbackPresenter?.HandleFeedback(Feedback.FeedbackEvent.PieceRemoved(pos, piece.Type));
        }

        private void HandlePieceMoved(GridPosition from, GridPosition to, Piece piece)
        {
            if (_pieceViews.TryGetValue(piece.Id, out var view))
            {
                view.SetGridPosition(to);
                view.MoveTo(GridToWorldPosition(to), animate: true);
                FeedbackPresenter?.HandleFeedback(Feedback.FeedbackEvent.PieceMoved(to, piece.Type));
            }
        }

        private void HandleBoardCleared()
        {
            ClearVisualPieces();
        }

        protected virtual PieceView CreatePieceView(GridPosition pos, Piece piece)
        {
            PieceView view;
            if (piecePrefab != null)
            {
                view = Instantiate(piecePrefab, transform);
            }
            else
            {
                var go = new GameObject($"Piece_{piece.Id}");
                go.transform.SetParent(transform, false);
                view = go.AddComponent<PieceView>();
            }

            view.Bind(piece, pos, GridToWorldPosition(pos));
            view.OnClicked += HandlePieceViewClicked;

            _pieceViews[piece.Id] = view;
            return view;
        }

        protected virtual void RemovePieceView(int pieceId)
        {
            if (_pieceViews.TryGetValue(pieceId, out var view))
            {
                view.OnClicked -= HandlePieceViewClicked;
                view.OnRemoved();
                _pieceViews.Remove(pieceId);
            }
        }

        protected virtual void HandlePieceViewClicked(IPieceView view)
        {
            if (view == null) return;

            // Formulate tap command and forward through presentation input boundary
            var screenPos = Vector2.zero;
            var command = new TapCommand(screenPos, view.GridPosition.ToVector2Int());
            EmitInput(command);
        }

        private void ClearVisualPieces()
        {
            foreach (var kvp in _pieceViews)
            {
                var view = kvp.Value;
                if (view != null)
                {
                    view.OnClicked -= HandlePieceViewClicked;
                    view.OnRemoved();
                }
            }
            _pieceViews.Clear();
        }

        private void OnDestroy()
        {
            Unbind();
        }
    }
}
