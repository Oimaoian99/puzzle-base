using System;
using NUnit.Framework;
using Puzzle.Core.GameFlow;
using Puzzle.Core.GameFlow.States;
using Puzzle.Core.Level;
using Puzzle.Core.Services.Mocks;

namespace Puzzle.Tests.Core.EditMode
{
    [TestFixture]
    public class GameFlowStateTests
    {
        private GameStateMachine _fsm;
        private MockSaveService _saveService;
        private MockAnalyticsService _analyticsService;
        private MockLevelLoader _levelLoader;

        private BootState _bootState;
        private InitState _initState;
        private HomeState _homeState;
        private LevelLoadingState _loadingState;
        private PlayState _playState;
        private ResultState _resultState;
        private PausedState _pausedState;

        [SetUp]
        public void SetUp()
        {
            _fsm = new GameStateMachine();
            _saveService = new MockSaveService();
            _analyticsService = new MockAnalyticsService();
            _levelLoader = new MockLevelLoader();

            _bootState = new BootState(_fsm);
            _initState = new InitState(_fsm, _saveService);
            _homeState = new HomeState(_fsm);
            _loadingState = new LevelLoadingState(_fsm, _levelLoader);
            _playState = new PlayState(_fsm, _analyticsService);
            _resultState = new ResultState(_fsm, _levelLoader, _saveService, _analyticsService);
            _pausedState = new PausedState(_fsm, _levelLoader);

            _fsm.RegisterState(_bootState);
            _fsm.RegisterState(_initState);
            _fsm.RegisterState(_homeState);
            _fsm.RegisterState(_loadingState);
            _fsm.RegisterState(_playState);
            _fsm.RegisterState(_resultState);
            _fsm.RegisterState(_pausedState);
        }

        [Test]
        public void Boot_To_Init_To_Home_AutomaticTransitionsWork()
        {
            // Initial state is None
            Assert.AreEqual(GameStateId.None, _fsm.CurrentStateId);

            // Trigger Boot -> auto-transitions to Init -> auto-transitions to Home
            _fsm.ChangeState(GameStateId.Boot);

            // Should have reached HomeState cleanly
            Assert.AreEqual(GameStateId.Home, _fsm.CurrentStateId);
            Assert.AreEqual(1, _saveService.LoadCallCount);
        }

        [Test]
        public void HomeState_PlayLevel_ConfiguresLoadingState_TransitionsToLevelLoading()
        {
            _fsm.ChangeState(GameStateId.Boot);
            Assert.AreEqual(GameStateId.Home, _fsm.CurrentStateId);

            var targetLevel = new LevelId("level_005");
            _homeState.PlayLevel(targetLevel);

            Assert.AreEqual(GameStateId.Play, _fsm.CurrentStateId); // LevelLoader loads synchronously in mock
            Assert.AreEqual(targetLevel, _loadingState.TargetLevelId);
            Assert.AreEqual(targetLevel, _playState.ActiveRuntime.Id);
            Assert.AreEqual(1, _analyticsService.StartedLevels.Count);
            Assert.AreEqual(targetLevel, _analyticsService.StartedLevels[0]);
        }

        [Test]
        public void LevelLoadingState_EmptyTargetLevel_ReturnsToHome()
        {
            _fsm.ChangeState(GameStateId.Boot);
            Assert.AreEqual(GameStateId.Home, _fsm.CurrentStateId);

            _loadingState.SetTargetLevel(LevelId.Empty);
            _fsm.ChangeState(GameStateId.LevelLoading);

            // Reverts to Home because level ID is empty
            Assert.AreEqual(GameStateId.Home, _fsm.CurrentStateId);
        }

        [Test]
        public void LevelLoadingState_LoaderFailure_RemainsInLoading_AndCanCancelToHome()
        {
            _fsm.ChangeState(GameStateId.Boot);
            _levelLoader.SimulateFailure = true;
            _levelLoader.FailureMessage = "Asset bundle not found.";

            _loadingState.SetTargetLevel(new LevelId("level_error"));
            _fsm.ChangeState(GameStateId.LevelLoading);

            // Remains in LevelLoading so user can see error UI and decide
            Assert.AreEqual(GameStateId.LevelLoading, _fsm.CurrentStateId);
            Assert.IsTrue(_loadingState.HasError);
            Assert.AreEqual("Asset bundle not found.", _loadingState.LastErrorMessage);

            // Cancel out to Home
            _loadingState.CancelToHome();
            Assert.AreEqual(GameStateId.Home, _fsm.CurrentStateId);
        }

        [Test]
        public void PlayState_Win_TransitionsToResultState_AndSavesProgress()
        {
            _fsm.ChangeState(GameStateId.Boot);
            _homeState.PlayLevel(new LevelId("level_010"));

            Assert.AreEqual(GameStateId.Play, _fsm.CurrentStateId);
            var runtime = _playState.ActiveRuntime;

            // Player scores and completes level
            runtime.AddScore(1500);
            runtime.CompleteLevel(stars: 3);

            // PlayState detects completion and transitions to ResultState
            Assert.AreEqual(GameStateId.Result, _fsm.CurrentStateId);
            Assert.IsNotNull(_resultState.ResultData);
            Assert.IsTrue(_resultState.ResultData.IsSuccess);
            Assert.AreEqual(1500, _resultState.ResultData.Score);
            Assert.AreEqual(3, _resultState.ResultData.Stars);

            // Saved progress verified
            Assert.IsTrue(_saveService.IsLevelCompleted(new LevelId("level_010")));
            Assert.AreEqual(3, _saveService.GetStarsEarned(new LevelId("level_010")));
            Assert.AreEqual(1, _saveService.SaveCallCount);
            Assert.AreEqual(1, _analyticsService.CompletedLevels.Count);
        }

        [Test]
        public void PlayState_Fail_TransitionsToResultState_DoesNotSaveCompletion()
        {
            _fsm.ChangeState(GameStateId.Boot);
            _homeState.PlayLevel(new LevelId("level_020"));

            var runtime = _playState.ActiveRuntime;
            runtime.FailLevel();

            Assert.AreEqual(GameStateId.Result, _fsm.CurrentStateId);
            Assert.IsFalse(_resultState.ResultData.IsSuccess);
            Assert.IsFalse(_saveService.IsLevelCompleted(new LevelId("level_020")));
            Assert.AreEqual(0, _saveService.SaveCallCount);
        }

        [Test]
        public void ResultState_NextLevel_UnloadsPrevious_LoadsNextLevel()
        {
            _fsm.ChangeState(GameStateId.Boot);
            _homeState.PlayLevel(new LevelId("level_001"));

            _playState.ActiveRuntime.CompleteLevel(2);
            Assert.AreEqual(GameStateId.Result, _fsm.CurrentStateId);

            var unloadCountBefore = _levelLoader.UnloadCallCount;

            // Player taps Next Level
            _resultState.NextLevel(new LevelId("level_002"));

            // Previous level was unloaded
            Assert.Greater(_levelLoader.UnloadCallCount, unloadCountBefore);
            // New level is playing
            Assert.AreEqual(GameStateId.Play, _fsm.CurrentStateId);
            Assert.AreEqual("level_002", _playState.ActiveRuntime.Id.Value);
        }

        [Test]
        public void ResultState_Retry_UnloadsPrevious_ReloadsSameLevel()
        {
            _fsm.ChangeState(GameStateId.Boot);
            _homeState.PlayLevel(new LevelId("level_003"));

            _playState.ActiveRuntime.FailLevel();
            Assert.AreEqual(GameStateId.Result, _fsm.CurrentStateId);

            var unloadCountBefore = _levelLoader.UnloadCallCount;

            // Player taps Retry
            _resultState.Retry();

            // Previous level unloaded and reloaded
            Assert.Greater(_levelLoader.UnloadCallCount, unloadCountBefore);
            Assert.AreEqual(GameStateId.Play, _fsm.CurrentStateId);
            Assert.AreEqual("level_003", _playState.ActiveRuntime.Id.Value);
        }

        [Test]
        public void ResultState_ReturnHome_UnloadsLevel_TransitionsToHome()
        {
            _fsm.ChangeState(GameStateId.Boot);
            _homeState.PlayLevel(new LevelId("level_004"));

            _playState.ActiveRuntime.CompleteLevel(3);
            Assert.AreEqual(GameStateId.Result, _fsm.CurrentStateId);

            var unloadCountBefore = _levelLoader.UnloadCallCount;

            _resultState.ReturnHome();

            Assert.Greater(_levelLoader.UnloadCallCount, unloadCountBefore);
            Assert.AreEqual(GameStateId.Home, _fsm.CurrentStateId);
        }

        [Test]
        public void PausedState_ResumeAndExit_BehaveCorrectly()
        {
            _fsm.ChangeState(GameStateId.Boot);
            _homeState.PlayLevel(new LevelId("level_005"));

            _pausedState.SetActiveLevel(new LevelId("level_005"));
            _playState.PauseGame();
            Assert.AreEqual(GameStateId.Paused, _fsm.CurrentStateId);

            // Resume
            _pausedState.Resume();
            Assert.AreEqual(GameStateId.Play, _fsm.CurrentStateId);

            // Pause again and return home
            _playState.PauseGame();
            _pausedState.ReturnHome();
            Assert.AreEqual(GameStateId.Home, _fsm.CurrentStateId);
        }
    }
}
