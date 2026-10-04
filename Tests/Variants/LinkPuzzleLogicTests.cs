using System.Collections.Generic;
using NUnit.Framework;
using Puzzle.Composition;
using Puzzle.Core.GameFlow;
using Puzzle.Core.GameFlow.States;
using Puzzle.Core.Gameplay.Board;
using Puzzle.Core.Gameplay.Data;
using Puzzle.Core.Gameplay.Grid;
using Puzzle.Core.Gameplay.Pieces;
using Puzzle.Core.Gameplay.Puzzle;
using Puzzle.Core.Gameplay.Runtime;
using Puzzle.Core.Input.Commands;
using Puzzle.Core.Level;
using Puzzle.Variants.Link.Logic;
using VContainer;

namespace Puzzle.Tests.Variants
{
    [TestFixture]
    public class LinkPuzzleLogicTests
    {
        private Board _board;
        private LevelData _levelData;
        private LevelRuntime _runtime;
        private LinkPuzzleLogic _logic;

        [SetUp]
        public void SetUp()
        {
            _levelData = LinkLevelFactory.CreateDeterministicLevel(new LevelId("test_link"), 4, 4, 10, 500);
            _board = new Board(4, 4);

            // Populate board from deterministic level
            foreach (var setup in _levelData.InitialPieces)
            {
                _board.SetPiece(setup.Position, new Piece(setup.PieceId, setup.Type));
            }

            _runtime = new LevelRuntime(_levelData.Id, _levelData.StartingMoves);
            _logic = new LinkPuzzleLogic
            {
                RefillOnClear = false // Disable refill by default for deterministic test verification
            };
            _logic.Initialize(_board, _levelData, _runtime);
        }

        [Test]
        public void Initialize_SetsUpStateAndResetsPath()
        {
            Assert.AreEqual(_board, _logic.Board);
            Assert.AreEqual(_levelData, _logic.LevelData);
            Assert.AreEqual(_runtime, _logic.LevelRuntime);
            Assert.AreEqual(0, _logic.CurrentPath.Count);
        }

        [Test]
        public void Valid3PieceLink_RemovesPieces_AwardsScore_ConsumesMove()
        {
            // Row 0 has 3 ColorA pieces at (0,0), (1,0), (2,0)
            var path = new[]
            {
                new GridPosition(0, 0),
                new GridPosition(1, 0),
                new GridPosition(2, 0)
            };

            bool success = _logic.TryExecuteLink(path);

            Assert.IsTrue(success);
            Assert.AreEqual(1, _logic.LevelRuntime.RemainingMoves < 10 ? 10 - _logic.LevelRuntime.RemainingMoves : 0);
            Assert.AreEqual(9, _logic.LevelRuntime.RemainingMoves);
            Assert.AreEqual(300, _logic.LevelRuntime.CurrentScore); // 3 * 100

            // Gravity shifted Row 1 pieces (ColorB) down to Row 0
            Assert.AreEqual(PieceType.ColorB, _board.GetPiece(new GridPosition(0, 0)).Type);
            Assert.AreEqual(PieceType.ColorB, _board.GetPiece(new GridPosition(1, 0)).Type);
            Assert.AreEqual(PieceType.ColorB, _board.GetPiece(new GridPosition(2, 0)).Type);

            // Row 3 cells for columns 0, 1, 2 are now vacated (null) because RefillOnClear is false
            Assert.IsNull(_board.GetPiece(new GridPosition(0, 3)));
            Assert.IsNull(_board.GetPiece(new GridPosition(1, 3)));
            Assert.IsNull(_board.GetPiece(new GridPosition(2, 3)));

            // (3,0) was not part of the chain and remains ColorB
            Assert.IsNotNull(_board.GetPiece(new GridPosition(3, 0)));
            Assert.AreEqual(PieceType.ColorB, _board.GetPiece(new GridPosition(3, 0)).Type);
        }

        [Test]
        public void ChainTooShort_RejectsMove_NoMoveConsumed()
        {
            var path = new[]
            {
                new GridPosition(0, 0),
                new GridPosition(1, 0)
            };

            bool success = _logic.TryExecuteLink(path);

            Assert.IsFalse(success);
            Assert.AreEqual(10, _logic.LevelRuntime.RemainingMoves);
            Assert.AreEqual(0, _logic.LevelRuntime.CurrentScore);
            Assert.IsNotNull(_board.GetPiece(new GridPosition(0, 0)));
        }

        [Test]
        public void NonAdjacentChain_RejectsMove()
        {
            // (0,0) and (2,0) are not adjacent (gap at 1,0)
            var path = new[]
            {
                new GridPosition(0, 0),
                new GridPosition(2, 0),
                new GridPosition(1, 0)
            };

            bool success = _logic.TryExecuteLink(path);

            Assert.IsFalse(success);
            Assert.AreEqual(10, _logic.LevelRuntime.RemainingMoves);
        }

        [Test]
        public void MismatchedTypes_RejectsMove()
        {
            // (0,0) is ColorA, (1,0) is ColorA, (1,1) is ColorB
            var path = new[]
            {
                new GridPosition(0, 0),
                new GridPosition(1, 0),
                new GridPosition(1, 1)
            };

            bool success = _logic.TryExecuteLink(path);

            Assert.IsFalse(success);
            Assert.AreEqual(10, _logic.LevelRuntime.RemainingMoves);
        }

        [Test]
        public void DuplicateCellsInPath_RejectsMove()
        {
            // Backtracking to (0,0) creating duplicate
            var path = new[]
            {
                new GridPosition(0, 0),
                new GridPosition(1, 0),
                new GridPosition(0, 0)
            };

            bool success = _logic.TryExecuteLink(path);

            Assert.IsFalse(success);
            Assert.AreEqual(10, _logic.LevelRuntime.RemainingMoves);
        }

        [Test]
        public void OutOfBoundsCell_RejectsMove()
        {
            var path = new[]
            {
                new GridPosition(0, 0),
                new GridPosition(1, 0),
                new GridPosition(4, 0) // Out of bounds on 4x4
            };

            bool success = _logic.TryExecuteLink(path);

            Assert.IsFalse(success);
        }

        [Test]
        public void TapChaining_SequentialSelectionAndConfirmation()
        {
            // Tap (0,0)
            _logic.ReceiveCommand(new TapCommand(UnityEngine.Vector2.zero, new UnityEngine.Vector2Int(0, 0)));
            Assert.AreEqual(1, _logic.CurrentPath.Count);

            // Tap (1,0)
            _logic.ReceiveCommand(new TapCommand(UnityEngine.Vector2.zero, new UnityEngine.Vector2Int(1, 0)));
            Assert.AreEqual(2, _logic.CurrentPath.Count);

            // Tap (2,0)
            _logic.ReceiveCommand(new TapCommand(UnityEngine.Vector2.zero, new UnityEngine.Vector2Int(2, 0)));
            Assert.AreEqual(3, _logic.CurrentPath.Count);

            // Tapping the last piece (2,0) again confirms and submits the chain
            _logic.ReceiveCommand(new TapCommand(UnityEngine.Vector2.zero, new UnityEngine.Vector2Int(2, 0)));

            Assert.AreEqual(0, _logic.CurrentPath.Count); // Path cleared after execution
            Assert.AreEqual(9, _logic.LevelRuntime.RemainingMoves);
            Assert.AreEqual(300, _logic.LevelRuntime.CurrentScore);
        }

        [Test]
        public void TapBacktrack_RemovesPreviousPiece()
        {
            // Tap (0,0) -> (1,0)
            _logic.ReceiveCommand(new TapCommand(UnityEngine.Vector2.zero, new UnityEngine.Vector2Int(0, 0)));
            _logic.ReceiveCommand(new TapCommand(UnityEngine.Vector2.zero, new UnityEngine.Vector2Int(1, 0)));
            Assert.AreEqual(2, _logic.CurrentPath.Count);

            // Tap (0,0) again -> backtracks and removes (1,0)
            _logic.ReceiveCommand(new TapCommand(UnityEngine.Vector2.zero, new UnityEngine.Vector2Int(0, 0)));
            Assert.AreEqual(1, _logic.CurrentPath.Count);
            Assert.AreEqual(new GridPosition(0, 0), _logic.CurrentPath[0]);
        }

        [Test]
        public void TapInvalidPiece_ResetsSelectionToNewPiece()
        {
            // Tap (0,0) [ColorA]
            _logic.ReceiveCommand(new TapCommand(UnityEngine.Vector2.zero, new UnityEngine.Vector2Int(0, 0)));
            Assert.AreEqual(1, _logic.CurrentPath.Count);

            // Tap (3,3) [ColorA but not adjacent]
            _logic.ReceiveCommand(new TapCommand(UnityEngine.Vector2.zero, new UnityEngine.Vector2Int(3, 3)));
            Assert.AreEqual(1, _logic.CurrentPath.Count);
            Assert.AreEqual(new GridPosition(3, 3), _logic.CurrentPath[0]);
        }

        [Test]
        public void Gravity_ShiftsPiecesDownIntoEmptyCells()
        {
            // Clear (0,0)
            _board.RemovePiece(new GridPosition(0, 0));
            Assert.IsNull(_board.GetPiece(new GridPosition(0, 0)));
            Assert.IsNotNull(_board.GetPiece(new GridPosition(0, 1))); // (0,1) is ColorB

            _logic.RefillOnClear = false;
            _logic.ApplyGravityAndRefill();

            // (0,1) piece should have fallen down to (0,0)
            var fallenPiece = _board.GetPiece(new GridPosition(0, 0));
            Assert.IsNotNull(fallenPiece);
            Assert.AreEqual(PieceType.ColorB, fallenPiece.Type);
        }

        [Test]
        public void Refill_SpawnsNewPiecesInVacatedCells()
        {
            _logic.RefillOnClear = true;
            _logic.SetRandomSeed(1234);

            var path = new[]
            {
                new GridPosition(0, 0),
                new GridPosition(1, 0),
                new GridPosition(2, 0)
            };

            bool success = _logic.TryExecuteLink(path);
            Assert.IsTrue(success);

            // With refill enabled, all cells should be occupied
            for (int x = 0; x < 4; x++)
            {
                for (int y = 0; y < 4; y++)
                {
                    Assert.IsNotNull(_board.GetPiece(new GridPosition(x, y)), $"Cell at ({x},{y}) should be refilled.");
                }
            }
        }

        [Test]
        public void WinCondition_TriggersWhenScoreReachesTarget()
        {
            Assert.AreEqual(PuzzleResult.Playing, _logic.CheckResult());

            // Target score is 500. Add 500 score directly
            _runtime.AddScore(500);

            var result = _logic.CheckResult();
            Assert.IsTrue(result.IsWin);
            Assert.AreEqual(1, result.Stars);
        }

        [Test]
        public void WinStars_ScaleWithScoreMultipliers()
        {
            // Target is 500
            // 2 stars >= 750 (1.5x)
            _runtime.AddScore(750);
            var result = _logic.CheckResult();
            Assert.IsTrue(result.IsWin);
            Assert.AreEqual(2, result.Stars);

            // 3 stars >= 1000 (2.0x)
            _runtime.AddScore(250); // Total 1000
            result = _logic.CheckResult();
            Assert.IsTrue(result.IsWin);
            Assert.AreEqual(3, result.Stars);
        }

        [Test]
        public void LoseCondition_TriggersWhenMovesDepletedWithoutTargetScore()
        {
            _runtime.TryConsumeMoves(10); // Deplete all 10 moves
            Assert.AreEqual(0, _runtime.RemainingMoves);

            var result = _logic.CheckResult();
            Assert.IsTrue(result.IsLose);
            Assert.IsFalse(result.IsWin);
        }

        [Test]
        public void IntegrationWithPuzzleRuntimeAndPlayState()
        {
            var sm = new GameStateMachine();
            var playState = new PlayState(sm);
            var resultState = new ResultState(sm, null);
            sm.RegisterState(playState);
            sm.RegisterState(resultState);

            var runtime = new PuzzleRuntime(_levelData, _logic, _runtime);
            playState.SetPuzzleRuntime(runtime);

            sm.ChangeState(GameStateId.Play);
            Assert.AreEqual(GameStateId.Play, sm.CurrentStateId);

            // Execute link command through playState.ReceiveInput
            var command = new LinkCommand(new[]
            {
                new GridPosition(0, 0),
                new GridPosition(1, 0),
                new GridPosition(2, 0)
            });

            playState.ReceiveInput(command);

            Assert.AreEqual(300, _runtime.CurrentScore);
            Assert.AreEqual(9, _runtime.RemainingMoves);

            // Add score to trigger win condition
            _runtime.AddScore(200); // 300 + 200 = 500 (Target reached)
            playState.Update(0.1f); // Ticks and evaluates result

            // Should have transitioned to ResultState!
            Assert.AreEqual(GameStateId.Result, sm.CurrentStateId);
            Assert.IsTrue(resultState.ResultData.IsSuccess);
        }

        [Test]
        public void VContainerChildScope_ResolvesLinkLogicAsIPuzzleLogic()
        {
            var rootBuilder = new ContainerBuilder();
            using var rootContainer = rootBuilder.Build();

            using var childScope = rootContainer.CreateScope(builder =>
            {
                LevelLifetimeScope.ConfigureLevelServices<LinkPuzzleLogic>(builder, new LevelId("test_link_scope"));
            });

            var logic = childScope.Resolve<IPuzzleLogic>();
            Assert.IsNotNull(logic);
            Assert.IsInstanceOf<LinkPuzzleLogic>(logic);

            var puzzleRuntime = childScope.Resolve<PuzzleRuntime>();
            Assert.IsNotNull(puzzleRuntime);
            Assert.IsInstanceOf<LinkPuzzleLogic>(puzzleRuntime.Logic);
        }
    }
}
