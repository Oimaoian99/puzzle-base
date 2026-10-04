using System;

namespace Puzzle.Core.Level
{
    [Serializable]
    public class LevelResultData
    {
        public LevelId LevelId { get; set; }
        public bool IsWin { get; set; }
        public int Score { get; set; }
        public int StarsEarned { get; set; } // 0 to 3
        public float DurationSeconds { get; set; }
        public int MovesRemaining { get; set; }

        public bool IsSuccess => IsWin;
        public int Stars => StarsEarned;
        public float ElapsedTime => DurationSeconds;
        public int RemainingMoves => MovesRemaining;

        public LevelResultData() { }

        public LevelResultData(LevelId levelId, bool isWin, int score, int stars, float duration, int movesRemaining)
        {
            LevelId = levelId;
            IsWin = isWin;
            Score = score;
            StarsEarned = Math.Max(0, Math.Min(3, stars));
            DurationSeconds = duration;
            MovesRemaining = movesRemaining;
        }
    }
}
