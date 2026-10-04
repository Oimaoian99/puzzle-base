using NUnit.Framework;
using Puzzle.Composition;
using Puzzle.Core.Level;
using Puzzle.Core.Services;
using VContainer;

namespace Puzzle.Tests.Composition
{
    [TestFixture]
    public class VContainerLifetimeTests
    {
        private IObjectResolver _rootContainer;

        [SetUp]
        public void SetUp()
        {
            var builder = new ContainerBuilder();
            RootLifetimeScope.ConfigureRootServices(builder);
            _rootContainer = builder.Build();
        }

        [TearDown]
        public void TearDown()
        {
            _rootContainer.Dispose();
        }

        [Test]
        public void LevelScope_Disposal_DisposesLevelObjects_AndPreservesRoot()
        {
            LevelSessionTracker trackerInstance;
            var saveService = _rootContainer.Resolve<ISaveService>();
            if (saveService is FileSaveService fileSave)
            {
                fileSave.ResetSave();
            }
            saveService.AddCoins(250);

            // Session 1: Run Level 1
            using (var levelScope = _rootContainer.CreateScope(builder =>
            {
                LevelLifetimeScope.ConfigureLevelServices(builder, new LevelId("level_01"), 20);
            }))
            {
                trackerInstance = levelScope.Resolve<LevelSessionTracker>();
                Assert.IsFalse(trackerInstance.IsDisposed);

                trackerInstance.CompleteSession(stars: 3);
                Assert.IsTrue(saveService.IsLevelCompleted(new LevelId("level_01")));
            }

            // Scope is now disposed: verify level-scoped tracker is marked disposed
            Assert.IsTrue(trackerInstance.IsDisposed);

            // Root services survived! Coins and progress are intact!
            Assert.AreEqual(250, saveService.GetCoins());
            Assert.IsTrue(saveService.IsLevelCompleted(new LevelId("level_01")));
        }

        [Test]
        public void SuccessiveLevelScopes_ProduceIndependentRuntimes_NoStateLeakage()
        {
            ILevelRuntime runtime1;
            ILevelRuntime runtime2;

            using (var scope1 = _rootContainer.CreateScope(builder =>
            {
                LevelLifetimeScope.ConfigureLevelServices(builder, new LevelId("level_01"), 15);
            }))
            {
                runtime1 = scope1.Resolve<ILevelRuntime>();
                runtime1.AddScore(500);
                runtime1.TryConsumeMoves(5);
                Assert.AreEqual(500, runtime1.CurrentScore);
                Assert.AreEqual(10, runtime1.RemainingMoves);
            }

            using (var scope2 = _rootContainer.CreateScope(builder =>
            {
                LevelLifetimeScope.ConfigureLevelServices(builder, new LevelId("level_02"), 25);
            }))
            {
                runtime2 = scope2.Resolve<ILevelRuntime>();
                // Session 2 is completely fresh!
                Assert.AreEqual(0, runtime2.CurrentScore);
                Assert.AreEqual(25, runtime2.RemainingMoves);
                Assert.AreNotSame(runtime1, runtime2);
            }
        }
    }
}
