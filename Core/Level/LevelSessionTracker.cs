using System;
using Puzzle.Core.Services;

namespace Puzzle.Core.Level
{
    /// <summary>
    /// Core domain coordinator for an active level session.
    /// Demonstrates pure constructor injection: depends only on abstractions,
    /// with ZERO dependencies on VContainer or any DI framework.
    /// </summary>
    public class LevelSessionTracker : IDisposable
    {
        public ILevelRuntime Runtime { get; }
        public IAnalyticsService Analytics { get; }
        public ISaveService Save { get; }
        public bool IsDisposed { get; private set; }

        public LevelSessionTracker(ILevelRuntime runtime, IAnalyticsService analytics, ISaveService save)
        {
            Runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
            Analytics = analytics ?? throw new ArgumentNullException(nameof(analytics));
            Save = save ?? throw new ArgumentNullException(nameof(save));
        }

        public void CompleteSession(int stars)
        {
            if (IsDisposed) throw new ObjectDisposedException(nameof(LevelSessionTracker));
            
            Runtime.CompleteLevel(stars);
            Analytics.LogLevelCompleted(new LevelResultData(
                Runtime.Id, 
                true, 
                Runtime.CurrentScore, 
                stars, 
                Runtime.ElapsedTime, 
                Runtime.RemainingMoves
            ));
            Save.SetLevelCompleted(Runtime.Id, stars, Runtime.CurrentScore);
            Save.Save();
        }

        public void Dispose()
        {
            IsDisposed = true;
        }
    }
}
