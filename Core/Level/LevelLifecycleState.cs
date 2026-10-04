namespace Puzzle.Core.Level
{
    public enum LevelLifecycleState
    {
        Unloaded = 0,
        Loading = 1,
        Ready = 2,
        Playing = 3,
        Paused = 4,
        Completed = 5,
        Failed = 6,
        Disposed = 7
    }
}
