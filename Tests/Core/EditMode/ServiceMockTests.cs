using System.Collections.Generic;
using NUnit.Framework;
using Puzzle.Core.Level;
using Puzzle.Core.Services.Mocks;

namespace Puzzle.Tests.Core.EditMode
{
    [TestFixture]
    public class ServiceMockTests
    {
        [Test]
        public void MockAdsService_ShowsRewarded_InvokesRewardCallback()
        {
            var ads = new MockAdsService();
            bool rewardGranted = false;
            bool closed = false;

            ads.ShowRewardedAd("double_coins", () => rewardGranted = true, () => closed = true);

            Assert.IsTrue(rewardGranted);
            Assert.IsTrue(closed);
            Assert.AreEqual(1, ads.RewardedAdsShownCount);
            CollectionAssert.Contains(ads.ShownPlacements, "double_coins");
        }

        [Test]
        public void MockAnalyticsService_TracksLevelLifecycleEvents()
        {
            var analytics = new MockAnalyticsService();
            var levelId = new LevelId("test_01");

            analytics.LogLevelStarted(levelId);
            Assert.AreEqual(1, analytics.StartedLevels.Count);
            Assert.AreEqual(levelId, analytics.StartedLevels[0]);

            var result = new LevelResultData(levelId, true, 1200, 3, 45.2f, 10);
            analytics.LogLevelCompleted(result);

            Assert.AreEqual(1, analytics.CompletedLevels.Count);
            Assert.AreEqual(1200, analytics.CompletedLevels[0].Score);
            Assert.AreEqual(2, analytics.EventLog.Count);
        }

        [Test]
        public void MockSaveService_StoresLevelCompletionAndCoins()
        {
            var save = new MockSaveService();
            var levelId = new LevelId("5");

            Assert.IsFalse(save.IsLevelCompleted(levelId));
            save.SetLevelCompleted(levelId, stars: 2, score: 500);

            Assert.IsTrue(save.IsLevelCompleted(levelId));
            Assert.AreEqual(2, save.GetStarsEarned(levelId));
            Assert.AreEqual(5, save.GetHighestCompletedLevelIndex());

            save.AddCoins(100);
            Assert.AreEqual(100, save.GetCoins());

            bool spent = save.TrySpendCoins(40);
            Assert.IsTrue(spent);
            Assert.AreEqual(60, save.GetCoins());

            bool overspent = save.TrySpendCoins(100);
            Assert.IsFalse(overspent);
            Assert.AreEqual(60, save.GetCoins());
        }
    }
}
