using System;

namespace Puzzle.Core.Config
{
    public class AppConfigService : IAppConfigService
    {
        private AppConfig _config;

        public AppConfig Config => _config;

        public AppConfigService() : this(null)
        {
        }

        public AppConfigService(AppConfig initialConfig)
        {
            _config = initialConfig ?? new AppConfig();
        }

        public void SetConfig(AppConfig config)
        {
            _config = config ?? new AppConfig();
        }

        public bool IsFeatureEnabled(string featureName)
        {
            if (string.IsNullOrEmpty(featureName) || _config == null || _config.Features == null)
            {
                return false;
            }

            switch (featureName.ToLowerInvariant())
            {
                case "match3bonusmoves":
                    return _config.Features.EnableMatch3BonusMoves;
                case "specialpiecevfx":
                    return _config.Features.EnableSpecialPieceVfx;
                case "haptics":
                    return _config.Features.EnableHaptics;
                default:
                    return false;
            }
        }
    }
}
