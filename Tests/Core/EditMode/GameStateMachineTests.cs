using System;
using System.Collections.Generic;
using NUnit.Framework;
using Puzzle.Core.GameFlow;
using Puzzle.Core.GameFlow.States;

namespace Puzzle.Tests.Core.EditMode
{
    [TestFixture]
    public class GameStateMachineTests
    {
        private GameStateMachine _fsm;
        private List<string> _transitionLog;

        [SetUp]
        public void SetUp()
        {
            _fsm = new GameStateMachine();
            _transitionLog = new List<string>();

            _fsm.RegisterState(new BootState(() => _transitionLog.Add("BootEnter")));
            _fsm.RegisterState(new InitState(() => _transitionLog.Add("InitEnter")));
            _fsm.RegisterState(new HomeState(() => _transitionLog.Add("HomeEnter")));
            _fsm.RegisterState(new LevelLoadingState(() => _transitionLog.Add("LoadEnter")));
            _fsm.RegisterState(new PlayState(() => _transitionLog.Add("PlayEnter")));
            _fsm.RegisterState(new ResultState(() => _transitionLog.Add("ResultEnter")));
        }

        [Test]
        public void InitialState_IsNone()
        {
            Assert.AreEqual(GameStateId.None, _fsm.CurrentStateId);
            Assert.IsNull(_fsm.CurrentState);
        }

        [Test]
        public void ChangeState_ValidFlow_TransitionsSuccessfully()
        {
            _fsm.ChangeState(GameStateId.Boot);
            Assert.AreEqual(GameStateId.Boot, _fsm.CurrentStateId);

            _fsm.ChangeState(GameStateId.Init);
            Assert.AreEqual(GameStateId.Init, _fsm.CurrentStateId);

            _fsm.ChangeState(GameStateId.Home);
            Assert.AreEqual(GameStateId.Home, _fsm.CurrentStateId);

            _fsm.ChangeState(GameStateId.LevelLoading);
            Assert.AreEqual(GameStateId.LevelLoading, _fsm.CurrentStateId);

            _fsm.ChangeState(GameStateId.Play);
            Assert.AreEqual(GameStateId.Play, _fsm.CurrentStateId);

            _fsm.ChangeState(GameStateId.Result);
            Assert.AreEqual(GameStateId.Result, _fsm.CurrentStateId);

            CollectionAssert.AreEqual(
                new[] { "BootEnter", "InitEnter", "HomeEnter", "LoadEnter", "PlayEnter", "ResultEnter" },
                _transitionLog
            );
        }

        [Test]
        public void ChangeState_InvalidTransition_ThrowsInvalidOperationException()
        {
            _fsm.ChangeState(GameStateId.Boot);

            // Attempting illegal transition: Boot directly to Play
            Assert.Throws<InvalidOperationException>(() => _fsm.ChangeState(GameStateId.Play));
            Assert.AreEqual(GameStateId.Boot, _fsm.CurrentStateId);
        }

        [Test]
        public void ChangeState_SelfTransition_ThrowsInvalidOperationException()
        {
            _fsm.ChangeState(GameStateId.Boot);
            Assert.Throws<InvalidOperationException>(() => _fsm.ChangeState(GameStateId.Boot));
        }

        [Test]
        public void ChangeState_UnregisteredState_ThrowsKeyNotFoundException()
        {
            Assert.Throws<KeyNotFoundException>(() => _fsm.ChangeState(GameStateId.Paused));
        }

        [Test]
        public void OnStateChanged_FiresWithPreviousAndNextState()
        {
            GameStateId prevLogged = GameStateId.None;
            GameStateId nextLogged = GameStateId.None;

            _fsm.OnStateChanged += (prev, next) =>
            {
                prevLogged = prev;
                nextLogged = next;
            };

            _fsm.ChangeState(GameStateId.Boot);
            Assert.AreEqual(GameStateId.None, prevLogged);
            Assert.AreEqual(GameStateId.Boot, nextLogged);

            _fsm.ChangeState(GameStateId.Init);
            Assert.AreEqual(GameStateId.Boot, prevLogged);
            Assert.AreEqual(GameStateId.Init, nextLogged);
        }
    }
}
