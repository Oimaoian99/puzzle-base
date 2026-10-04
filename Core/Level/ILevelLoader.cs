using System;

namespace Puzzle.Core.Level
{
    public interface ILevelLoader
    {
        bool IsLoading { get; }
        LevelId ActiveLevelId { get; }

        void LoadLevel(LevelId id, Action<ILevelRuntime> onLoaded, Action<string> onError);
        void UnloadLevel(LevelId id = default);
    }
}
