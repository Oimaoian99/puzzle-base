using System.Collections.Generic;
using Puzzle.Core.Level;

namespace Puzzle.Core.Services.Mocks
{
    public class MockAnalyticsService : IAnalyticsService
    {
        public struct LoggedEvent
        {
            public string Name;
            public Dictionary<string, object> Parameters;
        }

        public List<LoggedEvent> EventLog { get; } = new List<LoggedEvent>();
        public List<LevelId> StartedLevels { get; } = new List<LevelId>();
        public List<LevelResultData> CompletedLevels { get; } = new List<LevelResultData>();
        public List<LevelResultData> FailedLevels { get; } = new List<LevelResultData>();

        public void LogEvent(string eventName, Dictionary<string, object> parameters = null)
        {
            EventLog.Add(new LoggedEvent { Name = eventName, Parameters = parameters });
        }

        public void LogLevelStarted(LevelId levelId)
        {
            StartedLevels.Add(levelId);
            LogEvent("level_started", new Dictionary<string, object> { { "level_id", levelId.Value } });
        }

        public void LogLevelCompleted(LevelResultData result)
        {
            CompletedLevels.Add(result);
            LogEvent("level_completed", new Dictionary<string, object>
            {
                { "level_id", result.LevelId.Value },
                { "score", result.Score },
                { "stars", result.StarsEarned }
            });
        }

        public void LogLevelFailed(LevelResultData result)
        {
            FailedLevels.Add(result);
            LogEvent("level_failed", new Dictionary<string, object>
            {
                { "level_id", result.LevelId.Value },
                { "duration", result.DurationSeconds }
            });
        }
    }
}
