using System.Collections.Generic;
using Puzzle.Core.Gameplay.Pieces;
using UnityEngine;

namespace Puzzle.Presentation.VFX
{
    public class MockVfxService : IVfxService
    {
        public struct SpawnedVfxRecord
        {
            public string EffectName;
            public Vector3 Position;
            public PieceType PieceType;
        }

        public List<SpawnedVfxRecord> SpawnedVfxLog { get; } = new List<SpawnedVfxRecord>();
        public List<string> SpawnedScreenVfxLog { get; } = new List<string>();

        public void SpawnPieceVfx(string effectName, Vector3 worldPosition, PieceType pieceType = PieceType.None)
        {
            SpawnedVfxLog.Add(new SpawnedVfxRecord
            {
                EffectName = effectName,
                Position = worldPosition,
                PieceType = pieceType
            });
        }

        public void SpawnScreenVfx(string effectName)
        {
            SpawnedScreenVfxLog.Add(effectName);
        }

        public void ClearAllVfx()
        {
            SpawnedVfxLog.Clear();
            SpawnedScreenVfxLog.Clear();
        }
    }
}
