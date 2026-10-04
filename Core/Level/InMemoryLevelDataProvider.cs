using System;
using System.Collections.Generic;
using Puzzle.Core.Gameplay.Data;

namespace Puzzle.Core.Level
{
    /// <summary>
    /// In-memory level data provider for unit testing, headless automation,
    /// and dynamic runtime level registration without requiring Unity asset bundles.
    /// </summary>
    public class InMemoryLevelDataProvider : ILevelDataProvider
    {
        private readonly Dictionary<string, LevelData> _levels = new Dictionary<string, LevelData>(StringComparer.OrdinalIgnoreCase);

        public int Count => _levels.Count;

        public void RegisterLevel(LevelData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (data.Id.IsEmpty) throw new ArgumentException("Level data has an empty ID.", nameof(data));

            _levels[data.Id.Value] = data;
        }

        public bool RemoveLevel(LevelId id)
        {
            if (id.IsEmpty) return false;
            return _levels.Remove(id.Value);
        }

        public void Clear()
        {
            _levels.Clear();
        }

        public bool HasLevel(LevelId id)
        {
            if (id.IsEmpty) return false;
            return _levels.ContainsKey(id.Value);
        }

        public bool TryGetLevelData(LevelId id, out LevelData levelData)
        {
            if (id.IsEmpty)
            {
                levelData = null;
                return false;
            }

            return _levels.TryGetValue(id.Value, out levelData);
        }

        public LevelData GetLevelData(LevelId id)
        {
            if (TryGetLevelData(id, out var data))
            {
                return data;
            }

            throw new KeyNotFoundException($"Level '{id}' not found in InMemoryLevelDataProvider.");
        }
    }
}
