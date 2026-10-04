using System;
using NUnit.Framework;
using Puzzle.Composition;
using Puzzle.Core.GameFlow;
using Puzzle.Core.Level;
using Puzzle.Core.Services;
using VContainer;

namespace Puzzle.Tests.Composition
{
    [TestFixture]
    public class VContainerResolutionTests
    {
        [Test]
        public void RootContainer_ResolvesAllCoreServices()
        {
            var builder = new ContainerBuilder();
            RootLifetimeScope.ConfigureRootServices(builder);
            var container = builder.Build();

            var saveService = container.Resolve<ISaveService>();
            var adsService = container.Resolve<IAdsService>();
            var analyticsService = container.Resolve<IAnalyticsService>();
            var iapService = container.Resolve<IIAPService>();
            var locService = container.Resolve<ILocalizationService>();
            var audioService = container.Resolve<IAudioService>();
            var hapticService = container.Resolve<IHapticService>();
            var configService = container.Resolve<Puzzle.Core.Config.IAppConfigService>();
            var fsm = container.Resolve<IGameStateMachine>();

            Assert.IsNotNull(saveService);
            Assert.IsNotNull(adsService);
            Assert.IsNotNull(analyticsService);
            Assert.IsNotNull(iapService);
            Assert.IsNotNull(locService);
            Assert.IsNotNull(audioService);
            Assert.IsNotNull(hapticService);
            Assert.IsNotNull(configService);
            Assert.IsNotNull(fsm);
        }

        [Test]
        public void RootContainer_ServicesAreSingletons()
        {
            var builder = new ContainerBuilder();
            RootLifetimeScope.ConfigureRootServices(builder);
            var container = builder.Build();

            var save1 = container.Resolve<ISaveService>();
            var save2 = container.Resolve<ISaveService>();

            Assert.AreSame(save1, save2);
        }

        [Test]
        public void ChildScope_ResolvesConstructorInjectedSessionTracker()
        {
            var rootBuilder = new ContainerBuilder();
            RootLifetimeScope.ConfigureRootServices(rootBuilder);
            var rootContainer = rootBuilder.Build();

            using (var levelScope = rootContainer.CreateScope(levelBuilder =>
            {
                LevelLifetimeScope.ConfigureLevelServices(levelBuilder, new LevelId("test_level"), 30);
            }))
            {
                var tracker = levelScope.Resolve<LevelSessionTracker>();
                Assert.IsNotNull(tracker);
                Assert.IsNotNull(tracker.Runtime);
                Assert.IsNotNull(tracker.Analytics);
                Assert.IsNotNull(tracker.Save);

                Assert.AreEqual("test_level", tracker.Runtime.Id.Value);
                Assert.AreEqual(30, tracker.Runtime.RemainingMoves);
            }
        }

        [Test]
        public void MissingDependency_ThrowsVContainerException()
        {
            var builder = new ContainerBuilder();
            // Deliberately do not register ISaveService or ILevelRuntime
            builder.Register<LevelSessionTracker>(Lifetime.Scoped);
            var container = builder.Build();

            Assert.Throws<VContainerException>(() =>
            {
                container.Resolve<LevelSessionTracker>();
            });
        }
    }
}
