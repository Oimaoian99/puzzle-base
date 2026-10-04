using System;
using System.IO;
using NUnit.Framework;
using Puzzle.Core.Config;
using Puzzle.Core.Level;
using Puzzle.Core.Services;
using Puzzle.Core.Services.Mocks;

namespace Puzzle.Tests.Core.EditMode
{
    [TestFixture]
    public class ProductionInfrastructureTests
    {
        private string _tempSaveDir;
        private string _tempSaveFile;

        [SetUp]
        public void SetUp()
        {
            _tempSaveDir = Path.Combine(Path.GetTempPath(), "PuzzleBase_Tests_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempSaveDir);
            _tempSaveFile = Path.Combine(_tempSaveDir, "test_player_save.json");
        }

        [TearDown]
        public void TearDown()
        {
            try
            {
                if (Directory.Exists(_tempSaveDir))
                {
                    Directory.Delete(_tempSaveDir, true);
                }
            }
            catch
            {
                // Best effort cleanup in test temp dir
            }
        }

        // ==========================================
        // 1. SAVE SERVICE & PERSISTENCE TESTS
        // ==========================================

        [Test]
        public void FileSaveService_NewFile_InitializesWithDefaultValues()
        {
            var saveService = new FileSaveService(_tempSaveFile);

            Assert.AreEqual(0, saveService.GetCoins());
            Assert.AreEqual(0, saveService.GetHighestCompletedLevelIndex());
            Assert.IsFalse(saveService.IsLevelCompleted(new LevelId("level_001")));
            Assert.AreEqual(0, saveService.GetStarsEarned(new LevelId("level_001")));
        }

        [Test]
        public void FileSaveService_AddAndSpendCoins_PersistsAtomicallyToDisk()
        {
            var saveService = new FileSaveService(_tempSaveFile);
            saveService.AddCoins(100);

            Assert.AreEqual(100, saveService.GetCoins());
            Assert.IsTrue(File.Exists(_tempSaveFile), "Save file should be written to disk.");

            // Verify spending
            bool spentSuccess = saveService.TrySpendCoins(40);
            Assert.IsTrue(spentSuccess);
            Assert.AreEqual(60, saveService.GetCoins());

            bool spentFail = saveService.TrySpendCoins(1000);
            Assert.IsFalse(spentFail);
            Assert.AreEqual(60, saveService.GetCoins());

            // Reload from disk in a fresh instance
            var reloadedService = new FileSaveService(_tempSaveFile);
            Assert.AreEqual(60, reloadedService.GetCoins());
        }

        [Test]
        public void FileSaveService_CompleteLevel_TracksStarsAndHighScore()
        {
            var saveService = new FileSaveService(_tempSaveFile);
            var levelId = new LevelId("level_005");

            saveService.SetLevelCompleted(levelId, 2, 1500);

            Assert.IsTrue(saveService.IsLevelCompleted(levelId));
            Assert.AreEqual(2, saveService.GetStarsEarned(levelId));
            Assert.AreEqual(5, saveService.GetHighestCompletedLevelIndex());

            // Improve score and stars
            saveService.SetLevelCompleted(levelId, 3, 2000);
            Assert.AreEqual(3, saveService.GetStarsEarned(levelId));

            // Reload from disk to verify persistence
            var reloadedService = new FileSaveService(_tempSaveFile);
            Assert.IsTrue(reloadedService.IsLevelCompleted(levelId));
            Assert.AreEqual(3, reloadedService.GetStarsEarned(levelId));
            Assert.AreEqual(5, reloadedService.GetHighestCompletedLevelIndex());
        }

        [Test]
        public void FileSaveService_CorruptFile_RecoversGracefullyWithoutCrashing()
        {
            // Write corrupted garbage to the save file
            File.WriteAllText(_tempSaveFile, "{ corrupt_json: [ invalid data @#$!! ");

            // Loading corrupt file must NOT throw an unhandled exception
            FileSaveService saveService = null;
            Assert.DoesNotThrow(() =>
            {
                saveService = new FileSaveService(_tempSaveFile);
            });

            Assert.IsNotNull(saveService);
            Assert.AreEqual(0, saveService.GetCoins());
            Assert.AreEqual(0, saveService.GetHighestCompletedLevelIndex());

            // Modifying and saving now heals the file
            saveService.AddCoins(50);
            Assert.AreEqual(50, saveService.GetCoins());

            var reloaded = new FileSaveService(_tempSaveFile);
            Assert.AreEqual(50, reloaded.GetCoins());
        }

        // ==========================================
        // 2. ADS BOUNDARY TESTS
        // ==========================================

        [Test]
        public void AdsService_ShowRewarded_ExecutesRewardAndClosedCallbacks()
        {
            var adsService = new MockAdsService();
            bool rewardGranted = false;
            bool closedCalled = false;

            adsService.ShowRewardedAd("double_coins", () => rewardGranted = true, () => closedCalled = true);

            Assert.IsTrue(rewardGranted, "Reward callback must be invoked upon ad completion.");
            Assert.IsTrue(closedCalled, "Closed callback must be invoked after ad finishes.");
        }

        [Test]
        public void AdsService_ShowInterstitial_ExecutesClosedCallback()
        {
            var adsService = new MockAdsService();
            bool closedCalled = false;

            adsService.ShowInterstitialAd("level_end", () => closedCalled = true);

            Assert.IsTrue(closedCalled, "Interstitial closed callback must be invoked.");
        }

        // ==========================================
        // 3. IAP BOUNDARY TESTS
        // ==========================================

        [Test]
        public void IAPService_PurchaseProduct_SucceedsAndMarksProductOwned()
        {
            var iapService = new MockIAPService();
            bool callbackInvoked = false;
            bool purchaseSuccess = false;
            string purchaseError = null;

            iapService.PurchaseProduct("no_ads", (success, err) =>
            {
                callbackInvoked = true;
                purchaseSuccess = success;
                purchaseError = err;
            });

            Assert.IsTrue(callbackInvoked);
            Assert.IsTrue(purchaseSuccess);
            Assert.IsNull(purchaseError);
            Assert.IsTrue(iapService.IsProductOwned("no_ads"));
            Assert.IsFalse(iapService.IsProductOwned("unowned_item"));
        }

        [Test]
        public void IAPService_PurchaseFailure_HandlesGracefullyWithErrorMessage()
        {
            var iapService = new MockIAPService();
            iapService.FailNextPurchase = true;

            bool purchaseSuccess = true;
            string purchaseError = null;

            iapService.PurchaseProduct("coins_bundle_100", (success, err) =>
            {
                purchaseSuccess = success;
                purchaseError = err;
            });

            Assert.IsFalse(purchaseSuccess);
            Assert.IsNotNull(purchaseError);
            Assert.IsFalse(iapService.IsProductOwned("coins_bundle_100"));
        }

        // ==========================================
        // 4. LOCALIZATION BOUNDARY TESTS
        // ==========================================

        [Test]
        public void LocalizationService_RetrievesDefaultAndFormattedStrings()
        {
            var loc = new SimpleLocalizationService("en");

            Assert.AreEqual("Play", loc.GetText("ui_play"));
            Assert.AreEqual("Score: 1200", loc.GetText("ui_score", 1200));
            Assert.AreEqual("Victory!", loc.GetText("ui_win"));
        }

        [Test]
        public void LocalizationService_MissingKey_FallsBackToKeyGracefullyWithoutCrashing()
        {
            var loc = new SimpleLocalizationService("en");

            // Missing key should safely return the key itself
            string result = loc.GetText("ui_unknown_action_key");
            Assert.AreEqual("ui_unknown_action_key", result);
        }

        [Test]
        public void LocalizationService_LanguageSwitch_UpdatesRetrievedText()
        {
            var loc = new SimpleLocalizationService("en");
            loc.AddTranslation("es", "ui_play", "Jugar");

            bool languageChangedFired = false;
            loc.OnLanguageChanged += lang => languageChangedFired = true;

            loc.SetLanguage("es");

            Assert.IsTrue(languageChangedFired);
            Assert.AreEqual("es", loc.CurrentLanguage);
            Assert.AreEqual("Jugar", loc.GetText("ui_play"));

            // Key not present in 'es' safely falls back to 'en'
            Assert.AreEqual("Victory!", loc.GetText("ui_win"));
        }

        // ==========================================
        // 5. AUDIO & HAPTIC BOUNDARY TESTS
        // ==========================================

        [Test]
        public void AudioService_PlaySfxAndMusic_TracksPlaybackAndRespectsMute()
        {
            var audio = new MockAudioService();

            audio.PlaySfx("click");
            audio.PlayMusic("bgm_main");

            Assert.AreEqual(1, audio.PlayedSfxLog.Count);
            Assert.AreEqual("click", audio.PlayedSfxLog[0]);
            Assert.AreEqual("bgm_main", audio.CurrentlyPlayingMusic);

            // SFX Mute check
            audio.IsSfxMuted = true;
            audio.PlaySfx("explosion");
            Assert.AreEqual(1, audio.PlayedSfxLog.Count, "Muted SFX should not be triggered.");

            // Music Stop check
            audio.StopMusic();
            Assert.IsNull(audio.CurrentlyPlayingMusic);
        }

        [Test]
        public void HapticService_TriggerHaptic_TracksTriggersAndRespectsEnabledFlag()
        {
            var haptics = new MockHapticService();

            haptics.TriggerHaptic(HapticFeedbackType.Light);
            haptics.TriggerHaptic(HapticFeedbackType.Success);

            Assert.AreEqual(2, haptics.TriggeredHapticsLog.Count);
            Assert.AreEqual(HapticFeedbackType.Light, haptics.TriggeredHapticsLog[0]);
            Assert.AreEqual(HapticFeedbackType.Success, haptics.TriggeredHapticsLog[1]);

            haptics.IsHapticsEnabled = false;
            haptics.TriggerHaptic(HapticFeedbackType.Heavy);
            Assert.AreEqual(2, haptics.TriggeredHapticsLog.Count, "Disabled haptics should not trigger.");
        }

        // ==========================================
        // 6. APPLICATION CONFIGURATION TESTS
        // ==========================================

        [Test]
        public void AppConfigService_RetrievesFeatureFlagsAndSettings()
        {
            var config = new AppConfig();
            config.Features.EnableMatch3BonusMoves = true;
            config.Features.EnableSpecialPieceVfx = false;

            var service = new AppConfigService(config);

            Assert.IsTrue(service.IsFeatureEnabled("match3bonusmoves"));
            Assert.IsFalse(service.IsFeatureEnabled("specialpiecevfx"));
            Assert.IsFalse(service.IsFeatureEnabled("non_existent_feature"));
            Assert.AreEqual("Development", service.Config.Environment);
        }
    }
}
