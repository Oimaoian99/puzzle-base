using System.Collections.Generic;
using Puzzle.Core.Logging;

namespace Puzzle.Core.Services.Mocks
{
    public class MockHapticService : IHapticService
    {
        public bool IsHapticsEnabled { get; set; } = true;
        public List<HapticFeedbackType> TriggeredHapticsLog { get; } = new List<HapticFeedbackType>();

        public void TriggerHaptic(HapticFeedbackType type)
        {
            if (!IsHapticsEnabled) return;
            TriggeredHapticsLog.Add(type);
            CoreLogger.Log($"[MockHapticService] Triggered Haptic: {type}");
        }
    }
}
