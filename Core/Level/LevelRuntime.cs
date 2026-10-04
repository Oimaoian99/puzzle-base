using System;

namespace Puzzle.Core.Level
{
    public class LevelRuntime : ILevelRuntime
    {
        public LevelId Id { get; }
        public LevelLifecycleState State { get; private set; }
        public int CurrentScore { get; private set; }
        public int RemainingMoves { get; private set; }
        public float ElapsedTime { get; private set; }
        public bool IsFinished => State == LevelLifecycleState.Completed || State == LevelLifecycleState.Failed;

        public event Action<LevelLifecycleState> OnStateChanged;
        public event Action<int> OnScoreChanged;
        public event Action<int> OnMovesChanged;
        public event Action<LevelResultData> OnLevelFinished;

        public LevelRuntime(LevelId id, int startingMoves = 30)
        {
            Id = id;
            RemainingMoves = Math.Max(0, startingMoves);
            CurrentScore = 0;
            ElapsedTime = 0f;
            State = LevelLifecycleState.Ready;
        }

        public void AddScore(int amount)
        {
            if (IsFinished || amount <= 0) return;
            CurrentScore += amount;
            OnScoreChanged?.Invoke(CurrentScore);
        }

        public bool TryConsumeMoves(int count = 1)
        {
            if (IsFinished || count <= 0) return false;
            if (RemainingMoves < count) return false;

            RemainingMoves -= count;
            OnMovesChanged?.Invoke(RemainingMoves);
            return true;
        }

        public void UpdateTimer(float deltaTime)
        {
            if (State == LevelLifecycleState.Playing && deltaTime > 0f)
            {
                ElapsedTime += deltaTime;
            }
        }

        public void TransitionState(LevelLifecycleState newState)
        {
            if (State == newState) return;
            State = newState;
            OnStateChanged?.Invoke(newState);
        }

        public void CompleteLevel(int stars)
        {
            if (IsFinished) return;
            TransitionState(LevelLifecycleState.Completed);

            var result = new LevelResultData(Id, true, CurrentScore, stars, ElapsedTime, RemainingMoves);
            OnLevelFinished?.Invoke(result);
        }

        public void FailLevel()
        {
            if (IsFinished) return;
            TransitionState(LevelLifecycleState.Failed);

            var result = new LevelResultData(Id, false, CurrentScore, 0, ElapsedTime, RemainingMoves);
            OnLevelFinished?.Invoke(result);
        }
    }
}
