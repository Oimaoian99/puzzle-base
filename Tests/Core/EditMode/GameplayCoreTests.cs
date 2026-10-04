using System;
using NUnit.Framework;
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
using Puzzle.Core.Services.Mocks;
using UnityEngine;

namespace Puzzle.Tests.Core.EditMode
{
    [TestFixture]
    public class GameplayCoreTests
    {
        #region GridPosition Tests

        [Test]
        public void GridPosition_ConstructAndProperties_AreCorrect()
        {
            var pos = new GridPosition(3, 5);
            Assert.AreEqual(3, pos.X);
            Assert.AreEqual(5, pos.Y);
            Assert.AreEqual("(3, 5)", pos.ToString());
        }

        [Test]
        public void GridPosition_EqualityAndOperators_WorkCorrectly()
        {
            var p1 = new GridPosition(2, 4);
            var p2 = new GridPosition(2, 4);
            var p3 = new GridPosition(3, 4);

            Assert.IsTrue(p1 == p2);
            Assert.IsFalse(p1 != p2);
            Assert.IsTrue(p1 != p3);
            Assert.AreEqual(p1.GetHashCode(), p2.GetHashCode());
            Assert.IsTrue(p1.Equals(p2));
            Assert.IsFalse(p1.Equals((object)"not a pos"));
        }

        [Test]
        public void GridPosition_ManhattanDistance_CalculatesAccurately()
        {
            var a = new GridPosition(1, 2);
            var b = new GridPosition(4, 6);

            Assert.AreEqual(7, a.ManhattanDistance(b));
            Assert.AreEqual(7, b.ManhattanDistance(a));
            Assert.AreEqual(0, a.ManhattanDistance(a));
        }

        [Test]
        public void GridPosition_OffsetAndArithmetic_WorkCorrectly()
        {
            var p = new GridPosition(2, 3);
            var moved = p.Offset(1, -1);
            Assert.AreEqual(new GridPosition(3, 2), moved);

            var added = p + new GridPosition(3, 4);
            Assert.AreEqual(new GridPosition(5, 7), added);

            var subtracted = p - new GridPosition(1, 1);
            Assert.AreEqual(new GridPosition(1, 2), subtracted);
        }

        [Test]
        public void GridPosition_Vector2Int_ConversionsWork()
        {
            var pos = new GridPosition(7, 9);
            Vector2Int v = pos.ToVector2Int();
            Assert.AreEqual(7, v.x);
            Assert.AreEqual(9, v.y);

            GridPosition fromVec = (GridPosition)v;
            Assert.AreEqual(pos, fromVec);
        }

        #endregion

        #region Piece and Cell Tests

        [Test]
        public void Piece_Properties_AreSetCorrectly()
        {
            var piece = new Piece(101, PieceType.ColorA, "bonus");
            Assert.AreEqual(101, piece.Id);
            Assert.AreEqual(PieceType.ColorA, piece.Type);
            Assert.AreEqual("bonus", piece.Tag);
            Assert.IsTrue(piece.ToString().Contains("ColorA"));
        }

        [Test]
        public void Cell_InitialState_IsEmptyAndNotBlocked()
        {
            var cell = new Cell(new GridPosition(1, 1));
            Assert.AreEqual(new GridPosition(1, 1), cell.Position);
            Assert.IsTrue(cell.IsEmpty);
            Assert.IsFalse(cell.IsBlocked);
            Assert.IsNull(cell.CurrentPiece);
        }

        [Test]
        public void Cell_PlaceAndRemovePiece_OperatesCorrectly()
        {
            var cell = new Cell(new GridPosition(0, 0));
            var piece = new Piece(1, PieceType.Standard);

            bool placed = cell.PlacePiece(piece);
            Assert.IsTrue(placed);
            Assert.IsFalse(cell.IsEmpty);
            Assert.AreSame(piece, cell.CurrentPiece);

            // Cannot place piece on already occupied cell
            var secondPiece = new Piece(2, PieceType.ColorB);
            bool placedSecond = cell.PlacePiece(secondPiece);
            Assert.IsFalse(placedSecond);
            Assert.AreSame(piece, cell.CurrentPiece);

            // Remove piece
            var removed = cell.RemovePiece();
            Assert.AreSame(piece, removed);
            Assert.IsTrue(cell.IsEmpty);
            Assert.IsNull(cell.CurrentPiece);
        }

        [Test]
        public void Cell_BlockedCell_RejectsPiecePlacement()
        {
            var cell = new Cell(new GridPosition(2, 2)) { IsBlocked = true };
            var piece = new Piece(1, PieceType.Standard);

            bool placed = cell.PlacePiece(piece);
            Assert.IsFalse(placed);
            Assert.IsNull(cell.CurrentPiece);
            Assert.IsTrue(cell.IsBlocked);
        }

        #endregion

        #region Board Tests

        [Test]
        public void Board_Construct_InitializesAllCells()
        {
            var board = new Board(4, 5);
            Assert.AreEqual(4, board.Width);
            Assert.AreEqual(5, board.Height);

            for (int x = 0; x < 4; x++)
            {
                for (int y = 0; y < 5; y++)
                {
                    var cell = board.GetCell(x, y);
                    Assert.IsNotNull(cell);
                    Assert.AreEqual(new GridPosition(x, y), cell.Position);
                    Assert.IsTrue(cell.IsEmpty);
                }
            }
        }

        [Test]
        public void Board_ConstructInvalidDimensions_ThrowsException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Board(0, 5));
            Assert.Throws<ArgumentOutOfRangeException>(() => new Board(5, -1));
        }

        [Test]
        public void Board_IsInside_ValidatesBoundsAccurately()
        {
            var board = new Board(3, 3);
            Assert.IsTrue(board.IsInside(0, 0));
            Assert.IsTrue(board.IsInside(2, 2));
            Assert.IsFalse(board.IsInside(-1, 0));
            Assert.IsFalse(board.IsInside(0, -1));
            Assert.IsFalse(board.IsInside(3, 1));
            Assert.IsFalse(board.IsInside(1, 3));
            Assert.IsNull(board.GetCell(9, 9));
        }

        [Test]
        public void Board_SetGetRemovePiece_WorksAcrossBoard()
        {
            var board = new Board(4, 4);
            var pos = new GridPosition(1, 2);
            var piece = new Piece(42, PieceType.ColorC);

            bool set = board.SetPiece(pos, piece);
            Assert.IsTrue(set);
            Assert.AreSame(piece, board.GetPiece(pos));

            var removed = board.RemovePiece(pos);
            Assert.AreSame(piece, removed);
            Assert.IsNull(board.GetPiece(pos));
        }

        [Test]
        public void Board_Clear_EmptiesAllCells()
        {
            var board = new Board(3, 3);
            board.SetPiece(new GridPosition(0, 0), new Piece(1, PieceType.Standard));
            board.SetPiece(new GridPosition(1, 1), new Piece(2, PieceType.Standard));
            board.GetCell(2, 2).IsBlocked = true;

            board.Clear();

            Assert.IsNull(board.GetPiece(new GridPosition(0, 0)));
            Assert.IsNull(board.GetPiece(new GridPosition(1, 1)));
            Assert.IsFalse(board.GetCell(2, 2).IsBlocked);
        }

        #endregion

        #region PuzzleResult Tests

        [Test]
        public void PuzzleResult_PlayingState_IsNotFinished()
        {
            var result = PuzzleResult.Playing;
            Assert.AreEqual(PuzzleStatus.Playing, result.Status);
            Assert.IsFalse(result.IsFinished);
            Assert.IsFalse(result.IsWin);
            Assert.IsFalse(result.IsLose);
            Assert.AreEqual(0, result.Stars);
        }

        [Test]
        public void PuzzleResult_CreateWin_ClampsStarsCorrectly()
        {
            var win = PuzzleResult.CreateWin(2, "Stage Cleared");
            Assert.IsTrue(win.IsFinished);
            Assert.IsTrue(win.IsWin);
            Assert.IsFalse(win.IsLose);
            Assert.AreEqual(2, win.Stars);
            Assert.AreEqual("Stage Cleared", win.Message);

            // Clamps 0 to 1, and 5 to 3
            var clampedLow = PuzzleResult.CreateWin(0);
            Assert.AreEqual(1, clampedLow.Stars);

            var clampedHigh = PuzzleResult.CreateWin(5);
            Assert.AreEqual(3, clampedHigh.Stars);
        }

        [Test]
        public void PuzzleResult_CreateLose_SetsLossCorrectly()
        {
            var lose = PuzzleResult.CreateLose("Out of moves");
            Assert.IsTrue(lose.IsFinished);
            Assert.IsFalse(lose.IsWin);
            Assert.IsTrue(lose.IsLose);
            Assert.AreEqual(0, lose.Stars);
            Assert.AreEqual("Out of moves", lose.Message);
        }

        [Test]
        public void PuzzleResult_Equality_MatchesCorrectly()
        {
            var r1 = PuzzleResult.CreateWin(3, "Win");
            var r2 = PuzzleResult.CreateWin(3, "Win");
            var r3 = PuzzleResult.CreateLose("Lose");

            Assert.IsTrue(r1 == r2);
            Assert.IsTrue(r1 != r3);
            Assert.AreEqual(r1.GetHashCode(), r2.GetHashCode());
        }

        #endregion

        #region PuzzleRuntime Tests

        [Test]
        public void PuzzleRuntime_InitializesFromLevelData()
        {
            var levelData = new LevelData(new LevelId("level_001"), 6, 6, 20);
            levelData.BlockedCells.Add(new GridPosition(0, 0));
            levelData.InitialPieces.Add(new InitialPieceSetup(new GridPosition(1, 1), PieceType.ColorA, 10, "tag1"));

            var mockLogic = new MockPuzzleLogic();
            var levelRuntime = new LevelRuntime(levelData.Id, levelData.StartingMoves);
            var runtime = new PuzzleRuntime(levelData, mockLogic, levelRuntime);

            Assert.AreEqual(6, runtime.Board.Width);
            Assert.AreEqual(6, runtime.Board.Height);
            Assert.IsTrue(runtime.Board.GetCell(0, 0).IsBlocked);

            var piece = runtime.Board.GetPiece(new GridPosition(1, 1));
            Assert.IsNotNull(piece);
            Assert.AreEqual(PieceType.ColorA, piece.Type);
            Assert.AreEqual(10, piece.Id);
            Assert.AreEqual("tag1", piece.Tag);

            Assert.AreSame(runtime.Board, mockLogic.Board);
            Assert.AreSame(levelData, mockLogic.LevelData);
            Assert.AreSame(levelRuntime, mockLogic.LevelRuntime);
        }

        [Test]
        public void PuzzleRuntime_HandleInput_ForwardsToLogicAndConsumesMoves()
        {
            var levelData = LevelData.CreateDefault(new LevelId("level_001"), 4, 4, 15);
            var mockLogic = new MockPuzzleLogic { ScorePerInput = 250 };
            var levelRuntime = new LevelRuntime(levelData.Id, levelData.StartingMoves);
            levelRuntime.TransitionState(LevelLifecycleState.Playing);

            var runtime = new PuzzleRuntime(levelData, mockLogic, levelRuntime);

            var tap = new TapCommand(new Vector2(100, 100), new Vector2Int(1, 1));
            runtime.HandleInput(tap);

            Assert.AreEqual(1, mockLogic.ReceivedCommands.Count);
            Assert.IsInstanceOf<TapCommand>(mockLogic.ReceivedCommands[0]);
            var receivedTap = (TapCommand)mockLogic.ReceivedCommands[0];
            Assert.AreEqual(tap.GridCoordinate, receivedTap.GridCoordinate);
            Assert.AreEqual(250, levelRuntime.CurrentScore);
            Assert.AreEqual(14, levelRuntime.RemainingMoves);
        }

        [Test]
        public void PuzzleRuntime_Tick_UpdatesTimerAndLogic()
        {
            var levelData = LevelData.CreateDefault(new LevelId("level_001"), 4, 4, 10);
            var mockLogic = new MockPuzzleLogic();
            var levelRuntime = new LevelRuntime(levelData.Id, levelData.StartingMoves);
            levelRuntime.TransitionState(LevelLifecycleState.Playing);

            var runtime = new PuzzleRuntime(levelData, mockLogic, levelRuntime);

            runtime.Tick(0.5f);
            runtime.Tick(0.25f);

            Assert.AreEqual(2, mockLogic.TickCount);
            Assert.AreEqual(0.75f, levelRuntime.ElapsedTime, 0.001f);
        }

        [Test]
        public void PuzzleRuntime_WinCondition_CompletesLevel()
        {
            var levelData = LevelData.CreateDefault(new LevelId("level_001"), 4, 4, 10);
            var mockLogic = new MockPuzzleLogic
            {
                TriggerWinOnInput = true,
                WinStars = 3
            };
            var levelRuntime = new LevelRuntime(levelData.Id, levelData.StartingMoves);
            levelRuntime.TransitionState(LevelLifecycleState.Playing);

            var runtime = new PuzzleRuntime(levelData, mockLogic, levelRuntime);

            runtime.HandleInput(new TapCommand(Vector2.zero, Vector2Int.zero));

            Assert.IsTrue(runtime.IsFinished);
            Assert.IsTrue(runtime.CurrentResult.IsWin);
            Assert.AreEqual(LevelLifecycleState.Completed, levelRuntime.State);

            // Inputs after finish are ignored
            runtime.HandleInput(new TapCommand(Vector2.one, Vector2Int.one));
            Assert.AreEqual(1, mockLogic.ReceivedCommands.Count);
        }

        [Test]
        public void PuzzleRuntime_LoseCondition_FailsLevel()
        {
            var levelData = LevelData.CreateDefault(new LevelId("level_001"), 4, 4, 10);
            var mockLogic = new MockPuzzleLogic
            {
                TriggerLoseOnInput = true
            };
            var levelRuntime = new LevelRuntime(levelData.Id, levelData.StartingMoves);
            levelRuntime.TransitionState(LevelLifecycleState.Playing);

            var runtime = new PuzzleRuntime(levelData, mockLogic, levelRuntime);

            runtime.HandleInput(new TapCommand(Vector2.zero, Vector2Int.zero));

            Assert.IsTrue(runtime.IsFinished);
            Assert.IsTrue(runtime.CurrentResult.IsLose);
            Assert.AreEqual(LevelLifecycleState.Failed, levelRuntime.State);
        }

        [Test]
        public void PuzzleRuntime_Dispose_CleansBoardAndResetsLogic()
        {
            var levelData = LevelData.CreateDefault(new LevelId("level_001"), 4, 4, 10);
            levelData.InitialPieces.Add(new InitialPieceSetup(new GridPosition(0, 0), PieceType.Obstacle));
            var mockLogic = new MockPuzzleLogic();
            var levelRuntime = new LevelRuntime(levelData.Id, levelData.StartingMoves);

            var runtime = new PuzzleRuntime(levelData, mockLogic, levelRuntime);
            Assert.IsNotNull(runtime.Board.GetPiece(new GridPosition(0, 0)));

            runtime.Dispose();

            Assert.IsNull(runtime.Board.GetPiece(new GridPosition(0, 0)));
            Assert.AreEqual(0, mockLogic.ReceivedCommands.Count);
        }

        #endregion

        #region PlayState Integration Tests

        [Test]
        public void PlayState_WithPuzzleRuntime_InputForwardsAndTransitionsToResultState()
        {
            var stateMachine = new GameStateMachine();
            var mockAnalytics = new MockAnalyticsService();
            var mockLoader = new MockLevelLoader();
            var playState = new PlayState(stateMachine, mockAnalytics);
            var resultState = new ResultState(stateMachine, mockLoader);

            stateMachine.RegisterState(playState);
            stateMachine.RegisterState(resultState);

            var levelData = LevelData.CreateDefault(new LevelId("level_play_test"), 4, 4, 5);
            var mockLogic = new MockPuzzleLogic
            {
                TriggerWinOnInput = true,
                WinStars = 2
            };
            var levelRuntime = new LevelRuntime(levelData.Id, levelData.StartingMoves);
            levelRuntime.TransitionState(LevelLifecycleState.Playing);

            var puzzleRuntime = new PuzzleRuntime(levelData, mockLogic, levelRuntime);

            playState.SetPuzzleRuntime(puzzleRuntime);
            Assert.AreSame(puzzleRuntime, playState.ActivePuzzleRuntime);
            Assert.AreSame(levelRuntime, playState.ActiveRuntime);

            stateMachine.ChangeState(GameStateId.Play);
            Assert.AreEqual(GameStateId.Play, stateMachine.CurrentStateId);

            // Send input to PlayState
            playState.ReceiveInput(new TapCommand(Vector2.zero, Vector2Int.zero));

            // Should have evaluated win, finished level, and transitioned to ResultState
            Assert.AreEqual(GameStateId.Result, stateMachine.CurrentStateId);
            Assert.IsNotNull(resultState.ResultData);
            Assert.IsTrue(resultState.ResultData.IsSuccess);
            Assert.AreEqual(2, resultState.ResultData.Stars);

            // Clean up
            puzzleRuntime.Dispose();
        }

        #endregion
    }
}
