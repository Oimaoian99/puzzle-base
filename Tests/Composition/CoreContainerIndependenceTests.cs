using NUnit.Framework;
using Puzzle.Core.Level;
using Puzzle.Core.Services.Mocks;

namespace Puzzle.Tests.Composition
{
    [TestFixture]
    public class CoreContainerIndependenceTests
    {
        [Test]
        public void CoreConsumer_CanBeTestedDirectly_WithoutDIContainer()
        {
            // Pure C# Arrange: zero VContainer calls!
            var runtime = new LevelRuntime(new LevelId("standalone_01"), startingMoves: 10);
            var analytics = new MockAnalyticsService();
            var save = new MockSaveService();

            var tracker = new LevelSessionTracker(runtime, analytics, save);

            // Act
            tracker.Runtime.AddScore(250);
            tracker.CompleteSession(stars: 2);

            // Assert
            Assert.AreEqual(250, runtime.CurrentScore);
            Assert.AreEqual(LevelLifecycleState.Completed, runtime.State);
            Assert.IsTrue(save.IsLevelCompleted(new LevelId("standalone_01")));
            Assert.AreEqual(2, save.GetStarsEarned(new LevelId("standalone_01")));
            Assert.AreEqual(1, analytics.CompletedLevels.Count);

            tracker.Dispose();
            Assert.IsTrue(tracker.IsDisposed);
        }
    }
}
