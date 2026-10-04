using NUnit.Framework;
using Puzzle.Composition;
using Puzzle.Core.Gameplay.Board;
using Puzzle.Core.Gameplay.Data;
using Puzzle.Core.Gameplay.Grid;
using Puzzle.Core.Gameplay.Pieces;
using Puzzle.Core.Gameplay.Puzzle;
using Puzzle.Core.Gameplay.Runtime;
using Puzzle.Core.Input.Commands;
using Puzzle.Core.Level;
using UnityEngine;
using VContainer;

namespace Puzzle.Tests.Composition
{
    [TestFixture]
    public class LevelScopeGameplayTests
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
            _rootContainer?.Dispose();
        }

        [Test]
        public void LevelScope_ResolvesAllGameplayCoreDependencies()
        {
            using var levelScope = _rootContainer.CreateScope(builder =>
            {
                LevelLifetimeScope.ConfigureLevelServices(builder, new LevelId("level_m4_01"), 20);
            });

            var levelData = levelScope.Resolve<LevelData>();
            var levelRuntime = levelScope.Resolve<ILevelRuntime>();
            var puzzleLogic = levelScope.Resolve<IPuzzleLogic>();
            var puzzleRuntime = levelScope.Resolve<PuzzleRuntime>();
            var board = levelScope.Resolve<Board>();

            Assert.IsNotNull(levelData);
            Assert.IsNotNull(levelRuntime);
            Assert.IsNotNull(puzzleLogic);
            Assert.IsNotNull(puzzleRuntime);
            Assert.IsNotNull(board);

            Assert.AreEqual("level_m4_01", levelRuntime.Id.Value);
            Assert.AreEqual(20, levelRuntime.RemainingMoves);
            Assert.AreEqual(8, board.Width);
            Assert.AreEqual(8, board.Height);
            Assert.AreSame(board, puzzleRuntime.Board);
            Assert.AreSame(puzzleLogic, puzzleRuntime.Logic);
            Assert.AreSame(levelRuntime, puzzleRuntime.LevelRuntime);
        }

        [Test]
        public void LevelScope_PuzzleRuntime_ExecutesInputAndUpdatesLevelRuntime()
        {
            using var levelScope = _rootContainer.CreateScope(builder =>
            {
                LevelLifetimeScope.ConfigureLevelServices(builder, new LevelId("level_m4_02"), 15);
            });

            var levelRuntime = levelScope.Resolve<ILevelRuntime>();
            var puzzleRuntime = levelScope.Resolve<PuzzleRuntime>();

            levelRuntime.TransitionState(LevelLifecycleState.Playing);

            // Handle input
            puzzleRuntime.HandleInput(new TapCommand(new Vector2(50, 50), new Vector2Int(0, 0)));

            Assert.AreEqual(100, levelRuntime.CurrentScore);
            Assert.AreEqual(14, levelRuntime.RemainingMoves);
        }

        [Test]
        public void LevelScope_CustomLevelData_ConfiguresCustomBoardAndPieces()
        {
            var customData = new LevelData(new LevelId("level_custom"), 5, 5, 12, 500);
            customData.BlockedCells.Add(new GridPosition(0, 0));
            customData.InitialPieces.Add(new InitialPieceSetup(new GridPosition(2, 2), PieceType.ColorA, 7, "gem"));

            using var levelScope = _rootContainer.CreateScope(builder =>
            {
                LevelLifetimeScope.ConfigureLevelServices(builder, customData.Id, customData.StartingMoves, customData);
            });

            var board = levelScope.Resolve<Board>();
            var runtime = levelScope.Resolve<PuzzleRuntime>();

            Assert.AreEqual(5, board.Width);
            Assert.AreEqual(5, board.Height);
            Assert.IsTrue(board.GetCell(0, 0).IsBlocked);

            var piece = board.GetPiece(new GridPosition(2, 2));
            Assert.IsNotNull(piece);
            Assert.AreEqual(PieceType.ColorA, piece.Type);
            Assert.AreEqual("gem", piece.Tag);
        }

        [Test]
        public void LevelScope_Disposal_CleansUpPuzzleRuntimeAndBoard()
        {
            Board boardRef;
            PuzzleRuntime runtimeRef;

            using (var levelScope = _rootContainer.CreateScope(builder =>
            {
                LevelLifetimeScope.ConfigureLevelServices(builder, new LevelId("level_dispose_test"), 10);
            }))
            {
                runtimeRef = levelScope.Resolve<PuzzleRuntime>();
                boardRef = levelScope.Resolve<Board>();

                boardRef.SetPiece(new GridPosition(1, 1), new Piece(1, PieceType.Standard));
                Assert.IsNotNull(boardRef.GetPiece(new GridPosition(1, 1)));
            }

            // After scope disposal, PuzzleRuntime.Dispose() has cleared the board
            Assert.IsNull(boardRef.GetPiece(new GridPosition(1, 1)));
        }
    }
}
