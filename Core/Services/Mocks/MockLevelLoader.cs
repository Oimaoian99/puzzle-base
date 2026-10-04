using System;
using Puzzle.Core.Level;

namespace Puzzle.Core.Services.Mocks
{
    public class MockLevelLoader : ILevelLoader
    {
        public bool IsLoading { get; private set; }
        public LevelId ActiveLevelId { get; private set; }
        public ILevelRuntime ActiveRuntime { get; private set; }

        public bool SimulateFailure { get; set; }
        public string FailureMessage { get; set; } = "Simulated level load failure.";
        public int DefaultStartingMoves { get; set; } = 25;

        public int LoadCallCount { get; private set; }
        public int UnloadCallCount { get; private set; }

        public void LoadLevel(LevelId id, Action<ILevelRuntime> onLoaded, Action<string> onError)
        {
            LoadCallCount++;
            ActiveLevelId = id;

            if (SimulateFailure)
            {
                IsLoading = false;
                onError?.Invoke(FailureMessage);
                return;
            }

            IsLoading = false;
            ActiveRuntime = new LevelRuntime(id, DefaultStartingMoves);
            onLoaded?.Invoke(ActiveRuntime);
        }

        public void UnloadLevel(LevelId id = default)
        {
            UnloadCallCount++;
            ActiveLevelId = LevelId.Empty;
            ActiveRuntime = null;
        }
    }
}
