using System.Collections.Generic;
using Puzzle.Core.Level;

namespace Puzzle.Core.Services
{
    public interface IAnalyticsService
    {
        void LogEvent(string eventName, Dictionary<string, object> parameters = null);
        void LogLevelStarted(LevelId levelId);
        void LogLevelCompleted(LevelResultData result);
        void LogLevelFailed(LevelResultData result);
    }
}
