using Puzzle.Core.Level;

namespace Puzzle.Core.Data
{
    public interface ILevelDataProvider
    {
        bool HasLevel(LevelId id);
        LevelId GetNextLevelId(LevelId current);
        int TotalLevelCount { get; }
    }

    public interface ISaveDataProvider
    {
        string LoadRawData();
        void SaveRawData(string serializedPayload);
        bool HasSavedData();
        void ClearData();
    }
}
