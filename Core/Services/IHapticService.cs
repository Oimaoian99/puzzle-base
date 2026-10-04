namespace Puzzle.Core.Services
{
    public enum HapticFeedbackType
    {
        Light,
        Medium,
        Heavy,
        Success,
        Warning,
        Failure
    }

    public interface IHapticService
    {
        bool IsHapticsEnabled { get; set; }
        void TriggerHaptic(HapticFeedbackType type);
    }
}
