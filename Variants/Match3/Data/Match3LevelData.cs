using System;
using Puzzle.Core.Gameplay.Data;
using Puzzle.Core.Level;

namespace Puzzle.Variants.Match3.Data
{
    /// <summary>
    /// Specialized level configuration for Match-3 puzzle sessions.
    /// Extends core LevelData with Match-3-specific match and refill rules.
    /// Pure C# data model.
    /// </summary>
    [Serializable]
    public class Match3LevelData : LevelData
    {
        public int MinMatchLength { get; set; } = 3;
        public bool RefillOnClear { get; set; } = true;

        static Match3LevelData()
        {
            LevelValidator.RegisterVariantValidator("Match3", (data, name, result) =>
            {
                if (data is Match3LevelData m3)
                {
                    if (m3.MinMatchLength < 3)
                    {
                        result.AddError($"Level {name}: Match-3 MinMatchLength must be at least 3 (got {m3.MinMatchLength}).");
                    }
                }
            });
        }

        public Match3LevelData()
        {
            VariantId = "Match3";
        }

        public Match3LevelData(
            LevelId id,
            int width,
            int height,
            int startingMoves = 20,
            int targetScore = 1000,
            int minMatchLength = 3,
            bool refillOnClear = true)
            : base(id, width, height, startingMoves, targetScore, "Match3")
        {
            MinMatchLength = minMatchLength;
            RefillOnClear = refillOnClear;
        }
    }
}
