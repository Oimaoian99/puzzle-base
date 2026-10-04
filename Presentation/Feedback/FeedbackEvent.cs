using System;
using Puzzle.Core.Gameplay.Grid;
using Puzzle.Core.Gameplay.Pieces;
using UnityEngine;

namespace Puzzle.Presentation.Feedback
{
    public enum FeedbackTriggerType
    {
        PieceSelected,
        PieceDeselected,
        PieceMatched,
        PieceRemoved,
        PieceMoved,
        LevelStarted,
        LevelCompleted,
        LevelFailed
    }

    public readonly struct FeedbackEvent
    {
        public FeedbackTriggerType Type { get; }
        public GridPosition Position { get; }
        public PieceType PieceType { get; }
        public int Score { get; }
        public int Stars { get; }
        public string Message { get; }

        public FeedbackEvent(
            FeedbackTriggerType type,
            GridPosition position = default,
            PieceType pieceType = PieceType.None,
            int score = 0,
            int stars = 0,
            string message = null)
        {
            Type = type;
            Position = position;
            PieceType = pieceType;
            Score = score;
            Stars = stars;
            Message = message;
        }

        public static FeedbackEvent PieceSelected(GridPosition pos, PieceType type) =>
            new FeedbackEvent(FeedbackTriggerType.PieceSelected, pos, type);

        public static FeedbackEvent PieceDeselected(GridPosition pos, PieceType type) =>
            new FeedbackEvent(FeedbackTriggerType.PieceDeselected, pos, type);

        public static FeedbackEvent PieceMatched(GridPosition pos, PieceType type, int score) =>
            new FeedbackEvent(FeedbackTriggerType.PieceMatched, pos, type, score: score);

        public static FeedbackEvent PieceRemoved(GridPosition pos, PieceType type) =>
            new FeedbackEvent(FeedbackTriggerType.PieceRemoved, pos, type);

        public static FeedbackEvent PieceMoved(GridPosition pos, PieceType type) =>
            new FeedbackEvent(FeedbackTriggerType.PieceMoved, pos, type);

        public static FeedbackEvent LevelStarted() =>
            new FeedbackEvent(FeedbackTriggerType.LevelStarted);

        public static FeedbackEvent LevelCompleted(int stars, int score) =>
            new FeedbackEvent(FeedbackTriggerType.LevelCompleted, stars: stars, score: score);

        public static FeedbackEvent LevelFailed() =>
            new FeedbackEvent(FeedbackTriggerType.LevelFailed);
    }

    public interface IFeedbackPresenter
    {
        void HandleFeedback(in FeedbackEvent evt);
    }
}
