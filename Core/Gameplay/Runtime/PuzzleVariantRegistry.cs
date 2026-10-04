using System;
using System.Collections.Generic;
using Puzzle.Core.Gameplay.Puzzle;

namespace Puzzle.Core.Gameplay.Runtime
{
    /// <summary>
    /// Thread-safe registry mapping variant strings (e.g. "Link", "Match3")
    /// to their corresponding IPuzzleLogic implementation Types.
    /// Enables dynamic variant instantiation from authored LevelData.
    /// </summary>
    public class PuzzleVariantRegistry : IPuzzleVariantRegistry
    {
        private readonly Dictionary<string, Type> _registry = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

        public void RegisterVariant(string variantId, Type logicType)
        {
            if (string.IsNullOrWhiteSpace(variantId)) throw new ArgumentException("VariantId cannot be empty.", nameof(variantId));
            if (logicType == null) throw new ArgumentNullException(nameof(logicType));

            if (!typeof(IPuzzleLogic).IsAssignableFrom(logicType))
            {
                throw new ArgumentException($"Type '{logicType.Name}' must implement IPuzzleLogic.", nameof(logicType));
            }

            _registry[variantId.Trim()] = logicType;
        }

        public bool TryGetLogicType(string variantId, out Type logicType)
        {
            if (string.IsNullOrWhiteSpace(variantId))
            {
                logicType = null;
                return false;
            }

            return _registry.TryGetValue(variantId.Trim(), out logicType);
        }

        public Type GetLogicType(string variantId)
        {
            if (TryGetLogicType(variantId, out var type))
            {
                return type;
            }

            throw new KeyNotFoundException($"No puzzle logic registered for variant '{variantId}'.");
        }
    }
}
