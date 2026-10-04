using Puzzle.Core.Gameplay.Data;
using UnityEngine;

namespace Puzzle.Variants.Match3.Data
{
    /// <summary>
    /// ScriptableObject for authoring Match-3 puzzle levels in the Unity Inspector.
    /// Designers set up board size, moves, target score, pieces, minimum match length, and refill behavior.
    /// Right click in Project: Create > Puzzle > Levels > Match-3 Level
    /// </summary>
    [CreateAssetMenu(fileName = "Match3Level_001", menuName = "Puzzle/Levels/Match-3 Level")]
    public class Match3LevelDataSO : LevelDataSO
    {
        [Header("Match-3 Rules")]
        [SerializeField, Range(3, 5)] private int minMatchLength = 3;
        [SerializeField] private bool refillOnClear = true;

        public int MinMatchLength => minMatchLength;
        public bool RefillOnClear => refillOnClear;

        public override string VariantId => "Match3";

        public override LevelData ToLevelData()
        {
            var data = new Match3LevelData
            {
                MinMatchLength = minMatchLength,
                RefillOnClear = refillOnClear
            };

            PopulateBaseLevelData(data);
            return data;
        }

        public void SetMatch3Config(int matchLength, bool refill)
        {
            minMatchLength = matchLength;
            refillOnClear = refill;
        }
    }
}
