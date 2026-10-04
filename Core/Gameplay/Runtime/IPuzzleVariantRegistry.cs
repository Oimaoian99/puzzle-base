using System;

namespace Puzzle.Core.Gameplay.Runtime
{
    /// <summary>
    /// Registry decoupling Core/Composition from specific variant assembly types.
    /// Allows variants to register their IPuzzleLogic types by variant identifier.
    /// </summary>
    public interface IPuzzleVariantRegistry
    {
        void RegisterVariant(string variantId, Type logicType);
        bool TryGetLogicType(string variantId, out Type logicType);
        Type GetLogicType(string variantId);
    }
}
