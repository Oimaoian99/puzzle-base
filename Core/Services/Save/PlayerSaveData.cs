using System;
using System.Collections.Generic;

namespace Puzzle.Core.Services
{
    /// <summary>
    /// Progress record for an individual level.
    /// Pure serializable POCO data model.
    /// </summary>
    [Serializable]
    public class LevelRecord
    {
        public string LevelId;
        public int Stars;
        public int HighScore;
        public bool IsCompleted;

        public LevelRecord() { }

        public LevelRecord(string levelId, int stars, int highScore, bool isCompleted)
        {
            LevelId = levelId ?? string.Empty;
            Stars = stars;
            HighScore = highScore;
            IsCompleted = isCompleted;
        }
    }

    /// <summary>
    /// Persistent player save data structure (Tier 4 Data Architecture).
    /// Completely separated from transient runtime state (LevelRuntime, Board, Piece)
    /// and static authoring data (LevelData).
    /// </summary>
    [Serializable]
    public class PlayerSaveData
    {
        public int SaveVersion = 1;
        public int Coins = 0;
        public int HighestCompletedLevelIndex = 0;
        public List<LevelRecord> LevelRecords = new List<LevelRecord>();

        public LevelRecord FindRecord(string levelId)
        {
            if (string.IsNullOrEmpty(levelId) || LevelRecords == null) return null;
            for (int i = 0; i < LevelRecords.Count; i++)
            {
                if (string.Equals(LevelRecords[i].LevelId, levelId, StringComparison.OrdinalIgnoreCase))
                {
                    return LevelRecords[i];
                }
            }
            return null;
        }
    }
}
