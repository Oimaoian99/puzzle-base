using System;
using System.Collections.Generic;
using NUnit.Framework;
using Puzzle.Core.Gameplay.Board;
using Puzzle.Core.Gameplay.Grid;
using Puzzle.Core.Gameplay.Pieces;
using Puzzle.Core.Services;
using Puzzle.Core.Services.Mocks;
using Puzzle.Presentation.Feedback;
using Puzzle.Presentation.VFX;
using Puzzle.Tests.Presentation.Mocks;
using UnityEngine;

namespace Puzzle.Tests.Presentation
{
    [TestFixture]
    public class FeedbackPresentationTests
    {
        private FeedbackPresenter _presenter;
        private MockAudioService _audioService;
        private MockHapticService _hapticService;
        private MockVfxService _vfxService;

        [SetUp]
        public void SetUp()
        {
            _audioService = new MockAudioService();
            _hapticService = new MockHapticService();
            _vfxService = new MockVfxService();

            _presenter = new FeedbackPresenter(
                _audioService,
                _hapticService,
                _vfxService,
                pos => new Vector3(pos.X * 1.5f, pos.Y * 2.0f, 0f)
            );
        }

        [Test]
        public void FeedbackPresenter_PieceSelected_TriggersAudioHapticAndVfx()
        {
            var pos = new GridPosition(2, 3);
            var evt = FeedbackEvent.PieceSelected(pos, PieceType.ColorA);

            _presenter.HandleFeedback(evt);

            Assert.Contains("piece_select", _audioService.PlayedSfxLog);
            Assert.Contains(HapticFeedbackType.Light, _hapticService.TriggeredHapticsLog);
            Assert.AreEqual(1, _vfxService.SpawnedVfxLog.Count);
            Assert.AreEqual("select_sparkle", _vfxService.SpawnedVfxLog[0].EffectName);
            Assert.AreEqual(new Vector3(3.0f, 6.0f, 0f), _vfxService.SpawnedVfxLog[0].Position);
            Assert.AreEqual(PieceType.ColorA, _vfxService.SpawnedVfxLog[0].PieceType);
        }

        [Test]
        public void FeedbackPresenter_PieceDeselected_TriggersAudioOnly()
        {
            var evt = FeedbackEvent.PieceDeselected(new GridPosition(1, 1), PieceType.ColorB);

            _presenter.HandleFeedback(evt);

            Assert.Contains("piece_deselect", _audioService.PlayedSfxLog);
            Assert.AreEqual(0, _hapticService.TriggeredHapticsLog.Count);
            Assert.AreEqual(0, _vfxService.SpawnedVfxLog.Count);
        }

        [Test]
        public void FeedbackPresenter_PieceMatched_TriggersAudioHapticAndVfx()
        {
            var pos = new GridPosition(1, 4);
            var evt = FeedbackEvent.PieceMatched(pos, PieceType.ColorC, 150);

            _presenter.HandleFeedback(evt);

            Assert.Contains("piece_match", _audioService.PlayedSfxLog);
            Assert.Contains(HapticFeedbackType.Medium, _hapticService.TriggeredHapticsLog);
            Assert.AreEqual(1, _vfxService.SpawnedVfxLog.Count);
            Assert.AreEqual("match_burst", _vfxService.SpawnedVfxLog[0].EffectName);
            Assert.AreEqual(new Vector3(1.5f, 8.0f, 0f), _vfxService.SpawnedVfxLog[0].Position);
            Assert.AreEqual(PieceType.ColorC, _vfxService.SpawnedVfxLog[0].PieceType);
        }

        [Test]
        public void FeedbackPresenter_PieceRemoved_TriggersAudioAndVfx()
        {
            var pos = new GridPosition(0, 2);
            var evt = FeedbackEvent.PieceRemoved(pos, PieceType.ColorD);

            _presenter.HandleFeedback(evt);

            Assert.Contains("piece_pop", _audioService.PlayedSfxLog);
            Assert.AreEqual(1, _vfxService.SpawnedVfxLog.Count);
            Assert.AreEqual("pop_dust", _vfxService.SpawnedVfxLog[0].EffectName);
            Assert.AreEqual(new Vector3(0f, 4.0f, 0f), _vfxService.SpawnedVfxLog[0].Position);
        }

        [Test]
        public void FeedbackPresenter_PieceMoved_TriggersAudio()
        {
            var pos = new GridPosition(3, 1);
            var evt = FeedbackEvent.PieceMoved(pos, PieceType.ColorA);

            _presenter.HandleFeedback(evt);

            Assert.Contains("piece_slide", _audioService.PlayedSfxLog);
            Assert.AreEqual(0, _vfxService.SpawnedVfxLog.Count);
        }

        [Test]
        public void FeedbackPresenter_LevelStarted_PlaysBgm()
        {
            var evt = FeedbackEvent.LevelStarted();

            _presenter.HandleFeedback(evt);

            Assert.AreEqual("bgm_gameplay", _audioService.CurrentlyPlayingMusic);
        }

        [Test]
        public void FeedbackPresenter_LevelCompleted_TriggersWinSequence()
        {
            var evt = FeedbackEvent.LevelCompleted(stars: 3, score: 500);

            _presenter.HandleFeedback(evt);

            Assert.Contains("level_win", _audioService.PlayedSfxLog);
            Assert.Contains(HapticFeedbackType.Success, _hapticService.TriggeredHapticsLog);
            Assert.Contains("win_confetti", _vfxService.SpawnedScreenVfxLog);
        }

        [Test]
        public void FeedbackPresenter_LevelFailed_TriggersFailureFeedback()
        {
            var evt = FeedbackEvent.LevelFailed();

            _presenter.HandleFeedback(evt);

            Assert.Contains("level_fail", _audioService.PlayedSfxLog);
            Assert.Contains(HapticFeedbackType.Failure, _hapticService.TriggeredHapticsLog);
        }

        [Test]
        public void FeedbackPresenter_Resilience_NullServices_DoNotThrow()
        {
            var nullPresenter = new FeedbackPresenter();

            foreach (FeedbackTriggerType trigger in Enum.GetValues(typeof(FeedbackTriggerType)))
            {
                Assert.DoesNotThrow(() =>
                {
                    nullPresenter.HandleFeedback(new FeedbackEvent(trigger, new GridPosition(1, 1), PieceType.ColorA));
                }, $"Failed handling trigger {trigger} with null services");
            }
        }

        [Test]
        public void MockVfxService_SpawnAndClear_OperatesCorrectly()
        {
            var mockVfx = new MockVfxService();

            mockVfx.SpawnPieceVfx("fx1", Vector3.one, PieceType.ColorA);
            mockVfx.SpawnPieceVfx("fx2", Vector3.zero, PieceType.ColorB);
            mockVfx.SpawnScreenVfx("screen_burst");

            Assert.AreEqual(2, mockVfx.SpawnedVfxLog.Count);
            Assert.AreEqual(1, mockVfx.SpawnedScreenVfxLog.Count);

            mockVfx.ClearAllVfx();

            Assert.AreEqual(0, mockVfx.SpawnedVfxLog.Count);
            Assert.AreEqual(0, mockVfx.SpawnedScreenVfxLog.Count);
        }

        [Test]
        public void FeedbackEvent_FactoryMethods_PopulateFieldsProperly()
        {
            var sel = FeedbackEvent.PieceSelected(new GridPosition(3, 4), PieceType.ColorB);
            Assert.AreEqual(FeedbackTriggerType.PieceSelected, sel.Type);
            Assert.AreEqual(new GridPosition(3, 4), sel.Position);
            Assert.AreEqual(PieceType.ColorB, sel.PieceType);

            var desel = FeedbackEvent.PieceDeselected(new GridPosition(1, 2), PieceType.ColorC);
            Assert.AreEqual(FeedbackTriggerType.PieceDeselected, desel.Type);

            var matched = FeedbackEvent.PieceMatched(new GridPosition(0, 1), PieceType.ColorA, 300);
            Assert.AreEqual(FeedbackTriggerType.PieceMatched, matched.Type);
            Assert.AreEqual(300, matched.Score);

            var removed = FeedbackEvent.PieceRemoved(new GridPosition(2, 2), PieceType.ColorD);
            Assert.AreEqual(FeedbackTriggerType.PieceRemoved, removed.Type);

            var moved = FeedbackEvent.PieceMoved(new GridPosition(1, 0), PieceType.ColorA);
            Assert.AreEqual(FeedbackTriggerType.PieceMoved, moved.Type);

            var started = FeedbackEvent.LevelStarted();
            Assert.AreEqual(FeedbackTriggerType.LevelStarted, started.Type);

            var completed = FeedbackEvent.LevelCompleted(3, 1000);
            Assert.AreEqual(FeedbackTriggerType.LevelCompleted, completed.Type);
            Assert.AreEqual(3, completed.Stars);
            Assert.AreEqual(1000, completed.Score);

            var failed = FeedbackEvent.LevelFailed();
            Assert.AreEqual(FeedbackTriggerType.LevelFailed, failed.Type);
        }

        private class RecordingFeedbackPresenter : IFeedbackPresenter
        {
            public List<FeedbackEvent> HandledEvents { get; } = new List<FeedbackEvent>();

            public void HandleFeedback(in FeedbackEvent evt)
            {
                HandledEvents.Add(evt);
            }
        }

        [Test]
        public void BoardView_DispatchesPieceRemoved_ToFeedbackPresenter()
        {
            var boardView = new MockBoardView();
            var recorder = new RecordingFeedbackPresenter();
            boardView.FeedbackPresenter = recorder;

            var board = new Board(3, 3);
            var piece = new Piece(10, PieceType.ColorA);
            var pos = new GridPosition(1, 1);
            board.SetPiece(pos, piece);

            boardView.Bind(board);

            // Removing the piece should dispatch FeedbackTriggerType.PieceRemoved
            board.RemovePiece(pos);

            Assert.AreEqual(1, recorder.HandledEvents.Count);
            Assert.AreEqual(FeedbackTriggerType.PieceRemoved, recorder.HandledEvents[0].Type);
            Assert.AreEqual(pos, recorder.HandledEvents[0].Position);
            Assert.AreEqual(PieceType.ColorA, recorder.HandledEvents[0].PieceType);
        }

        [Test]
        public void BoardView_DispatchesPieceMoved_ToFeedbackPresenter()
        {
            var boardView = new MockBoardView();
            var recorder = new RecordingFeedbackPresenter();
            boardView.FeedbackPresenter = recorder;

            var board = new Board(3, 3);
            var piece = new Piece(20, PieceType.ColorB);
            var fromPos = new GridPosition(0, 0);
            var toPos = new GridPosition(0, 1);
            board.SetPiece(fromPos, piece);

            boardView.Bind(board);

            // Moving the piece should dispatch FeedbackTriggerType.PieceMoved
            board.MovePiece(fromPos, toPos);

            Assert.AreEqual(1, recorder.HandledEvents.Count);
            Assert.AreEqual(FeedbackTriggerType.PieceMoved, recorder.HandledEvents[0].Type);
            Assert.AreEqual(toPos, recorder.HandledEvents[0].Position);
            Assert.AreEqual(PieceType.ColorB, recorder.HandledEvents[0].PieceType);
        }
    }
}
