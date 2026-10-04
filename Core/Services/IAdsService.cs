using System;

namespace Puzzle.Core.Services
{
    public interface IAdsService
    {
        bool IsRewardedAdReady(string placement);
        void ShowRewardedAd(string placement, Action onRewardGranted, Action onClosed);
        
        bool IsInterstitialAdReady(string placement);
        void ShowInterstitialAd(string placement, Action onClosed);
    }
}
