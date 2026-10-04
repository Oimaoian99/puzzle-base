using UnityEngine;

namespace Puzzle.Core.Input.Commands
{
    public readonly struct TapCommand : IInputCommand
    {
        public string CommandName => "Tap";
        public float Timestamp { get; }
        public Vector2 ScreenPosition { get; }
        public Vector2Int GridCoordinate { get; }

        public TapCommand(Vector2 screenPosition, Vector2Int gridCoordinate, float timestamp = 0f)
        {
            ScreenPosition = screenPosition;
            GridCoordinate = gridCoordinate;
            Timestamp = timestamp;
        }
    }

    public enum DragPhase { Start, Delta, End, Cancel }

    public readonly struct DragCommand : IInputCommand
    {
        public string CommandName => "Drag";
        public float Timestamp { get; }
        public Vector2 StartPosition { get; }
        public Vector2 CurrentPosition { get; }
        public Vector2 Delta { get; }
        public DragPhase Phase { get; }

        public DragCommand(Vector2 startPosition, Vector2 currentPosition, Vector2 delta, DragPhase phase, float timestamp = 0f)
        {
            StartPosition = startPosition;
            CurrentPosition = currentPosition;
            Delta = delta;
            Phase = phase;
            Timestamp = timestamp;
        }
    }

    public enum SwipeDirection { Up, Down, Left, Right }

    public readonly struct SwipeCommand : IInputCommand
    {
        public string CommandName => "Swipe";
        public float Timestamp { get; }
        public Vector2Int OriginGridCoordinate { get; }
        public SwipeDirection Direction { get; }

        public SwipeCommand(Vector2Int origin, SwipeDirection direction, float timestamp = 0f)
        {
            OriginGridCoordinate = origin;
            Direction = direction;
            Timestamp = timestamp;
        }
    }

    public readonly struct LinkCommand : IInputCommand
    {
        public string CommandName => "Link";
        public float Timestamp { get; }
        public System.Collections.Generic.IReadOnlyList<Gameplay.Grid.GridPosition> Path { get; }

        public LinkCommand(System.Collections.Generic.IReadOnlyList<Gameplay.Grid.GridPosition> path, float timestamp = 0f)
        {
            Path = path ?? System.Array.Empty<Gameplay.Grid.GridPosition>();
            Timestamp = timestamp;
        }
    }

    public readonly struct SwapCommand : IInputCommand
    {
        public string CommandName => "Swap";
        public float Timestamp { get; }
        public Gameplay.Grid.GridPosition From { get; }
        public Gameplay.Grid.GridPosition To { get; }

        public SwapCommand(Gameplay.Grid.GridPosition from, Gameplay.Grid.GridPosition to, float timestamp = 0f)
        {
            From = from;
            To = to;
            Timestamp = timestamp;
        }
    }
}
