using NUnit.Framework;
using Puzzle.Core.Level;

namespace Puzzle.Tests.Core.EditMode
{
    [TestFixture]
    public class LevelLifecycleTests
    {
        private LevelRuntime _runtime;

        [SetUp]
        public void SetUp()
        {
            _runtime = new LevelRuntime(new LevelId("level_test"), startingMoves: 20);
        }

        [Test]
        public void InitialState_IsReady_AndHasConfiguredMoves()
        {
            Assert.AreEqual(LevelLifecycleState.Ready, _runtime.State);
            Assert.AreEqual(20, _runtime.RemainingMoves);
            Assert.AreEqual(0, _runtime.CurrentScore);
            Assert.IsFalse(_runtime.IsFinished);
        }

        [Test]
        public void AddScore_IncrementsScoreAndFiresEvent()
        {
            int updatedScore = 0;
            _runtime.OnScoreChanged += s => updatedScore = s;

            _runtime.AddScore(150);
            Assert.AreEqual(150, _runtime.CurrentScore);
            Assert.AreEqual(150, updatedScore);

            _runtime.AddScore(50);
            Assert.AreEqual(200, _runtime.CurrentScore);
            Assert.AreEqual(200, updatedScore);
        }

        [Test]
        public void TryConsumeMoves_DecrementsRemainingMoves()
        {
            int movesLogged = 0;
            _runtime.OnMovesChanged += m => movesLogged = m;

            bool success = _runtime.TryConsumeMoves(2);
            Assert.IsTrue(success);
            Assert.AreEqual(18, _runtime.RemainingMoves);
            Assert.AreEqual(18, movesLogged);
        }

        [Test]
        public void TryConsumeMoves_InsufficientMoves_ReturnsFalse()
        {
            bool success = _runtime.TryConsumeMoves(25);
            Assert.IsFalse(success);
            Assert.AreEqual(20, _runtime.RemainingMoves); // Untouched
        }

        [Test]
        public void CompleteLevel_TransitionsToCompleted_AndDispatchesResult()
        {
            LevelResultData result = null;
            _runtime.OnLevelFinished += r => result = r;

            _runtime.AddScore(500);
            _runtime.TryConsumeMoves(5);
            _runtime.CompleteLevel(stars: 3);

            Assert.AreEqual(LevelLifecycleState.Completed, _runtime.State);
            Assert.IsTrue(_runtime.IsFinished);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsWin);
            Assert.AreEqual(500, result.Score);
            Assert.AreEqual(3, result.StarsEarned);
            Assert.AreEqual(15, result.MovesRemaining);
        }

        [Test]
        public void FailLevel_TransitionsToFailed_AndDispatchesResult()
        {
            LevelResultData result = null;
            _runtime.OnLevelFinished += r => result = r;

            _runtime.FailLevel();

            Assert.AreEqual(LevelLifecycleState.Failed, _runtime.State);
            Assert.IsTrue(_runtime.IsFinished);
            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsWin);
            Assert.AreEqual(0, result.StarsEarned);
        }

        [Test]
        public void FinishedLevel_CannotMutateScoreOrMoves()
        {
            _runtime.CompleteLevel(3);

            _runtime.AddScore(100);
            Assert.AreEqual(0, _runtime.CurrentScore);

            bool moveConsumed = _runtime.TryConsumeMoves(1);
            Assert.IsFalse(moveConsumed);
        }
    }
}
