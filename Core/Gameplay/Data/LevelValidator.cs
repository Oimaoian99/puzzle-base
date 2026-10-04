using System;
using System.Collections.Generic;
using Puzzle.Core.Gameplay.Grid;
using Puzzle.Core.Gameplay.Pieces;

namespace Puzzle.Core.Gameplay.Data
{
    /// <summary>
    /// Validates authoring LevelData before runtime consumption.
    /// Detects out-of-bounds pieces, blocked cell collisions, invalid dimensions,
    /// duplicate placements, and variant-specific configuration errors.
    /// Pure C#, headless-friendly, readable and maintainable by Junior developers.
    /// </summary>
    public class LevelValidator
    {
        public const int MaxSupportedDimension = 32;

        public virtual ValidationResult Validate(LevelData data)
        {
            var result = new ValidationResult();

            if (data == null)
            {
                result.AddError("LevelData is null.");
                return result;
            }

            string levelName = data.Id.IsEmpty ? "<Unset>" : data.Id.Value;

            // 1. Level ID validation
            if (data.Id.IsEmpty || string.IsNullOrWhiteSpace(data.Id.Value))
            {
                result.AddError($"Level {levelName}: Missing Level ID.");
            }

            // 2. Board dimension validation
            if (data.Width <= 0 || data.Height <= 0)
            {
                result.AddError($"Level {levelName}: Invalid board size {data.Width}x{data.Height}. Board dimensions must be greater than zero.");
            }
            else if (data.Width > MaxSupportedDimension || data.Height > MaxSupportedDimension)
            {
                result.AddError($"Level {levelName}: Board size {data.Width}x{data.Height} exceeds maximum supported dimension of {MaxSupportedDimension}x{MaxSupportedDimension}.");
            }

            // 3. Goal & constraint validation
            if (data.StartingMoves <= 0)
            {
                result.AddError($"Level {levelName}: StartingMoves must be greater than 0 (got {data.StartingMoves}).");
            }

            if (data.TargetScore <= 0)
            {
                result.AddError($"Level {levelName}: TargetScore must be greater than 0 (got {data.TargetScore}).");
            }

            // If board dimensions are invalid, skip coordinate checking to prevent misleading secondary errors
            if (data.Width <= 0 || data.Height <= 0)
            {
                return result;
            }

            // 4. Blocked cells validation
            var blockedSet = new HashSet<GridPosition>();
            if (data.BlockedCells != null)
            {
                for (int i = 0; i < data.BlockedCells.Count; i++)
                {
                    var pos = data.BlockedCells[i];
                    if (pos.X < 0 || pos.X >= data.Width || pos.Y < 0 || pos.Y >= data.Height)
                    {
                        result.AddError($"Level {levelName}: Blocked cell at ({pos.X}, {pos.Y}) is outside board size {data.Width}x{data.Height}.");
                    }
                    else if (!blockedSet.Add(pos))
                    {
                        result.AddWarning($"Level {levelName}: Duplicate blocked cell declared at ({pos.X}, {pos.Y}).");
                    }
                }
            }

            // 5. Initial pieces validation
            var occupiedPositions = new HashSet<GridPosition>();
            if (data.InitialPieces != null)
            {
                for (int i = 0; i < data.InitialPieces.Count; i++)
                {
                    var piece = data.InitialPieces[i];
                    if (piece == null)
                    {
                        result.AddError($"Level {levelName}: Initial piece at index {i} is null.");
                        continue;
                    }

                    var pos = piece.Position;

                    // Bounds check
                    if (pos.X < 0 || pos.X >= data.Width || pos.Y < 0 || pos.Y >= data.Height)
                    {
                        result.AddError($"Level {levelName}: Piece '{piece.PieceId}' ({piece.Type}) at ({pos.X}, {pos.Y}) is outside board size {data.Width}x{data.Height}.");
                        continue;
                    }

                    // Blocked cell collision
                    if (blockedSet.Contains(pos))
                    {
                        result.AddError($"Level {levelName}: Piece '{piece.PieceId}' at ({pos.X}, {pos.Y}) placed on a blocked cell.");
                    }

                    // Duplicate coordinate collision
                    if (!occupiedPositions.Add(pos))
                    {
                        result.AddError($"Level {levelName}: Duplicate piece placement at ({pos.X}, {pos.Y}). Multiple pieces share the same position.");
                    }

                    // Type validation
                    if (piece.Type == PieceType.None)
                    {
                        result.AddWarning($"Level {levelName}: Piece at ({pos.X}, {pos.Y}) has type 'None'.");
                    }
                }
            }

            // 6. Variant-specific validation hook
            ValidateVariantSpecific(data, levelName, result);

            return result;
        }

        private static readonly Dictionary<string, Action<LevelData, string, ValidationResult>> CustomVariantValidators
            = new Dictionary<string, Action<LevelData, string, ValidationResult>>(StringComparer.OrdinalIgnoreCase);

        public static void RegisterVariantValidator(string variantId, Action<LevelData, string, ValidationResult> validator)
        {
            if (string.IsNullOrWhiteSpace(variantId) || validator == null) return;
            CustomVariantValidators[variantId.Trim()] = validator;
        }

        protected virtual void ValidateVariantSpecific(LevelData data, string levelName, ValidationResult result)
        {
            if (!string.IsNullOrEmpty(data.VariantId) && CustomVariantValidators.TryGetValue(data.VariantId, out var validator))
            {
                validator.Invoke(data, levelName, result);
            }
        }
    }
}
