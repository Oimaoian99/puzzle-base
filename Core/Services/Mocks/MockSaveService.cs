using System;
using System.Collections.Generic;
using Puzzle.Core.Level;

namespace Puzzle.Core.Services.Mocks
{
    public class MockSaveService : ISaveService
    {
        private readonly Dictionary<string, int> _levelStars = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, int> _levelScores = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        private int _highestCompletedLevelIndex = 0;
        private int _coins = 0;

        public int SaveCallCount { get; private set; }

        public int GetHighestCompletedLevelIndex() => _highestCompletedLevelIndex;

        public void SetLevelCompleted(LevelId levelId, int stars, int score)
        {
            _levelStars[levelId.Value] = Math.Max(GetStarsEarned(levelId), stars);
            _levelScores[levelId.Value] = Math.Max(_levelScores.TryGetValue(levelId.Value, out var s) ? s : 0, score);

            if (levelId.TryGetIntIndex(out var idx))
            {
                if (idx > _highestCompletedLevelIndex)
                {
                    _highestCompletedLevelIndex = idx;
                }
            }
        }

        public bool IsLevelCompleted(LevelId levelId)
        {
            return _levelStars.ContainsKey(levelId.Value);
        }

        public int GetStarsEarned(LevelId levelId)
        {
            return _levelStars.TryGetValue(levelId.Value, out var stars) ? stars : 0;
        }

        public int GetCoins() => _coins;

        public void AddCoins(int amount)
        {
            if (amount > 0) _coins += amount;
        }

        public bool TrySpendCoins(int amount)
        {
            if (amount <= 0 || _coins < amount) return false;
            _coins -= amount;
            return true;
        }

        public int LoadCallCount { get; private set; }

        public void Save()
        {
            SaveCallCount++;
        }

        public void Load()
        {
            LoadCallCount++;
        }
    }
}
