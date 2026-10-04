using NUnit.Framework;
using Puzzle.Composition;
using Puzzle.Core.GameFlow;
using Puzzle.Core.GameFlow.States;
using Puzzle.Core.Level;
using Puzzle.Core.Services;
using VContainer;

namespace Puzzle.Tests.Composition
{
    [TestFixture]
    public class GameFlowCompositionTests
    {
        private IObjectResolver _rootContainer;

        [SetUp]
        public void SetUp()
        {
            var builder = new ContainerBuilder();
            RootLifetimeScope.ConfigureRootServices(builder);
            _rootContainer = builder.Build();

            // In headless/test environment, manually invoke entry point Start
            var starter = _rootContainer.Resolve<GameFlowStarter>();
            starter.Start();
        }

        [TearDown]
        public void TearDown()
        {
            _rootContainer.Dispose();
        }

        [Test]
        public void GameFlowStarter_BootsIntoHomeState_ViaVContainerRoot()
        {
            var fsm = _rootContainer.Resolve<IGameStateMachine>();

            // Entry point has registered states and reached HomeState
            Assert.AreEqual(GameStateId.Home, fsm.CurrentStateId);
            Assert.IsInstanceOf<HomeState>(fsm.CurrentState);

            var saveService = _rootContainer.Resolve<ISaveService>();
            Assert.IsNotNull(saveService);
        }

        [Test]
        public void LevelScopeLoader_CreatesChildScopeAndScopedRuntime()
        {
            var loader = (LevelScopeLoader)_rootContainer.Resolve<ILevelLoader>();
            Assert.IsFalse(loader.HasActiveScope);

            ILevelRuntime loadedRuntime = null;
            loader.LoadLevel(new LevelId("level_vcontainer_1"), runtime => loadedRuntime = runtime, null);

            Assert.IsNotNull(loadedRuntime);
            Assert.AreEqual("level_vcontainer_1", loadedRuntime.Id.Value);
            Assert.IsTrue(loader.HasActiveScope);

            // Verify LevelSessionTracker was registered in the child scope
            var tracker = loader.ActiveScope.Resolve<LevelSessionTracker>();
            Assert.IsNotNull(tracker);
            Assert.AreSame(loadedRuntime, tracker.Runtime);
            Assert.IsFalse(tracker.IsDisposed);
        }

        [Test]
        public void LevelScopeLoader_UnloadLevel_DisposesChildScopeAndTracksDisposal()
        {
            var loader = (LevelScopeLoader)_rootContainer.Resolve<ILevelLoader>();

            loader.LoadLevel(new LevelId("level_vcontainer_2"), null, null);
            var tracker = loader.ActiveScope.Resolve<LevelSessionTracker>();
            Assert.IsFalse(tracker.IsDisposed);

            loader.UnloadLevel();

            Assert.IsFalse(loader.HasActiveScope);
            Assert.IsTrue(tracker.IsDisposed);
        }

        [Test]
        public void EndToEndFlow_HomeToWinToNextLevel_WithRealChildScopes_NoStateLeakage()
        {
            var fsm = _rootContainer.Resolve<IGameStateMachine>();
            var loader = (LevelScopeLoader)_rootContainer.Resolve<ILevelLoader>();
            var homeState = fsm.GetState<HomeState>(GameStateId.Home);
            var resultState = fsm.GetState<ResultState>(GameStateId.Result);
            var saveService = _rootContainer.Resolve<ISaveService>();

            // 1. In HomeState, player chooses level 1
            Assert.AreEqual(GameStateId.Home, fsm.CurrentStateId);
            homeState.PlayLevel(new LevelId("level_001"));

            // 2. State machine transitions to PlayState with active level scope
            Assert.AreEqual(GameStateId.Play, fsm.CurrentStateId);
            Assert.IsTrue(loader.HasActiveScope);
            var trackerLevel1 = loader.ActiveScope.Resolve<LevelSessionTracker>();
            var runtimeLevel1 = trackerLevel1.Runtime;
            Assert.AreEqual("level_001", runtimeLevel1.Id.Value);
            Assert.IsFalse(trackerLevel1.IsDisposed);

            // 3. Player plays and completes level 1 with 3 stars
            runtimeLevel1.AddScore(2500);
            runtimeLevel1.CompleteLevel(3);

            // 4. Transitions to ResultState
            Assert.AreEqual(GameStateId.Result, fsm.CurrentStateId);
            Assert.IsTrue(saveService.IsLevelCompleted(new LevelId("level_001")));
            Assert.AreEqual(3, saveService.GetStarsEarned(new LevelId("level_001")));

            // 5. Player taps Next Level ("level_002")
            resultState.NextLevel(new LevelId("level_002"));

            // 6. Old level 1 scope was cleanly disposed!
            Assert.IsTrue(trackerLevel1.IsDisposed);

            // 7. New level 2 is playing with a fresh scope!
            Assert.AreEqual(GameStateId.Play, fsm.CurrentStateId);
            var trackerLevel2 = loader.ActiveScope.Resolve<LevelSessionTracker>();
            var runtimeLevel2 = trackerLevel2.Runtime;

            Assert.AreNotSame(runtimeLevel1, runtimeLevel2);
            Assert.AreEqual("level_002", runtimeLevel2.Id.Value);
            Assert.AreEqual(0, runtimeLevel2.CurrentScore); // Clean fresh state
            Assert.IsFalse(trackerLevel2.IsDisposed);

            // 8. Player wins level 2 and returns home
            runtimeLevel2.CompleteLevel(2);
            resultState.ReturnHome();

            // 9. Reached HomeState and all level scopes are destroyed
            Assert.AreEqual(GameStateId.Home, fsm.CurrentStateId);
            Assert.IsFalse(loader.HasActiveScope);
            Assert.IsTrue(trackerLevel2.IsDisposed);

            // 10. Root services survive with accumulated progress
            Assert.IsTrue(saveService.IsLevelCompleted(new LevelId("level_001")));
            Assert.IsTrue(saveService.IsLevelCompleted(new LevelId("level_002")));
        }

        [Test]
        public void EndToEndFlow_Retry_PreservesRoot_RecreatesFreshLevelScope()
        {
            var fsm = _rootContainer.Resolve<IGameStateMachine>();
            var loader = (LevelScopeLoader)_rootContainer.Resolve<ILevelLoader>();
            var homeState = fsm.GetState<HomeState>(GameStateId.Home);
            var resultState = fsm.GetState<ResultState>(GameStateId.Result);

            homeState.PlayLevel(new LevelId("level_003"));
            var trackerFirst = loader.ActiveScope.Resolve<LevelSessionTracker>();
            trackerFirst.Runtime.AddScore(800);
            trackerFirst.Runtime.FailLevel();

            Assert.AreEqual(GameStateId.Result, fsm.CurrentStateId);

            // Player hits Retry
            resultState.Retry();

            // Old tracker disposed
            Assert.IsTrue(trackerFirst.IsDisposed);

            // Fresh level 3 scope playing
            Assert.AreEqual(GameStateId.Play, fsm.CurrentStateId);
            var trackerSecond = loader.ActiveScope.Resolve<LevelSessionTracker>();
            Assert.AreNotSame(trackerFirst, trackerSecond);
            Assert.AreEqual("level_003", trackerSecond.Runtime.Id.Value);
            Assert.AreEqual(0, trackerSecond.Runtime.CurrentScore);
            Assert.IsFalse(trackerSecond.IsDisposed);
        }
    }
}
