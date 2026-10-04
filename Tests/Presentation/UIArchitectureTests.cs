using NUnit.Framework;
using Puzzle.Core.GameFlow;
using Puzzle.Core.GameFlow.States;
using Puzzle.Core.Level;
using Puzzle.Presentation.UI;
using Puzzle.Tests.Presentation.Mocks;

namespace Puzzle.Tests.Presentation
{
    [TestFixture]
    public class UIArchitectureTests
    {
        private MockUIManager _uiManager;
        private GameStateMachine _fsm;

        [SetUp]
        public void SetUp()
        {
            _uiManager = new MockUIManager();

            _fsm = new GameStateMachine();
            _fsm.RegisterState(new BootState(_fsm, null, false));
            _fsm.RegisterState(new InitState(_fsm, null, null, false));
            _fsm.RegisterState(new HomeState(_fsm));
            _fsm.RegisterState(new LevelLoadingState(() => { }));
            _fsm.RegisterState(new PlayState(_fsm));
            _fsm.RegisterState(new PausedState(_fsm));
            _fsm.RegisterState(new ResultState(() => { }));
        }

        [Test]
        public void UIWindowLayer_EnumOrder_MaintainsHierarchySemantics()
        {
            Assert.AreEqual(0, (int)UIWindowLayer.Screen);
            Assert.AreEqual(1, (int)UIWindowLayer.HUD);
            Assert.AreEqual(2, (int)UIWindowLayer.Popup);
            Assert.AreEqual(3, (int)UIWindowLayer.Overlay);
        }

        [Test]
        public void MockUIManager_ShowHUD_RecordsCallAndMarksVisible()
        {
            var runtime = new LevelRuntime(new LevelId("level_001"), 25);

            _uiManager.ShowHUD(runtime, _fsm, 1000);

            Assert.IsTrue(_uiManager.WasHUDShown);
            Assert.AreSame(runtime, _uiManager.LastBoundRuntime);
            Assert.IsTrue(_uiManager.IsWindowVisible<GameplayHUDView>());
        }

        [Test]
        public void MockUIManager_ShowWin_RecordsScoreAndStars()
        {
            var result = new LevelResultData(new LevelId("level_001"), true, 2500, 3, 30f, 5);

            _uiManager.ShowWin(result, _fsm);

            Assert.IsTrue(_uiManager.WasWinShown);
            Assert.AreEqual(2500, _uiManager.LastResult.Score);
            Assert.AreEqual(3, _uiManager.LastResult.Stars);
            Assert.IsTrue(_uiManager.LastResult.IsWin);
            Assert.IsTrue(_uiManager.IsWindowVisible<WinResultPopupView>());
        }

        [Test]
        public void MockUIManager_ShowLose_RecordsDefeatState()
        {
            var result = new LevelResultData(new LevelId("level_001"), false, 450, 0, 45f, 0);

            _uiManager.ShowLose(result, _fsm);

            Assert.IsTrue(_uiManager.WasLoseShown);
            Assert.IsFalse(_uiManager.LastResult.IsWin);
            Assert.AreEqual(450, _uiManager.LastResult.Score);
            Assert.IsTrue(_uiManager.IsWindowVisible<LoseResultPopupView>());
        }

        [Test]
        public void MockUIManager_ShowPause_RecordsPausePopupVisibility()
        {
            _uiManager.ShowPause(_fsm);

            Assert.IsTrue(_uiManager.WasPauseShown);
            Assert.IsTrue(_uiManager.IsWindowVisible<PausePopupView>());
        }

        [Test]
        public void MockUIManager_ShowHome_RecordsHomeMenuVisibility()
        {
            _uiManager.ShowHome(_fsm);

            Assert.IsTrue(_uiManager.WasHomeShown);
            Assert.IsTrue(_uiManager.IsWindowVisible<HomeScreenView>());
        }

        [Test]
        public void MockUIManager_HideAllPopups_RemovesModalWindowsOnly()
        {
            _uiManager.ShowPause(_fsm);
            _uiManager.ShowWin(new LevelResultData(new LevelId("l1"), true, 100, 2, 10f, 1), _fsm);
            Assert.IsTrue(_uiManager.IsWindowVisible<PausePopupView>());
            Assert.IsTrue(_uiManager.IsWindowVisible<WinResultPopupView>());

            _uiManager.HideAllPopups();

            Assert.IsFalse(_uiManager.IsWindowVisible<PausePopupView>());
            Assert.IsFalse(_uiManager.IsWindowVisible<WinResultPopupView>());
        }

        [Test]
        public void LevelRuntime_EventsNotify_WhenMovesAndScoreChange()
        {
            var runtime = new LevelRuntime(new LevelId("level_test"), 20);
            int notifiedMoves = -1;
            int notifiedScore = -1;

            runtime.OnMovesChanged += moves => notifiedMoves = moves;
            runtime.OnScoreChanged += score => notifiedScore = score;

            Assert.IsTrue(runtime.TryConsumeMoves(1));
            Assert.AreEqual(19, notifiedMoves);
            Assert.AreEqual(19, runtime.RemainingMoves);

            runtime.AddScore(150);
            Assert.AreEqual(150, notifiedScore);
            Assert.AreEqual(150, runtime.CurrentScore);
        }

        [Test]
        public void FSM_TransitionsThroughUIFlow_RemainConsistent()
        {
            // Simulate full user UI navigation through states
            _fsm.ChangeState(GameStateId.Boot);
            _fsm.ChangeState(GameStateId.Init);
            _fsm.ChangeState(GameStateId.Home);
            _uiManager.ShowHome(_fsm);
            Assert.AreEqual(GameStateId.Home, _fsm.CurrentStateId);
            Assert.IsTrue(_uiManager.WasHomeShown);

            // User taps "Play" -> LevelLoading
            _fsm.ChangeState(GameStateId.LevelLoading);
            Assert.AreEqual(GameStateId.LevelLoading, _fsm.CurrentStateId);

            // Level loaded -> PlayState
            var runtime = new LevelRuntime(new LevelId("m3_01"), 20);
            _fsm.ChangeState(GameStateId.Play);
            _uiManager.ShowHUD(runtime, _fsm, 1000);
            Assert.AreEqual(GameStateId.Play, _fsm.CurrentStateId);
            Assert.IsTrue(_uiManager.WasHUDShown);

            // User taps "Pause" -> PausedState
            _fsm.ChangeState(GameStateId.Paused);
            _uiManager.ShowPause(_fsm);
            Assert.AreEqual(GameStateId.Paused, _fsm.CurrentStateId);
            Assert.IsTrue(_uiManager.WasPauseShown);

            // User taps "Resume" -> PlayState
            _fsm.ChangeState(GameStateId.Play);
            Assert.AreEqual(GameStateId.Play, _fsm.CurrentStateId);

            // Level completes -> ResultState
            var winResult = new LevelResultData(new LevelId("m3_01"), true, 1200, 3, 25f, 4);
            _fsm.ChangeState(GameStateId.Result);
            _uiManager.ShowWin(winResult, _fsm);
            Assert.AreEqual(GameStateId.Result, _fsm.CurrentStateId);
            Assert.IsTrue(_uiManager.WasWinShown);
        }
    }
}
