using System;
using System.Collections.Generic;
using Puzzle.Core.Gameplay.Data;
using UnityEngine;

namespace Puzzle.Core.Level
{
    /// <summary>
    /// ScriptableObject catalog storing references to authored LevelDataSO assets.
    /// Implements ILevelDataProvider to bridge Unity assets to the Core content pipeline.
    /// In future milestones, this can be swapped with AddressablesLevelDataProvider
    /// without any changes to Game Flow or gameplay logic.
    /// </summary>
    [CreateAssetMenu(fileName = "LevelRepository", menuName = "Puzzle/Levels/Level Repository")]
    public class ScriptableObjectLevelRepository : ScriptableObject, ILevelDataProvider
    {
        [SerializeField] private List<LevelDataSO> levels = new List<LevelDataSO>();

        public IReadOnlyList<LevelDataSO> Levels => levels;

        public void SetLevels(IEnumerable<LevelDataSO> newLevels)
        {
            levels = new List<LevelDataSO>(newLevels ?? Array.Empty<LevelDataSO>());
        }

        public bool HasLevel(LevelId id)
        {
            if (id.IsEmpty) return false;

            for (int i = 0; i < levels.Count; i++)
            {
                if (levels[i] != null && string.Equals(levels[i].LevelId.Value, id.Value, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        public bool TryGetLevelData(LevelId id, out LevelData levelData)
        {
            levelData = null;
            if (id.IsEmpty) return false;

            for (int i = 0; i < levels.Count; i++)
            {
                var asset = levels[i];
                if (asset != null && string.Equals(asset.LevelId.Value, id.Value, StringComparison.OrdinalIgnoreCase))
                {
                    levelData = asset.ToLevelData();
                    return true;
                }
            }

            return false;
        }

        public LevelData GetLevelData(LevelId id)
        {
            if (TryGetLevelData(id, out var data))
            {
                return data;
            }

            throw new KeyNotFoundException($"Level '{id}' not found in ScriptableObjectLevelRepository.");
        }
    }
}
