using System;
using System.Collections.Generic;

namespace Puzzle.Core.Services.Mocks
{
    public class MockAdsService : IAdsService
    {
        public bool ForceRewardSuccess = true;
        public int RewardedAdsShownCount { get; private set; }
        public int InterstitialAdsShownCount { get; private set; }
        public List<string> ShownPlacements { get; } = new List<string>();

        public bool IsRewardedAdReady(string placement) => true;

        public void ShowRewardedAd(string placement, Action onRewardGranted, Action onClosed)
        {
            RewardedAdsShownCount++;
            ShownPlacements.Add(placement);
            if (ForceRewardSuccess)
            {
                onRewardGranted?.Invoke();
            }
            onClosed?.Invoke();
        }

        public bool IsInterstitialAdReady(string placement) => true;

        public void ShowInterstitialAd(string placement, Action onClosed)
        {
            InterstitialAdsShownCount++;
            ShownPlacements.Add(placement);
            onClosed?.Invoke();
        }
    }
}
