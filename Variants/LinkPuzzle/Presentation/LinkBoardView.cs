using System.Collections.Generic;
using Puzzle.Core.Gameplay.Grid;
using Puzzle.Core.Gameplay.Pieces;
using Puzzle.Core.Input.Commands;
using Puzzle.Presentation.Board;
using Puzzle.Presentation.Feedback;
using Puzzle.Presentation.Pieces;
using Puzzle.Variants.Link.Logic;
using UnityEngine;

namespace Puzzle.Variants.Link.Presentation
{
    /// <summary>
    /// Specialized presentation for the Link/Line puzzle variant.
    /// Extends generic BoardView with path visualization, line rendering,
    /// and color tinting for piece types.
    /// Does NOT validate puzzle rules or calculate scores.
    /// </summary>
    public class LinkBoardView : BoardView
    {
        [SerializeField] private LineRenderer pathLineRenderer;
        [SerializeField] private Color colorA = new Color(0.95f, 0.26f, 0.21f); // Red
        [SerializeField] private Color colorB = new Color(0.13f, 0.59f, 0.95f); // Blue
        [SerializeField] private Color colorC = new Color(0.30f, 0.69f, 0.31f); // Green
        [SerializeField] private Color colorD = new Color(1.00f, 0.76f, 0.03f); // Yellow

        private LinkPuzzleLogic _linkLogic;
        private readonly List<GridPosition> _dragPath = new List<GridPosition>();
        private bool _isDragging;

        public void AttachLogic(LinkPuzzleLogic logic)
        {
            if (_linkLogic != null)
            {
                _linkLogic.OnPathUpdated -= HandlePathUpdated;
                _linkLogic.OnLinkCompleted -= HandleLinkCompleted;
                _linkLogic.OnLinkFailed -= HandleLinkFailed;
            }

            _linkLogic = logic;

            if (_linkLogic != null)
            {
                _linkLogic.OnPathUpdated += HandlePathUpdated;
                _linkLogic.OnLinkCompleted += HandleLinkCompleted;
                _linkLogic.OnLinkFailed += HandleLinkFailed;
            }
        }

        protected override PieceView CreatePieceView(GridPosition pos, Piece piece)
        {
            var view = base.CreatePieceView(pos, piece);

            // Apply color tint to piece visual representation
            ApplyPieceColor(view, piece.Type);

            return view;
        }

        private void ApplyPieceColor(PieceView view, PieceType type)
        {
            if (view == null) return;

            var color = GetColorForType(type);
            var spriteRenderer = view.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.color = color;
            }
            else
            {
                var meshRenderer = view.GetComponent<Renderer>();
                if (meshRenderer != null && meshRenderer.material != null)
                {
                    meshRenderer.material.color = color;
                }
            }
        }

        public Color GetColorForType(PieceType type)
        {
            switch (type)
            {
                case PieceType.ColorA: return colorA;
                case PieceType.ColorB: return colorB;
                case PieceType.ColorC: return colorC;
                case PieceType.ColorD: return colorD;
                default: return Color.white;
            }
        }

        private void HandlePathUpdated(IReadOnlyList<GridPosition> path)
        {
            UpdateVisualHighlights(path);
            UpdatePathLine(path);

            if (path != null && path.Count > 0)
            {
                var latest = path[path.Count - 1];
                var piece = Board?.GetPiece(latest);
                FeedbackPresenter?.HandleFeedback(FeedbackEvent.PieceSelected(latest, piece?.Type ?? PieceType.None));
            }
        }

        private void HandleLinkCompleted(IReadOnlyList<GridPosition> path, int score)
        {
            if (path != null && path.Count > 0)
            {
                var lead = path[0];
                var piece = Board?.GetPiece(lead);
                FeedbackPresenter?.HandleFeedback(FeedbackEvent.PieceMatched(lead, piece?.Type ?? PieceType.None, score));
            }
            ClearPathVisuals();
        }

        private void HandleLinkFailed(string reason)
        {
            FeedbackPresenter?.HandleFeedback(FeedbackEvent.PieceDeselected(default, PieceType.None));
            ClearPathVisuals();
        }

        private void UpdateVisualHighlights(IReadOnlyList<GridPosition> path)
        {
            var pathSet = path != null ? new HashSet<GridPosition>(path) : new HashSet<GridPosition>();

            foreach (var kvp in _pieceViews)
            {
                var view = kvp.Value;
                if (view != null)
                {
                    bool isHighlighted = pathSet.Contains(view.GridPosition);
                    view.SetHighlighted(isHighlighted);
                }
            }
        }

        private void UpdatePathLine(IReadOnlyList<GridPosition> path)
        {
            if (pathLineRenderer == null) return;

            if (path == null || path.Count < 2)
            {
                pathLineRenderer.positionCount = 0;
                return;
            }

            pathLineRenderer.positionCount = path.Count;
            for (int i = 0; i < path.Count; i++)
            {
                var worldPos = GridToWorldPosition(path[i]);
                worldPos.z = -0.5f; // Slight forward offset for line visibility
                pathLineRenderer.SetPosition(i, worldPos);
            }
        }

        private void ClearPathVisuals()
        {
            foreach (var kvp in _pieceViews)
            {
                if (kvp.Value != null)
                {
                    kvp.Value.SetHighlighted(false);
                }
            }

            if (pathLineRenderer != null)
            {
                pathLineRenderer.positionCount = 0;
            }
        }

        public void StartDrag(GridPosition startPos)
        {
            _isDragging = true;
            _dragPath.Clear();
            _dragPath.Add(startPos);
            UpdateVisualHighlights(_dragPath);
            UpdatePathLine(_dragPath);
        }

        public void ExtendDrag(GridPosition nextPos)
        {
            if (!_isDragging) return;
            if (_dragPath.Contains(nextPos)) return;

            if (_dragPath.Count > 0)
            {
                var last = _dragPath[_dragPath.Count - 1];
                int dx = Mathf.Abs(last.X - nextPos.X);
                int dy = Mathf.Abs(last.Y - nextPos.Y);
                if (dx > 1 || dy > 1 || (dx == 0 && dy == 0)) return;
            }

            _dragPath.Add(nextPos);
            UpdateVisualHighlights(_dragPath);
            UpdatePathLine(_dragPath);
        }

        public void EndDrag()
        {
            if (!_isDragging) return;
            _isDragging = false;

            if (_dragPath.Count >= 3)
            {
                // Emit LinkCommand through standard input boundary
                var command = new LinkCommand(new List<GridPosition>(_dragPath));
                EmitInput(command);
            }

            ClearPathVisuals();
            _dragPath.Clear();
        }

        private void OnDestroy()
        {
            if (_linkLogic != null)
            {
                _linkLogic.OnPathUpdated -= HandlePathUpdated;
                _linkLogic.OnLinkCompleted -= HandleLinkCompleted;
                _linkLogic.OnLinkFailed -= HandleLinkFailed;
                _linkLogic = null;
            }
        }
    }
}
