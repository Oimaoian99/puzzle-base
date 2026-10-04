using System;

namespace Puzzle.Core.Level
{
    public interface ILevelRuntime
    {
        LevelId Id { get; }
        LevelLifecycleState State { get; }
        int CurrentScore { get; }
        int RemainingMoves { get; }
        float ElapsedTime { get; }
        bool IsFinished { get; }

        event Action<LevelLifecycleState> OnStateChanged;
        event Action<int> OnScoreChanged;
        event Action<int> OnMovesChanged;
        event Action<LevelResultData> OnLevelFinished;

        void AddScore(int amount);
        bool TryConsumeMoves(int count = 1);
        void UpdateTimer(float deltaTime);
        void TransitionState(LevelLifecycleState newState);
        void CompleteLevel(int stars);
        void FailLevel();
    }
}
