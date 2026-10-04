using NUnit.Framework;
using Puzzle.Core.Input.Commands;
using UnityEngine;

namespace Puzzle.Tests.Core.EditMode
{
    [TestFixture]
    public class InputCommandTests
    {
        [Test]
        public void TapCommand_StoresCoordinatesAccurately()
        {
            var tap = new TapCommand(new Vector2(100f, 200f), new Vector2Int(3, 4), 1.5f);
            Assert.AreEqual("Tap", tap.CommandName);
            Assert.AreEqual(new Vector2(100f, 200f), tap.ScreenPosition);
            Assert.AreEqual(new Vector2Int(3, 4), tap.GridCoordinate);
            Assert.AreEqual(1.5f, tap.Timestamp);
        }

        [Test]
        public void DragCommand_StoresDeltaAndPhase()
        {
            var drag = new DragCommand(new Vector2(10f, 10f), new Vector2(25f, 30f), new Vector2(15f, 20f), DragPhase.Delta, 0.2f);
            Assert.AreEqual("Drag", drag.CommandName);
            Assert.AreEqual(DragPhase.Delta, drag.Phase);
            Assert.AreEqual(new Vector2(15f, 20f), drag.Delta);
        }

        [Test]
        public void SwipeCommand_StoresDirectionAndOrigin()
        {
            var swipe = new SwipeCommand(new Vector2Int(1, 1), SwipeDirection.Up, 0.5f);
            Assert.AreEqual("Swipe", swipe.CommandName);
            Assert.AreEqual(SwipeDirection.Up, swipe.Direction);
            Assert.AreEqual(new Vector2Int(1, 1), swipe.OriginGridCoordinate);
        }
    }
}
