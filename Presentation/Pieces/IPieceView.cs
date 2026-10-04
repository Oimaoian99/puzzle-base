using System;
using Puzzle.Core.Gameplay.Grid;
using Puzzle.Core.Gameplay.Pieces;
using UnityEngine;

namespace Puzzle.Presentation.Pieces
{
    /// <summary>
    /// Minimal presentation interface for a Piece visual instance.
    /// Exposes visual position and state without coupling to MonoBehaviours.
    /// </summary>
    public interface IPieceView
    {
        Piece Model { get; }
        int PieceId { get; }
        PieceType PieceType { get; }
        GridPosition GridPosition { get; }
        Vector3 Position { get; }
        bool IsMoving { get; }
        bool IsHighlighted { get; }

        void Bind(Piece piece, GridPosition gridPosition, Vector3 initialWorldPosition);
        void SetGridPosition(GridPosition newGridPos);
        void MoveTo(Vector3 targetWorldPosition, bool animate = true, float speed = 15f);
        void SetHighlighted(bool highlighted);
        void OnRemoved();

        event Action<IPieceView> OnClicked;
    }
}
