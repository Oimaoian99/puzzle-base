using System;

namespace Puzzle.Core.Gameplay.Puzzle
{
    /// <summary>
    /// Lifecycle states for an active puzzle evaluation.
    /// </summary>
    public enum PuzzleStatus
    {
        Playing = 0,
        Win = 1,
        Lose = 2
    }

    /// <summary>
    /// Explicit, lightweight puzzle result container returned by IPuzzleLogic.
    /// </summary>
    [Serializable]
    public readonly struct PuzzleResult : IEquatable<PuzzleResult>
    {
        public PuzzleStatus Status { get; }
        public int Stars { get; }
        public string Message { get; }

        public bool IsFinished => Status == PuzzleStatus.Win || Status == PuzzleStatus.Lose;
        public bool IsWin => Status == PuzzleStatus.Win;
        public bool IsLose => Status == PuzzleStatus.Lose;

        public static readonly PuzzleResult Playing = new PuzzleResult(PuzzleStatus.Playing, 0, "Playing");

        public PuzzleResult(PuzzleStatus status, int stars = 0, string message = null)
        {
            Status = status;
            Stars = status == PuzzleStatus.Win ? Math.Max(1, Math.Min(3, stars)) : 0;
            Message = message ?? status.ToString();
        }

        public static PuzzleResult CreateWin(int stars = 3, string message = "Level Won!")
            => new PuzzleResult(PuzzleStatus.Win, stars, message);

        public static PuzzleResult CreateLose(string message = "Level Failed!")
            => new PuzzleResult(PuzzleStatus.Lose, 0, message);

        public bool Equals(PuzzleResult other)
            => Status == other.Status && Stars == other.Stars && Message == other.Message;

        public override bool Equals(object obj)
            => obj is PuzzleResult other && Equals(other);

        public override int GetHashCode()
            => HashCode.Combine((int)Status, Stars, Message);

        public static bool operator ==(PuzzleResult left, PuzzleResult right) => left.Equals(right);
        public static bool operator !=(PuzzleResult left, PuzzleResult right) => !left.Equals(right);

        public override string ToString() => $"PuzzleResult({Status}, Stars={Stars}, Message={Message})";
    }
}
