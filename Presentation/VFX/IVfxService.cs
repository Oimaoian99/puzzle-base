using Puzzle.Core.Gameplay.Grid;
using Puzzle.Core.Gameplay.Pieces;
using UnityEngine;

namespace Puzzle.Presentation.VFX
{
    public interface IVfxService
    {
        void SpawnPieceVfx(string effectName, Vector3 worldPosition, PieceType pieceType = PieceType.None);
        void SpawnScreenVfx(string effectName);
        void ClearAllVfx();
    }
}
