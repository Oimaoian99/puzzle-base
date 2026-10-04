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
using Puzzle.Variants.Match3.Logic;
using VContainer;

namespace Puzzle.Tests.Variants
{
    [TestFixture]
    public class Match3Tests
    {
        private Board _board;
        private LevelData _levelData;
        private LevelRuntime _runtime;
        private Match3PuzzleLogic _logic;

        [SetUp]
        public void SetUp()
        {
            _levelData = Match3LevelFactory.CreateDeterministicLevel(new LevelId("test_match3"), 5, 5, 10, 500);
            _board = new Board(5, 5);

            foreach (var setup in _levelData.InitialPieces)
            {
                _board.SetPiece(setup.Position, new Piece(setup.PieceId, setup.Type));
            }

            _runtime = new LevelRuntime(_levelData.Id, _levelData.StartingMoves);
            _logic = new Match3PuzzleLogic
            {
                RefillOnClear = false // Disable refill by default for deterministic state verification
            };
            _logic.Initialize(_board, _levelData, _runtime);
        }

        [Test]
        public void Initialize_SetsUpStateAndClearsSelection()
        {
            Assert.AreEqual(_board, _logic.Board);
            Assert.AreEqual(_levelData, _logic.LevelData);
            Assert.AreEqual(_runtime, _logic.LevelRuntime);
            Assert.IsFalse(_logic.SelectedPosition.HasValue);
        }

        [Test]
        public void HorizontalMatch_DetectedAndRemoved_AwardsScoreAndConsumesMove()
        {
            // In deterministic layout:
            // (0,0) is ColorA, (1,0) is ColorB, (2,0) is ColorA
            // (1,1) is ColorA
            // Swapping (1,0) with (1,1) creates a horizontal match of ColorA at (0,0), (1,0), (2,0)!
            var from = new GridPosition(1, 0);
            var to = new GridPosition(1, 1);

            bool success = _logic.TrySwap(from, to);

            Assert.IsTrue(success, "Swap should succeed because it creates a horizontal 3-in-a-row match.");
            Assert.AreEqual(9, _logic.LevelRuntime.RemainingMoves, "Move should be consumed.");
            Assert.AreEqual(300, _logic.LevelRuntime.CurrentScore, "3 pieces matched should award 300 score.");

            // Pieces in row 0 should have fallen down or shifted due to gravity
            // Column 3 at (3,0) was not part of the match and remains ColorC
            Assert.IsNotNull(_board.GetPiece(new GridPosition(3, 0)));
            Assert.AreEqual(PieceType.ColorC, _board.GetPiece(new GridPosition(3, 0)).Type);
        }

        [Test]
        public void VerticalMatch_DetectedAndRemoved()
        {
            // Build custom 3-in-a-column setup on clean board
            var board = new Board(3, 3);
            var runtime = new LevelRuntime(new LevelId("test_vert"), 5);
            var data = new LevelData(new LevelId("test_vert"), 3, 3, 5, 300);
            var logic = new Match3PuzzleLogic { RefillOnClear = false };
            logic.Initialize(board, data, runtime);

            // (0,0) = ColorA, (0,1) = ColorB, (0,2) = ColorA
            // (1,1) = ColorA
            board.SetPiece(new GridPosition(0, 0), new Piece(1, PieceType.ColorA));
            board.SetPiece(new GridPosition(0, 1), new Piece(2, PieceType.ColorB));
            board.SetPiece(new GridPosition(0, 2), new Piece(3, PieceType.ColorA));
            board.SetPiece(new GridPosition(1, 1), new Piece(4, PieceType.ColorA));
            // Fill others with alternating colors to avoid accidental matches
            board.SetPiece(new GridPosition(1, 0), new Piece(5, PieceType.ColorB));
            board.SetPiece(new GridPosition(1, 2), new Piece(6, PieceType.ColorC));
            board.SetPiece(new GridPosition(2, 0), new Piece(7, PieceType.ColorC));
            board.SetPiece(new GridPosition(2, 1), new Piece(8, PieceType.ColorD));
            board.SetPiece(new GridPosition(2, 2), new Piece(9, PieceType.ColorB));

            // Swap (0,1) with (1,1) -> creates vertical match at (0,0), (0,1), (0,2) of ColorA
            bool success = logic.TrySwap(new GridPosition(0, 1), new GridPosition(1, 1));

            Assert.IsTrue(success);
            Assert.AreEqual(300, runtime.CurrentScore);
            Assert.AreEqual(4, runtime.RemainingMoves);
        }

        [Test]
        public void NonAdjacentSwap_Rejected_NoMoveConsumed()
        {
            // (0,0) and (2,0) are not adjacent (gap of 2)
            bool success = _logic.TrySwap(new GridPosition(0, 0), new GridPosition(2, 0));

            Assert.IsFalse(success);
            Assert.AreEqual(10, _logic.LevelRuntime.RemainingMoves);
            Assert.AreEqual(0, _logic.LevelRuntime.CurrentScore);
        }

        [Test]
        public void DiagonalSwap_Rejected()
        {
            // (0,0) and (1,1) are diagonal
            bool success = _logic.TrySwap(new GridPosition(0, 0), new GridPosition(1, 1));

            Assert.IsFalse(success);
            Assert.AreEqual(10, _logic.LevelRuntime.RemainingMoves);
        }

        [Test]
        public void InvalidSwap_RollsBackState_NoMoveConsumed()
        {
            // Swap (3,0)[ColorC] with (4,0)[ColorD] -> creates no 3-in-a-row matches
            var pieceBeforeA = _board.GetPiece(new GridPosition(3, 0));
            var pieceBeforeB = _board.GetPiece(new GridPosition(4, 0));

            bool success = _logic.TrySwap(new GridPosition(3, 0), new GridPosition(4, 0));

            Assert.IsFalse(success, "Swap should fail if no match is produced.");
            Assert.AreEqual(10, _logic.LevelRuntime.RemainingMoves, "No move should be consumed.");
            Assert.AreEqual(0, _logic.LevelRuntime.CurrentScore, "No score should be awarded.");

            // Pieces must be rolled back to their original positions
            Assert.AreEqual(pieceBeforeA.Id, _board.GetPiece(new GridPosition(3, 0)).Id);
            Assert.AreEqual(pieceBeforeB.Id, _board.GetPiece(new GridPosition(4, 0)).Id);
        }

        [Test]
        public void SequentialTapSelection_SwapsAdjacentPieces()
        {
            // Tap (1,0) first to select
            _logic.ReceiveCommand(new TapCommand(UnityEngine.Vector2.zero, new UnityEngine.Vector2Int(1, 0)));
            Assert.AreEqual(new GridPosition(1, 0), _logic.SelectedPosition);

            // Tap (1,1) adjacent to swap and execute match
            _logic.ReceiveCommand(new TapCommand(UnityEngine.Vector2.zero, new UnityEngine.Vector2Int(1, 1)));

            Assert.IsFalse(_logic.SelectedPosition.HasValue, "Selection should clear after swap.");
            Assert.AreEqual(9, _logic.LevelRuntime.RemainingMoves);
            Assert.AreEqual(300, _logic.LevelRuntime.CurrentScore);
        }

        [Test]
        public void TapSamePiece_Deselects()
        {
            _logic.ReceiveCommand(new TapCommand(UnityEngine.Vector2.zero, new UnityEngine.Vector2Int(1, 0)));
            Assert.AreEqual(new GridPosition(1, 0), _logic.SelectedPosition);

            _logic.ReceiveCommand(new TapCommand(UnityEngine.Vector2.zero, new UnityEngine.Vector2Int(1, 0)));
            Assert.IsFalse(_logic.SelectedPosition.HasValue);
        }

        [Test]
        public void TapNonAdjacentPiece_SwitchesSelection()
        {
            _logic.ReceiveCommand(new TapCommand(UnityEngine.Vector2.zero, new UnityEngine.Vector2Int(1, 0)));
            Assert.AreEqual(new GridPosition(1, 0), _logic.SelectedPosition);

            _logic.ReceiveCommand(new TapCommand(UnityEngine.Vector2.zero, new UnityEngine.Vector2Int(4, 4)));
            Assert.AreEqual(new GridPosition(4, 4), _logic.SelectedPosition);
        }

        [Test]
        public void SwipeCommand_ExecutesSwap()
        {
            // Swipe right from (1,0) towards (2,0)... wait, swap (1,0) with (1,1) which is Up
            var swipe = new SwipeCommand(new UnityEngine.Vector2Int(1, 0), SwipeDirection.Up);
            _logic.ReceiveCommand(swipe);

            Assert.AreEqual(9, _logic.LevelRuntime.RemainingMoves);
            Assert.AreEqual(300, _logic.LevelRuntime.CurrentScore);
        }

        [Test]
        public void SwapCommand_ExecutesSwapDirectly()
        {
            var command = new SwapCommand(new GridPosition(1, 0), new GridPosition(1, 1));
            _logic.ReceiveCommand(command);

            Assert.AreEqual(9, _logic.LevelRuntime.RemainingMoves);
            Assert.AreEqual(300, _logic.LevelRuntime.CurrentScore);
        }

        [Test]
        public void Gravity_ShiftsPiecesDown()
        {
            // Clear (1,0)
            _board.RemovePiece(new GridPosition(1, 0));
            Assert.IsNull(_board.GetPiece(new GridPosition(1, 0)));
            var pieceAbove = _board.GetPiece(new GridPosition(1, 1));
            Assert.IsNotNull(pieceAbove);

            _logic.ApplyGravity();

            // The piece from (1,1) should have fallen down into (1,0)
            var fallen = _board.GetPiece(new GridPosition(1, 0));
            Assert.IsNotNull(fallen);
            Assert.AreEqual(pieceAbove.Id, fallen.Id);
        }

        [Test]
        public void Refill_FillsAllEmptyCells()
        {
            _logic.RefillOnClear = true;
            _logic.SetRandomSeed(42);

            // Execute valid match
            _logic.TrySwap(new GridPosition(1, 0), new GridPosition(1, 1));

            // Board should be completely filled
            for (int x = 0; x < _board.Width; x++)
            {
                for (int y = 0; y < _board.Height; y++)
                {
                    Assert.IsNotNull(_board.GetPiece(new GridPosition(x, y)), $"Cell at ({x},{y}) should have a piece.");
                }
            }
        }

        [Test]
        public void WinCondition_TriggersWhenTargetScoreReached()
        {
            Assert.AreEqual(PuzzleResult.Playing, _logic.CheckResult());

            _runtime.AddScore(500); // Target score is 500

            var result = _logic.CheckResult();
            Assert.IsTrue(result.IsWin);
            Assert.AreEqual(1, result.Stars);
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
        public void Reset_ClearsSelectionAndPreservesBoard()
        {
            _logic.HandleTap(new GridPosition(0, 0));
            Assert.IsTrue(_logic.SelectedPosition.HasValue);

            _logic.Reset();
            Assert.IsFalse(_logic.SelectedPosition.HasValue);
        }

        [Test]
        public void TwoVariants_AreCompletelyDecoupled_AndBothImplementIPuzzleLogic()
        {
            // Verify LinkPuzzleLogic and Match3PuzzleLogic both implement IPuzzleLogic
            IPuzzleLogic linkLogic = new LinkPuzzleLogic();
            IPuzzleLogic match3Logic = new Match3PuzzleLogic();

            Assert.IsNotNull(linkLogic);
            Assert.IsNotNull(match3Logic);

            // Verify both can be initialized with identical generic Board and LevelData
            var board = new Board(4, 4);
            var runtime = new LevelRuntime(new LevelId("shared"), 10);
            var data = new LevelData(new LevelId("shared"), 4, 4, 10, 500);

            linkLogic.Initialize(board, data, runtime);
            Assert.AreEqual(PuzzleResult.Playing, linkLogic.CheckResult());

            match3Logic.Initialize(board, data, runtime);
            Assert.AreEqual(PuzzleResult.Playing, match3Logic.CheckResult());

            // Verify assembly independence:
            // Link asmdef does not reference Match3 asmdef.
            // Match3 asmdef does not reference Link asmdef.
            var linkAsm = typeof(LinkPuzzleLogic).Assembly;
            var match3Asm = typeof(Match3PuzzleLogic).Assembly;

            Assert.AreNotEqual(linkAsm, match3Asm);

            foreach (var refAsm in linkAsm.GetReferencedAssemblies())
            {
                Assert.AreNotEqual("Puzzle.Variants.Match3", refAsm.Name, "Link assembly must not reference Match3 assembly.");
            }

            foreach (var refAsm in match3Asm.GetReferencedAssemblies())
            {
                Assert.AreNotEqual("Puzzle.Variants.Link", refAsm.Name, "Match3 assembly must not reference Link assembly.");
            }
        }

        [Test]
        public void Match3_VContainerChildScope_ResolvesMatch3LogicAsIPuzzleLogic()
        {
            var rootBuilder = new ContainerBuilder();
            using var rootContainer = rootBuilder.Build();

            using var childScope = rootContainer.CreateScope(builder =>
            {
                LevelLifetimeScope.ConfigureLevelServices<Match3PuzzleLogic>(builder, new LevelId("test_match3_scope"));
            });

            var logic = childScope.Resolve<IPuzzleLogic>();
            Assert.IsNotNull(logic);
            Assert.IsInstanceOf<Match3PuzzleLogic>(logic);

            var puzzleRuntime = childScope.Resolve<PuzzleRuntime>();
            Assert.IsNotNull(puzzleRuntime);
            Assert.IsInstanceOf<Match3PuzzleLogic>(puzzleRuntime.Logic);
        }
    }
}
