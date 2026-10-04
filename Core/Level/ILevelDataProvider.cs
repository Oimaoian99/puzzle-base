using Puzzle.Core.Gameplay.Data;

namespace Puzzle.Core.Level
{
    /// <summary>
    /// Content boundary abstraction for retrieving authored LevelData.
    /// Decouples Game Flow and Level Loading from specific storage backends
    /// (ScriptableObjects, In-Memory DTOs, Local Resources, or Addressables).
    /// </summary>
    public interface ILevelDataProvider
    {
        bool HasLevel(LevelId id);
        bool TryGetLevelData(LevelId id, out LevelData levelData);
        LevelData GetLevelData(LevelId id);
    }
}
