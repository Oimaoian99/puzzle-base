using System;
using Puzzle.Core.Gameplay.Grid;
using Puzzle.Core.Gameplay.Pieces;
using UnityEngine;

namespace Puzzle.Presentation.Pieces
{
    /// <summary>
    /// Visual presentation representation of a Piece.
    /// Responsible only for visual display, position updates, and local movement interpolation.
    /// Does NOT implement puzzle mechanics, rules, or win/lose calculation.
    /// </summary>
    public class PieceView : MonoBehaviour, IPieceView
    {
        public Piece Model { get; private set; }
        public GridPosition GridPosition { get; private set; }
        public int PieceId => Model != null ? Model.Id : 0;
        public PieceType PieceType => Model != null ? Model.Type : PieceType.None;
        public Vector3 Position => transform.position;

        public bool IsMoving => _isMoving;
        public bool IsHighlighted { get; private set; }

        public event Action<IPieceView> OnClicked;

        private Vector3 _targetPosition;
        private bool _isMoving;
        private float _moveSpeed = 15f;

        public void Bind(Piece piece, GridPosition gridPosition, Vector3 initialWorldPosition)
        {
            Model = piece ?? throw new ArgumentNullException(nameof(piece));
            GridPosition = gridPosition;
            transform.position = initialWorldPosition;
            _targetPosition = initialWorldPosition;
            _isMoving = false;
            IsHighlighted = false;

            gameObject.name = $"Piece_{piece.Id}_{piece.Type}";
        }

        public void SetGridPosition(GridPosition newGridPos)
        {
            GridPosition = newGridPos;
        }

        public void SetHighlighted(bool highlighted)
        {
            IsHighlighted = highlighted;
            transform.localScale = highlighted ? new Vector3(1.2f, 1.2f, 1.2f) : Vector3.one;
        }

        public void MoveTo(Vector3 targetWorldPosition, bool animate = true, float speed = 15f)
        {
            _moveSpeed = speed > 0f ? speed : 15f;
            _targetPosition = targetWorldPosition;

            if (!animate)
            {
                transform.position = targetWorldPosition;
                _isMoving = false;
                return;
            }

            _isMoving = true;
        }

        private void Update()
        {
            if (!_isMoving) return;

            transform.position = Vector3.MoveTowards(transform.position, _targetPosition, _moveSpeed * Time.deltaTime);
            if (Vector3.Distance(transform.position, _targetPosition) < 0.001f)
            {
                transform.position = _targetPosition;
                _isMoving = false;
            }
        }

        public void HandleClick()
        {
            OnClicked?.Invoke(this);
        }

        public void OnRemoved()
        {
            if (gameObject != null)
            {
                Destroy(gameObject);
            }
        }
    }
}
