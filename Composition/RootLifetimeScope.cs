using Puzzle.Core.Config;
using Puzzle.Core.GameFlow;
using Puzzle.Core.GameFlow.States;
using Puzzle.Core.Level;
using Puzzle.Core.Services;
using Puzzle.Core.Services.Mocks;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Puzzle.Composition
{
    /// <summary>
    /// Application-level composition root managing long-lived global services.
    /// Exists outside of Core in the Composition layer.
    /// </summary>
    public class RootLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            ConfigureRootServices(builder);
        }

        public static void ConfigureRootServices(IContainerBuilder builder)
        {
            // Register Core Infrastructure Services as Singletons
            builder.Register(c => new FileSaveService(), Lifetime.Singleton).As<ISaveService>();
            builder.Register<MockAdsService>(Lifetime.Singleton).As<IAdsService>();
            builder.Register<MockAnalyticsService>(Lifetime.Singleton).As<IAnalyticsService>();
            builder.Register<MockIAPService>(Lifetime.Singleton).As<IIAPService>();
            builder.Register(c => new SimpleLocalizationService(), Lifetime.Singleton).As<ILocalizationService>();
            builder.Register<MockAudioService>(Lifetime.Singleton).As<IAudioService>();
            builder.Register<MockHapticService>(Lifetime.Singleton).As<IHapticService>();
            builder.Register(c => new AppConfigService(), Lifetime.Singleton).As<IAppConfigService>();

            // Register Level Scope Loader as Singleton ILevelLoader
            builder.Register<LevelScopeLoader>(Lifetime.Singleton).As<ILevelLoader>();

            // Register Master Game State Machine as Singleton
            builder.Register<GameStateMachine>(Lifetime.Singleton).As<IGameStateMachine>();

            // Register standard Game States as Singletons (explicit factory for IL2CPP/AOT stripping immunity)
            builder.Register(c => new BootState(c.Resolve<IGameStateMachine>()), Lifetime.Singleton);
            builder.Register(c => new InitState(c.Resolve<IGameStateMachine>(), c.Resolve<ISaveService>()), Lifetime.Singleton);
            builder.Register(c => new HomeState(c.Resolve<IGameStateMachine>()), Lifetime.Singleton);
            builder.Register(c => new LevelLoadingState(c.Resolve<IGameStateMachine>(), c.Resolve<ILevelLoader>()), Lifetime.Singleton);
            builder.Register(c => new PlayState(c.Resolve<IGameStateMachine>(), c.Resolve<IAnalyticsService>()), Lifetime.Singleton);
            builder.Register(c => new ResultState(c.Resolve<IGameStateMachine>(), c.Resolve<ILevelLoader>(), c.Resolve<ISaveService>(), c.Resolve<IAnalyticsService>()), Lifetime.Singleton);
            builder.Register(c => new PausedState(c.Resolve<IGameStateMachine>(), c.Resolve<ILevelLoader>()), Lifetime.Singleton);

            // Register GameFlowStarter as application entry point
            builder.RegisterEntryPoint<GameFlowStarter>().AsSelf();
        }
    }
}
