using Puzzle.Core.Gameplay.Board;
using Puzzle.Core.Gameplay.Data;
using Puzzle.Core.Gameplay.Puzzle;
using Puzzle.Core.Gameplay.Runtime;
using Puzzle.Core.Level;
using Puzzle.Core.Services.Mocks;
using Puzzle.Presentation.Board;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Puzzle.Composition
{
    /// <summary>
    /// Level-scoped composition root managing transient level runtime dependencies.
    /// Disposed when exiting, restarting, or transitioning levels.
    /// </summary>
    public class LevelLifetimeScope : LifetimeScope
    {
        [SerializeField] private string levelId = "level_001";
        [SerializeField] private int startingMoves = 25;
        [SerializeField] private BoardView boardView;

        public LevelId CurrentLevelId => new LevelId(levelId);

        public void SetLevelContext(LevelId id, int moves, BoardView view = null)
        {
            levelId = id.Value;
            startingMoves = moves;
            boardView = view;
        }

        protected override void Configure(IContainerBuilder builder)
        {
            ConfigureLevelServices(builder, CurrentLevelId, startingMoves, null, boardView);
        }

        public static void ConfigureLevelServices(IContainerBuilder builder, LevelId id, int startingMoves = 25, LevelData levelData = null, BoardView boardView = null, System.Type puzzleLogicType = null)
        {
            var data = levelData ?? LevelData.CreateDefault(id, 8, 8, startingMoves);
            builder.RegisterInstance(data);

            // Register session runtime instance as Scoped
            builder.Register<LevelRuntime>(Lifetime.Scoped)
                   .WithParameter("id", id)
                   .WithParameter("startingMoves", data.StartingMoves > 0 ? data.StartingMoves : startingMoves)
                   .As<ILevelRuntime>();

            // Register puzzle logic and runtime as Scoped
            var logicType = puzzleLogicType ?? typeof(MockPuzzleLogic);
            builder.Register(logicType, Lifetime.Scoped).As<IPuzzleLogic>();
            builder.Register<PuzzleRuntime>(Lifetime.Scoped);
            builder.Register(resolver => resolver.Resolve<PuzzleRuntime>().Board, Lifetime.Scoped);

            if (boardView != null)
            {
                builder.RegisterComponent(boardView);
            }

            // Register session coordinator as Scoped (pure constructor injection)
            builder.Register<LevelSessionTracker>(Lifetime.Scoped);
        }

        public static void ConfigureLevelServices<TLogic>(IContainerBuilder builder, LevelId id, int startingMoves = 25, LevelData levelData = null, BoardView boardView = null) where TLogic : IPuzzleLogic
        {
            ConfigureLevelServices(builder, id, startingMoves, levelData, boardView, typeof(TLogic));
        }
    }
}
