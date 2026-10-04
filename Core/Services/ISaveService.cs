using Puzzle.Core.Level;

namespace Puzzle.Core.Services
{
    public interface ISaveService
    {
        int GetHighestCompletedLevelIndex();
        void SetLevelCompleted(LevelId levelId, int stars, int score);
        bool IsLevelCompleted(LevelId levelId);
        int GetStarsEarned(LevelId levelId);
        
        int GetCoins();
        void AddCoins(int amount);
        bool TrySpendCoins(int amount);
        
        void Save();
        void Load();
    }
}
