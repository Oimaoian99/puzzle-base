using System;
using System.Collections.Generic;
using Puzzle.Core.Gameplay.Grid;
using Puzzle.Core.Gameplay.Pieces;
using Puzzle.Presentation.Board;
using Puzzle.Presentation.Feedback;
using Puzzle.Presentation.Pieces;
using Puzzle.Variants.Match3.Logic;
using UnityEngine;

namespace Puzzle.Variants.Match3.Presentation
{
    /// <summary>
    /// Specialized presentation for the Match-3 puzzle variant.
    /// Extends generic BoardView with piece selection highlights and color tinting.
    /// Does NOT validate matches or execute puzzle rules.
    /// </summary>
    public class Match3BoardView : BoardView
    {
        [SerializeField] private Color colorA = new Color(0.95f, 0.26f, 0.21f); // Red
        [SerializeField] private Color colorB = new Color(0.13f, 0.59f, 0.95f); // Blue
        [SerializeField] private Color colorC = new Color(0.30f, 0.69f, 0.31f); // Green
        [SerializeField] private Color colorD = new Color(1.00f, 0.76f, 0.03f); // Yellow

        private Match3PuzzleLogic _match3Logic;
        private GridPosition? _currentSelection;

        public void AttachLogic(Match3PuzzleLogic logic)
        {
            if (_match3Logic != null)
            {
                _match3Logic.OnSelectionChanged -= HandleSelectionChanged;
                _match3Logic.OnSwapFailed -= HandleSwapFailed;
                _match3Logic.OnMatchResolved -= HandleMatchResolved;
            }

            _match3Logic = logic;

            if (_match3Logic != null)
            {
                _match3Logic.OnSelectionChanged += HandleSelectionChanged;
                _match3Logic.OnSwapFailed += HandleSwapFailed;
                _match3Logic.OnMatchResolved += HandleMatchResolved;
            }
        }

        protected override PieceView CreatePieceView(GridPosition pos, Piece piece)
        {
            var view = base.CreatePieceView(pos, piece);
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
                var renderer = view.GetComponent<Renderer>();
                if (renderer != null && renderer.material != null)
                {
                    renderer.material.color = color;
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

        private void HandleSelectionChanged(GridPosition? selectedPos)
        {
            _currentSelection = selectedPos;

            foreach (var kvp in _pieceViews)
            {
                var view = kvp.Value;
                if (view != null)
                {
                    bool isSelected = selectedPos.HasValue && view.GridPosition == selectedPos.Value;
                    view.SetHighlighted(isSelected);
                }
            }

            if (selectedPos.HasValue)
            {
                var piece = Board?.GetPiece(selectedPos.Value);
                FeedbackPresenter?.HandleFeedback(FeedbackEvent.PieceSelected(selectedPos.Value, piece?.Type ?? PieceType.None));
            }
        }

        private void HandleSwapFailed(GridPosition a, GridPosition b)
        {
            FeedbackPresenter?.HandleFeedback(FeedbackEvent.PieceDeselected(a, PieceType.None));
            HandleSelectionChanged(null);
        }

        private void HandleMatchResolved(IReadOnlyList<GridPosition> matchedPositions, int score)
        {
            if (matchedPositions != null && matchedPositions.Count > 0)
            {
                var lead = matchedPositions[0];
                var piece = Board?.GetPiece(lead);
                FeedbackPresenter?.HandleFeedback(FeedbackEvent.PieceMatched(lead, piece?.Type ?? PieceType.None, score));
            }
        }

        private void OnDestroy()
        {
            if (_match3Logic != null)
            {
                _match3Logic.OnSelectionChanged -= HandleSelectionChanged;
                _match3Logic.OnSwapFailed -= HandleSwapFailed;
                _match3Logic.OnMatchResolved -= HandleMatchResolved;
                _match3Logic = null;
            }
        }
    }
}
