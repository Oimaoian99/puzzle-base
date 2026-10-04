using NUnit.Framework;
using Puzzle.Core.Level;

namespace Puzzle.Tests.Core.EditMode
{
    [TestFixture]
    public class LevelIdTests
    {
        [Test]
        public void Equality_SameString_AreEqual()
        {
            var id1 = new LevelId("level_001");
            var id2 = new LevelId("level_001");
            Assert.AreEqual(id1, id2);
            Assert.IsTrue(id1 == id2);
        }

        [Test]
        public void Equality_CaseInsensitive_AreEqual()
        {
            var id1 = new LevelId("Level_001");
            var id2 = new LevelId("level_001");
            Assert.AreEqual(id1, id2);
            Assert.AreEqual(id1.GetHashCode(), id2.GetHashCode());
        }

        [Test]
        public void TryGetIntIndex_ValidInteger_ReturnsTrueAndValue()
        {
            var id = new LevelId(42);
            Assert.IsTrue(id.TryGetIntIndex(out var idx));
            Assert.AreEqual(42, idx);
        }

        [Test]
        public void TryGetIntIndex_NonInteger_ReturnsFalse()
        {
            var id = new LevelId("bonus_stage_A");
            Assert.IsFalse(id.TryGetIntIndex(out _));
        }

        [Test]
        public void ImplicitConversions_WorkSeamlessly()
        {
            LevelId fromStr = "stage_1";
            LevelId fromInt = 5;

            Assert.AreEqual("stage_1", fromStr.Value);
            Assert.AreEqual("5", fromInt.Value);
        }

        [Test]
        public void Comparison_NumericLevels_ComparesByInt()
        {
            LevelId level2 = 2;
            LevelId level10 = 10;

            Assert.Less(level2.CompareTo(level10), 0);
        }
    }
}
