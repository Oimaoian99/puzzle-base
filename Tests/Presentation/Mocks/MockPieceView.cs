using System;
using Puzzle.Core.Gameplay.Grid;
using Puzzle.Core.Gameplay.Pieces;
using Puzzle.Presentation.Pieces;
using UnityEngine;

namespace Puzzle.Tests.Presentation.Mocks
{
    public class MockPieceView : IPieceView
    {
        public Piece Model { get; private set; }
        public int PieceId => Model != null ? Model.Id : 0;
        public PieceType PieceType => Model != null ? Model.Type : PieceType.None;
        public GridPosition GridPosition { get; private set; }
        public Vector3 Position { get; private set; }
        public bool IsMoving { get; private set; }
        public bool IsHighlighted { get; private set; }

        public event Action<IPieceView> OnClicked;

        public void Bind(Piece piece, GridPosition gridPosition, Vector3 initialWorldPosition)
        {
            Model = piece ?? throw new ArgumentNullException(nameof(piece));
            GridPosition = gridPosition;
            Position = initialWorldPosition;
            IsMoving = false;
            IsHighlighted = false;
        }

        public void SetGridPosition(GridPosition newGridPos)
        {
            GridPosition = newGridPos;
        }

        public void SetHighlighted(bool highlighted)
        {
            IsHighlighted = highlighted;
        }

        public void MoveTo(Vector3 targetWorldPosition, bool animate = true, float speed = 15f)
        {
            Position = targetWorldPosition;
            IsMoving = animate;
        }

        public void OnRemoved()
        {
            // Simulates destruction without Unity C++ engine
        }

        public void SimulateClick()
        {
            OnClicked?.Invoke(this);
        }
    }
}
