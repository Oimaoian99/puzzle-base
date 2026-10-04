using System.Collections.Generic;
using NUnit.Framework;
using Puzzle.Core.Gameplay.Data;
using Puzzle.Core.Gameplay.Grid;
using Puzzle.Core.Gameplay.Pieces;
using Puzzle.Core.Gameplay.Runtime;
using Puzzle.Core.Level;
using Puzzle.Core.Services.Mocks;

namespace Puzzle.Tests.Core.EditMode
{
    [TestFixture]
    public class LevelAuthoringTests
    {
        private LevelValidator _validator;

        [SetUp]
        public void SetUp()
        {
            _validator = new LevelValidator();
        }

        [Test]
        public void ValidLevel_PassesValidation()
        {
            var data = new LevelData(new LevelId("level_001"), 6, 6, 20, 1000);
            data.InitialPieces.Add(new InitialPieceSetup(new GridPosition(0, 0), PieceType.ColorA, 1));
            data.InitialPieces.Add(new InitialPieceSetup(new GridPosition(1, 0), PieceType.ColorB, 2));

            var result = _validator.Validate(data);

            Assert.IsTrue(result.IsValid);
            Assert.AreEqual(0, result.Errors.Count);
        }

        [Test]
        public void MissingLevelId_FailsValidation()
        {
            var data = new LevelData(LevelId.Empty, 6, 6, 20, 1000);

            var result = _validator.Validate(data);

            Assert.IsFalse(result.IsValid);
            Assert.That(result.Errors[0], Does.Contain("Missing Level ID"));
        }

        [Test]
        public void InvalidBoardDimensions_FailsValidation()
        {
            var dataZero = new LevelData(new LevelId("level_bad"), 0, 6, 20, 1000);
            var dataNeg = new LevelData(new LevelId("level_bad2"), 5, -2, 20, 1000);

            var res1 = _validator.Validate(dataZero);
            var res2 = _validator.Validate(dataNeg);

            Assert.IsFalse(res1.IsValid);
            Assert.IsFalse(res2.IsValid);
            Assert.That(res1.Errors[0], Does.Contain("Invalid board size"));
            Assert.That(res2.Errors[0], Does.Contain("Invalid board size"));
        }

        [Test]
        public void DimensionsExceedingMax_FailsValidation()
        {
            var data = new LevelData(new LevelId("level_huge"), 50, 50, 20, 1000);

            var result = _validator.Validate(data);

            Assert.IsFalse(result.IsValid);
            Assert.That(result.Errors[0], Does.Contain("exceeds maximum supported dimension"));
        }

        [Test]
        public void NonPositiveMovesOrScore_FailsValidation()
        {
            var dataMoves = new LevelData(new LevelId("level_001"), 6, 6, 0, 1000);
            var dataScore = new LevelData(new LevelId("level_002"), 6, 6, 20, -50);

            var resMoves = _validator.Validate(dataMoves);
            var resScore = _validator.Validate(dataScore);

            Assert.IsFalse(resMoves.IsValid);
            Assert.That(resMoves.Errors[0], Does.Contain("StartingMoves must be greater than 0"));

            Assert.IsFalse(resScore.IsValid);
            Assert.That(resScore.Errors[0], Does.Contain("TargetScore must be greater than 0"));
        }

        [Test]
        public void BlockedCellOutOfBounds_FailsValidation()
        {
            var data = new LevelData(new LevelId("level_blocked"), 5, 5, 20, 1000);
            data.BlockedCells.Add(new GridPosition(5, 2)); // X=5 is out of 0..4

            var result = _validator.Validate(data);

            Assert.IsFalse(result.IsValid);
            Assert.That(result.Errors[0], Does.Contain("Blocked cell at (5, 2) is outside board size 5x5"));
        }

        [Test]
        public void PieceOutOfBounds_FailsValidation()
        {
            var data = new LevelData(new LevelId("level_oob"), 8, 8, 20, 1000);
            data.InitialPieces.Add(new InitialPieceSetup(new GridPosition(8, 4), PieceType.ColorA, 1));

            var result = _validator.Validate(data);

            Assert.IsFalse(result.IsValid);
            Assert.That(result.Errors[0], Does.Contain("Piece '1' (ColorA) at (8, 4) is outside board size 8x8"));
        }

        [Test]
        public void PieceOnBlockedCell_FailsValidation()
        {
            var data = new LevelData(new LevelId("level_collision"), 5, 5, 20, 1000);
            data.BlockedCells.Add(new GridPosition(2, 2));
            data.InitialPieces.Add(new InitialPieceSetup(new GridPosition(2, 2), PieceType.ColorA, 1));

            var result = _validator.Validate(data);

            Assert.IsFalse(result.IsValid);
            Assert.That(result.Errors[0], Does.Contain("placed on a blocked cell"));
        }

        [Test]
        public void DuplicatePiecePlacement_FailsValidation()
        {
            var data = new LevelData(new LevelId("level_dup"), 5, 5, 20, 1000);
            data.InitialPieces.Add(new InitialPieceSetup(new GridPosition(1, 1), PieceType.ColorA, 1));
            data.InitialPieces.Add(new InitialPieceSetup(new GridPosition(1, 1), PieceType.ColorB, 2));

            var result = _validator.Validate(data);

            Assert.IsFalse(result.IsValid);
            Assert.That(result.Errors[0], Does.Contain("Duplicate piece placement at (1, 1)"));
        }

        [Test]
        public void InMemoryLevelDataProvider_RegistersAndRetrievesLevelData()
        {
            var provider = new InMemoryLevelDataProvider();
            var data = new LevelData(new LevelId("level_105"), 4, 4, 15, 800);

            provider.RegisterLevel(data);

            Assert.IsTrue(provider.HasLevel(new LevelId("level_105")));
            Assert.IsTrue(provider.TryGetLevelData(new LevelId("level_105"), out var retrieved));
            Assert.AreEqual(4, retrieved.Width);
            Assert.AreEqual(15, retrieved.StartingMoves);
            Assert.AreEqual(800, retrieved.TargetScore);
        }

        [Test]
        public void InMemoryLevelDataProvider_MissingLevel_ReturnsFalse()
        {
            var provider = new InMemoryLevelDataProvider();

            Assert.IsFalse(provider.HasLevel(new LevelId("non_existent")));
            Assert.IsFalse(provider.TryGetLevelData(new LevelId("non_existent"), out _));
        }

        [Test]
        public void PuzzleVariantRegistry_RegistersAndResolvesVariantLogic()
        {
            var registry = new PuzzleVariantRegistry();
            registry.RegisterVariant("Mock", typeof(MockPuzzleLogic));

            Assert.IsTrue(registry.TryGetLogicType("Mock", out var resolvedType));
            Assert.AreEqual(typeof(MockPuzzleLogic), resolvedType);

            // Case-insensitive check
            Assert.IsTrue(registry.TryGetLogicType("MOCK", out var resolvedUpper));
            Assert.AreEqual(typeof(MockPuzzleLogic), resolvedUpper);

            Assert.IsFalse(registry.TryGetLogicType("Unknown", out _));
        }
    }
}
