using System;
using System.Collections.Generic;

namespace Puzzle.Core.Config
{
    [Serializable]
    public class AdConfig
    {
        public bool BannerEnabled = false;
        public bool InterstitialEnabled = true;
        public int InterstitialIntervalSeconds = 60;
        public bool RewardedEnabled = true;
    }

    [Serializable]
    public class AnalyticsConfig
    {
        public bool AnalyticsEnabled = true;
        public bool LogDetailedEvents = false;
    }

    [Serializable]
    public class FeatureFlags
    {
        public bool EnableMatch3BonusMoves = false;
        public bool EnableSpecialPieceVfx = true;
        public bool EnableHaptics = true;
    }

    [Serializable]
    public class AppConfig
    {
        public string Environment = "Development";
        public string GameVersion = "1.0.0";
        public AdConfig Ads = new AdConfig();
        public AnalyticsConfig Analytics = new AnalyticsConfig();
        public FeatureFlags Features = new FeatureFlags();
    }
}
