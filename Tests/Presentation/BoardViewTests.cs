using NUnit.Framework;
using Puzzle.Core.Gameplay.Board;
using Puzzle.Core.Gameplay.Data;
using Puzzle.Core.Gameplay.Grid;
using Puzzle.Core.Gameplay.Pieces;
using Puzzle.Core.Gameplay.Runtime;
using Puzzle.Core.Input;
using Puzzle.Core.Input.Commands;
using Puzzle.Core.Level;
using Puzzle.Core.Services.Mocks;
using Puzzle.Tests.Presentation.Mocks;
using UnityEngine;

namespace Puzzle.Tests.Presentation
{
    [TestFixture]
    public class BoardViewTests
    {
        private MockBoardView _boardView;

        [SetUp]
        public void SetUp()
        {
            _boardView = new MockBoardView
            {
                CellSpacing = new Vector2(1f, 1f),
                Origin = Vector2.zero
            };
        }

        [TearDown]
        public void TearDown()
        {
            _boardView?.Unbind();
        }

        [Test]
        public void Board_CanExistAndMutate_WithoutPresentation()
        {
            var board = new Board(5, 5);
            var piece = new Piece(10, PieceType.Standard);
            var posA = new GridPosition(1, 1);
            var posB = new GridPosition(2, 2);

            // Set, Move, Remove entirely headless without Unity presentation
            Assert.IsTrue(board.SetPiece(posA, piece));
            Assert.AreSame(piece, board.GetPiece(posA));

            Assert.IsTrue(board.MovePiece(posA, posB));
            Assert.IsNull(board.GetPiece(posA));
            Assert.AreSame(piece, board.GetPiece(posB));

            var removed = board.RemovePiece(posB);
            Assert.AreSame(piece, removed);
            Assert.IsNull(board.GetPiece(posB));
        }

        [Test]
        public void BoardView_Bind_SpawnsInitialPieceViews()
        {
            var board = new Board(4, 4);
            var p1 = new Piece(1, PieceType.ColorA);
            var p2 = new Piece(2, PieceType.ColorB);

            board.SetPiece(new GridPosition(0, 0), p1);
            board.SetPiece(new GridPosition(2, 3), p2);

            _boardView.Bind(board);

            Assert.AreEqual(2, _boardView.ActivePieceViewCount);
            Assert.IsTrue(_boardView.HasPieceView(1));
            Assert.IsTrue(_boardView.HasPieceView(2));

            var view1 = _boardView.GetPieceView(1);
            Assert.IsNotNull(view1);
            Assert.AreEqual(new GridPosition(0, 0), view1.GridPosition);
            Assert.AreEqual(new Vector3(0f, 0f, 0f), view1.Position);

            var view2 = _boardView.GetPieceView(2);
            Assert.IsNotNull(view2);
            Assert.AreEqual(new GridPosition(2, 3), view2.GridPosition);
            Assert.AreEqual(new Vector3(2f, 3f, 0f), view2.Position);
        }

        [Test]
        public void BoardView_OnPiecePlaced_DynamicallySpawnsPieceView()
        {
            var board = new Board(4, 4);
            _boardView.Bind(board);
            Assert.AreEqual(0, _boardView.ActivePieceViewCount);

            var piece = new Piece(42, PieceType.ColorC);
            board.SetPiece(new GridPosition(1, 2), piece);

            Assert.AreEqual(1, _boardView.ActivePieceViewCount);
            Assert.IsTrue(_boardView.HasPieceView(42));

            var view = _boardView.GetPieceView(42);
            Assert.AreEqual(new GridPosition(1, 2), view.GridPosition);
            Assert.AreEqual(new Vector3(1f, 2f, 0f), view.Position);
        }

        [Test]
        public void BoardView_OnPieceRemoved_DynamicallyRemovesPieceView()
        {
            var board = new Board(4, 4);
            var piece = new Piece(77, PieceType.Obstacle);
            board.SetPiece(new GridPosition(3, 3), piece);

            _boardView.Bind(board);
            Assert.IsTrue(_boardView.HasPieceView(77));

            board.RemovePiece(new GridPosition(3, 3));

            Assert.AreEqual(0, _boardView.ActivePieceViewCount);
            Assert.IsFalse(_boardView.HasPieceView(77));
            Assert.IsNull(_boardView.GetPieceView(77));
        }

        [Test]
        public void BoardView_OnPieceMoved_DynamicallyUpdatesPieceView()
        {
            var board = new Board(4, 4);
            var piece = new Piece(99, PieceType.Special);
            board.SetPiece(new GridPosition(0, 1), piece);

            _boardView.Bind(board);
            var view = _boardView.GetPieceView(99);
            Assert.AreEqual(new GridPosition(0, 1), view.GridPosition);

            // Move piece on Board
            bool moved = board.MovePiece(new GridPosition(0, 1), new GridPosition(3, 2));
            Assert.IsTrue(moved);

            // View observes change and updates grid position and target
            Assert.AreEqual(new GridPosition(3, 2), view.GridPosition);
            Assert.AreEqual(new Vector3(3f, 2f, 0f), view.Position);
        }

        [Test]
        public void BoardView_OnBoardCleared_RemovesAllPieceViews()
        {
            var board = new Board(4, 4);
            board.SetPiece(new GridPosition(0, 0), new Piece(1, PieceType.Standard));
            board.SetPiece(new GridPosition(1, 1), new Piece(2, PieceType.Standard));
            board.SetPiece(new GridPosition(2, 2), new Piece(3, PieceType.Standard));

            _boardView.Bind(board);
            Assert.AreEqual(3, _boardView.ActivePieceViewCount);

            board.Clear();

            Assert.AreEqual(0, _boardView.ActivePieceViewCount);
        }

        [Test]
        public void BoardView_Unbind_CleansUpAndDetachesEvents()
        {
            var board = new Board(3, 3);
            board.SetPiece(new GridPosition(0, 0), new Piece(1, PieceType.Standard));

            _boardView.Bind(board);
            Assert.AreEqual(1, _boardView.ActivePieceViewCount);

            _boardView.Unbind();
            Assert.AreEqual(0, _boardView.ActivePieceViewCount);
            Assert.IsNull(_boardView.Board);

            // Board mutation after unbind should not affect BoardView
            board.SetPiece(new GridPosition(1, 1), new Piece(2, PieceType.Standard));
            Assert.AreEqual(0, _boardView.ActivePieceViewCount);
        }

        [Test]
        public void BoardView_InputBoundary_ProducesTapCommand()
        {
            var board = new Board(3, 3);
            var piece = new Piece(15, PieceType.ColorA);
            board.SetPiece(new GridPosition(2, 1), piece);

            IInputCommand receivedCommand = null;
            _boardView.Bind(board, cmd => receivedCommand = cmd);

            var view = (MockPieceView)_boardView.GetPieceView(15);
            Assert.IsNotNull(view);

            // Player taps on the piece view
            view.SimulateClick();

            Assert.IsNotNull(receivedCommand);
            Assert.IsInstanceOf<TapCommand>(receivedCommand);

            var tap = (TapCommand)receivedCommand;
            Assert.AreEqual(2, tap.GridCoordinate.x);
            Assert.AreEqual(1, tap.GridCoordinate.y);
        }

        [Test]
        public void Presentation_ConnectedToGameplay_UpdatesMovesAndScoreOnInput()
        {
            var levelData = LevelData.CreateDefault(new LevelId("level_pres_flow"), 4, 4, 15);
            levelData.InitialPieces.Add(new InitialPieceSetup(new GridPosition(1, 1), PieceType.ColorA, 55));

            var mockLogic = new MockPuzzleLogic { ScorePerInput = 150 };
            var levelRuntime = new LevelRuntime(levelData.Id, levelData.StartingMoves);
            levelRuntime.TransitionState(LevelLifecycleState.Playing);

            var puzzleRuntime = new PuzzleRuntime(levelData, mockLogic, levelRuntime);

            // Bind Presentation to Gameplay
            _boardView.Bind(puzzleRuntime.Board, puzzleRuntime.HandleInput);

            Assert.AreEqual(1, _boardView.ActivePieceViewCount);
            var pieceView = (MockPieceView)_boardView.GetPieceView(55);
            Assert.IsNotNull(pieceView);

            // Player taps the piece view
            pieceView.SimulateClick();

            // Gameplay handled input
            Assert.AreEqual(150, levelRuntime.CurrentScore);
            Assert.AreEqual(14, levelRuntime.RemainingMoves);

            puzzleRuntime.Dispose();
        }

        [Test]
        public void Gameplay_RunsCompleteLevelSimulation_WithoutPresentation()
        {
            // Verifies that pure C# gameplay requires zero presentation to function completely
            var levelData = LevelData.CreateDefault(new LevelId("level_headless"), 4, 4, 10);
            levelData.InitialPieces.Add(new InitialPieceSetup(new GridPosition(1, 1), PieceType.Standard, 100));

            var mockLogic = new MockPuzzleLogic { TriggerWinOnInput = true, WinStars = 3 };
            var levelRuntime = new LevelRuntime(levelData.Id, levelData.StartingMoves);
            levelRuntime.TransitionState(LevelLifecycleState.Playing);

            var runtime = new PuzzleRuntime(levelData, mockLogic, levelRuntime);
            Assert.IsNotNull(runtime.Board.GetPiece(new GridPosition(1, 1)));

            // Play input completely headless
            runtime.HandleInput(new TapCommand(Vector2.zero, new Vector2Int(1, 1)));

            Assert.IsTrue(runtime.IsFinished);
            Assert.IsTrue(runtime.CurrentResult.IsWin);
            Assert.AreEqual(LevelLifecycleState.Completed, levelRuntime.State);

            runtime.Dispose();
        }
    }
}
