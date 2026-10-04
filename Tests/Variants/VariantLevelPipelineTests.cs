using System;
using NUnit.Framework;
using Puzzle.Composition;
using Puzzle.Core.GameFlow;
using Puzzle.Core.GameFlow.States;
using Puzzle.Core.Gameplay.Data;
using Puzzle.Core.Gameplay.Grid;
using Puzzle.Core.Gameplay.Pieces;
using Puzzle.Core.Gameplay.Puzzle;
using Puzzle.Core.Gameplay.Runtime;
using Puzzle.Core.Level;
using Puzzle.Core.Services;
using Puzzle.Core.Services.Mocks;
using Puzzle.Variants.Link.Data;
using Puzzle.Variants.Link.Logic;
using Puzzle.Variants.Match3.Data;
using Puzzle.Variants.Match3.Logic;
using VContainer;

namespace Puzzle.Tests.Variants
{
    [TestFixture]
    public class VariantLevelPipelineTests
    {
        private LevelValidator _validator;
        private InMemoryLevelDataProvider _provider;
        private PuzzleVariantRegistry _registry;

        [SetUp]
        public void SetUp()
        {
            _validator = new LevelValidator();
            _provider = new InMemoryLevelDataProvider();
            _registry = new PuzzleVariantRegistry();

            _registry.RegisterVariant("Link", typeof(LinkPuzzleLogic));
            _registry.RegisterVariant("Match3", typeof(Match3PuzzleLogic));
        }

        [Test]
        public void LinkLevelData_ValidData_PassesValidation()
        {
            var data = new LinkLevelData(new LevelId("link_test_01"), 6, 6, 15, 1000, minLinkLength: 3);

            var result = _validator.Validate(data);

            Assert.IsTrue(result.IsValid);
            Assert.AreEqual(0, result.Errors.Count);
        }

        [Test]
        public void LinkLevelData_MinLinkLengthTooShort_FailsValidation()
        {
            var data = new LinkLevelData(new LevelId("link_invalid"), 6, 6, 15, 1000, minLinkLength: 1);

            var result = _validator.Validate(data);

            Assert.IsFalse(result.IsValid);
            Assert.That(result.Errors[0], Does.Contain("Link puzzle MinLinkLength must be at least 2"));
        }

        [Test]
        public void Match3LevelData_ValidData_PassesValidation()
        {
            var data = new Match3LevelData(new LevelId("match3_test_01"), 7, 7, 20, 1500, minMatchLength: 3, refillOnClear: true);

            var result = _validator.Validate(data);

            Assert.IsTrue(result.IsValid);
            Assert.AreEqual(0, result.Errors.Count);
        }

        [Test]
        public void Match3LevelData_MinMatchLengthTooShort_FailsValidation()
        {
            var data = new Match3LevelData(new LevelId("match3_invalid"), 7, 7, 20, 1500, minMatchLength: 2);

            var result = _validator.Validate(data);

            Assert.IsFalse(result.IsValid);
            Assert.That(result.Errors[0], Does.Contain("Match-3 MinMatchLength must be at least 3"));
        }

        [Test]
        public void LevelDataSO_ConvertsToPureLevelData_AndValidates()
        {
            // In headless .NET CLR, FormatterServices allocates the managed instance without requiring Unity's native engine C++ runtime
            var linkSO = (LinkLevelDataSO)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(LinkLevelDataSO));
            linkSO.SetBaseConfig("link_so_01", 5, 5, 12, 600);
            linkSO.SetLinkConfig(3);

            var linkData = linkSO.ToLevelData();
            Assert.IsInstanceOf<LinkLevelData>(linkData);
            Assert.AreEqual("Link", linkData.VariantId);
            Assert.AreEqual(3, ((LinkLevelData)linkData).MinLinkLength);
            Assert.AreEqual(5, linkData.Width);
            Assert.AreEqual(600, linkData.TargetScore);

            var match3SO = (Match3LevelDataSO)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(Match3LevelDataSO));
            match3SO.SetBaseConfig("match3_so_01", 6, 6, 18, 900);
            match3SO.SetMatch3Config(3, true);

            var match3Data = match3SO.ToLevelData();
            Assert.IsInstanceOf<Match3LevelData>(match3Data);
            Assert.AreEqual("Match3", match3Data.VariantId);
            Assert.AreEqual(3, ((Match3LevelData)match3Data).MinMatchLength);
            Assert.AreEqual(6, match3Data.Width);
            Assert.AreEqual(900, match3Data.TargetScore);
        }

        [Test]
        public void LevelScopeLoader_LoadsLinkLevel_AndResolvesLinkPuzzleLogic()
        {
            var linkData = new LinkLevelData(new LevelId("level_link_101"), 6, 6, 15, 1000, 3);
            _provider.RegisterLevel(linkData);

            var builder = new ContainerBuilder();
            builder.RegisterInstance<ILevelDataProvider>(_provider);
            builder.RegisterInstance<IPuzzleVariantRegistry>(_registry);
            builder.RegisterInstance(_validator);
            var rootContainer = builder.Build();

            var loader = new LevelScopeLoader(rootContainer, _provider, _validator, _registry);

            ILevelRuntime loadedRuntime = null;
            string loadError = null;

            loader.LoadLevel(
                new LevelId("level_link_101"),
                onLoaded: r => loadedRuntime = r,
                onError: e => loadError = e);

            Assert.IsNull(loadError);
            Assert.IsNotNull(loadedRuntime);
            Assert.AreEqual("level_link_101", loadedRuntime.Id.Value);
            Assert.AreEqual(15, loadedRuntime.RemainingMoves);

            var resolvedLogic = loader.ActiveScope.Resolve<IPuzzleLogic>();
            Assert.IsInstanceOf<LinkPuzzleLogic>(resolvedLogic);
            Assert.AreEqual(6, loader.ActiveScope.Resolve<PuzzleRuntime>().Board.Width);
        }

        [Test]
        public void LevelScopeLoader_LoadsMatch3Level_AndResolvesMatch3PuzzleLogic()
        {
            var match3Data = new Match3LevelData(new LevelId("level_match3_201"), 5, 5, 22, 1200, 3, true);
            _provider.RegisterLevel(match3Data);

            var builder = new ContainerBuilder();
            builder.RegisterInstance<ILevelDataProvider>(_provider);
            builder.RegisterInstance<IPuzzleVariantRegistry>(_registry);
            builder.RegisterInstance(_validator);
            var rootContainer = builder.Build();

            var loader = new LevelScopeLoader(rootContainer, _provider, _validator, _registry);

            ILevelRuntime loadedRuntime = null;
            string loadError = null;

            loader.LoadLevel(
                new LevelId("level_match3_201"),
                onLoaded: r => loadedRuntime = r,
                onError: e => loadError = e);

            Assert.IsNull(loadError);
            Assert.IsNotNull(loadedRuntime);
            Assert.AreEqual("level_match3_201", loadedRuntime.Id.Value);
            Assert.AreEqual(22, loadedRuntime.RemainingMoves);

            var resolvedLogic = loader.ActiveScope.Resolve<IPuzzleLogic>();
            Assert.IsInstanceOf<Match3PuzzleLogic>(resolvedLogic);
            Assert.AreEqual(5, loader.ActiveScope.Resolve<PuzzleRuntime>().Board.Width);
        }

        [Test]
        public void LevelScopeLoader_NonExistentLevel_InvokesOnError_DoesNotThrow()
        {
            var builder = new ContainerBuilder();
            var rootContainer = builder.Build();

            var loader = new LevelScopeLoader(rootContainer, _provider, _validator, _registry);

            string error = null;
            loader.LoadLevel(
                new LevelId("level_missing_999"),
                onLoaded: _ => { },
                onError: err => error = err);

            Assert.IsNotNull(error);
            Assert.That(error, Does.Contain("not found in level repository"));
            Assert.IsFalse(loader.HasActiveScope);
        }

        [Test]
        public void LevelScopeLoader_InvalidLevelData_InvokesOnError_PreventsScopeCreation()
        {
            var corruptedData = new LevelData(new LevelId("corrupted_01"), -4, 5, 0, 1000);
            _provider.RegisterLevel(corruptedData);

            var builder = new ContainerBuilder();
            var rootContainer = builder.Build();

            var loader = new LevelScopeLoader(rootContainer, _provider, _validator, _registry);

            string error = null;
            loader.LoadLevel(
                new LevelId("corrupted_01"),
                onLoaded: _ => { },
                onError: err => error = err);

            Assert.IsNotNull(error);
            Assert.That(error, Does.Contain("failed validation"));
            Assert.IsFalse(loader.HasActiveScope);
        }

        [Test]
        public void EndToEndFlow_AuthoredLevel_TransitionsToPlayStateWithLoadedBoard()
        {
            var authoredLevel = new Match3LevelData(new LevelId("level_flow_test"), 6, 6, 25, 1000, 3, true);
            authoredLevel.InitialPieces.Add(new InitialPieceSetup(new GridPosition(0, 0), PieceType.ColorA, 1));
            authoredLevel.InitialPieces.Add(new InitialPieceSetup(new GridPosition(1, 0), PieceType.ColorB, 2));
            _provider.RegisterLevel(authoredLevel);

            var stateMachine = new GameStateMachine();
            var builder = new ContainerBuilder();
            builder.RegisterInstance(stateMachine).As<IGameStateMachine>();
            var rootContainer = builder.Build();

            var loader = new LevelScopeLoader(rootContainer, _provider, _validator, _registry);
            var playState = new PlayState(stateMachine, new MockAnalyticsService());
            var loadingState = new LevelLoadingState(stateMachine, loader);

            stateMachine.RegisterState(loadingState);
            stateMachine.RegisterState(playState);

            loadingState.SetTargetLevel(new LevelId("level_flow_test"));
            stateMachine.ChangeState(GameStateId.LevelLoading);

            // Verified state machine transitioned to PlayState automatically on successful load
            Assert.AreEqual(GameStateId.Play, stateMachine.CurrentStateId);
            Assert.IsNotNull(playState.ActiveRuntime);
            Assert.AreEqual("level_flow_test", playState.ActiveRuntime.Id.Value);
            Assert.AreEqual(25, playState.ActiveRuntime.RemainingMoves);

            var board = loader.ActiveScope.Resolve<PuzzleRuntime>().Board;
            Assert.AreEqual(6, board.Width);
            Assert.AreEqual(6, board.Height);
            Assert.IsTrue(board.GetCell(new GridPosition(0, 0)).HasPiece);
            Assert.AreEqual(PieceType.ColorA, board.GetCell(new GridPosition(0, 0)).CurrentPiece.Type);
        }
    }
}
