using System;
using Puzzle.Core.Gameplay.Data;
using Puzzle.Core.Level;

namespace Puzzle.Variants.Link.Data
{
    /// <summary>
    /// Specialized level configuration for Link/Line puzzle sessions.
    /// Extends core LevelData with Link-specific connection parameters.
    /// Pure C# data model.
    /// </summary>
    [Serializable]
    public class LinkLevelData : LevelData
    {
        public int MinLinkLength { get; set; } = 3;

        static LinkLevelData()
        {
            LevelValidator.RegisterVariantValidator("Link", (data, name, result) =>
            {
                if (data is LinkLevelData linkData)
                {
                    if (linkData.MinLinkLength < 2)
                    {
                        result.AddError($"Level {name}: Link puzzle MinLinkLength must be at least 2 (got {linkData.MinLinkLength}).");
                    }
                }
            });
        }

        public LinkLevelData()
        {
            VariantId = "Link";
        }

        public LinkLevelData(
            LevelId id,
            int width,
            int height,
            int startingMoves = 15,
            int targetScore = 1000,
            int minLinkLength = 3)
            : base(id, width, height, startingMoves, targetScore, "Link")
        {
            MinLinkLength = minLinkLength;
        }
    }
}
