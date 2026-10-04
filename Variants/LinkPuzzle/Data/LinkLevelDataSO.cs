using Puzzle.Core.Gameplay.Data;
using UnityEngine;

namespace Puzzle.Variants.Link.Data
{
    /// <summary>
    /// ScriptableObject for authoring Link/Line puzzle levels in the Unity Inspector.
    /// Designers set up board size, moves, target score, pieces, and minimum link length.
    /// Right click in Project: Create > Puzzle > Levels > Link Level
    /// </summary>
    [CreateAssetMenu(fileName = "LinkLevel_001", menuName = "Puzzle/Levels/Link Level")]
    public class LinkLevelDataSO : LevelDataSO
    {
        [Header("Link Rules")]
        [SerializeField, Range(2, 8)] private int minLinkLength = 3;

        public int MinLinkLength => minLinkLength;

        public override string VariantId => "Link";

        public override LevelData ToLevelData()
        {
            var data = new LinkLevelData
            {
                MinLinkLength = minLinkLength
            };

            PopulateBaseLevelData(data);
            return data;
        }

        public void SetLinkConfig(int linkLength)
        {
            minLinkLength = linkLength;
        }
    }
}
