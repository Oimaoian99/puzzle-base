namespace Puzzle.Core.Config
{
    public interface IAppConfigService
    {
        AppConfig Config { get; }
        bool IsFeatureEnabled(string featureName);
        void SetConfig(AppConfig config);
    }
}
