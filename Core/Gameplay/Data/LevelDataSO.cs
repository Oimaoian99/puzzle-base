using System;
using System.Collections.Generic;
using Puzzle.Core.Gameplay.Grid;
using Puzzle.Core.Level;
using UnityEngine;

namespace Puzzle.Core.Gameplay.Data
{
    /// <summary>
    /// Base ScriptableObject for authoring level configurations inside Unity Editor.
    /// Designers edit level properties in the standard Unity Inspector without modifying C# code.
    /// Converts to pure C# LevelData for runtime consumption.
    /// </summary>
    public abstract class LevelDataSO : ScriptableObject
    {
        [Header("Level Identification")]
        [SerializeField] private string levelId = "level_001";

        [Header("Board Dimensions")]
        [SerializeField, Range(2, 32)] private int width = 6;
        [SerializeField, Range(2, 32)] private int height = 6;

        [Header("Session Rules")]
        [SerializeField, Min(1)] private int startingMoves = 20;
        [SerializeField, Min(1)] private int targetScore = 1000;

        [Header("Board Layout")]
        [SerializeField] private List<GridPosition> blockedCells = new List<GridPosition>();
        [SerializeField] private List<InitialPieceSetup> initialPieces = new List<InitialPieceSetup>();

        public LevelId LevelId => new LevelId(levelId);
        public int Width => width;
        public int Height => height;
        public int StartingMoves => startingMoves;
        public int TargetScore => targetScore;
        public IReadOnlyList<GridPosition> BlockedCells => blockedCells;
        public IReadOnlyList<InitialPieceSetup> InitialPieces => initialPieces;

        public abstract string VariantId { get; }

        public abstract LevelData ToLevelData();

        protected void PopulateBaseLevelData(LevelData target)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));

            target.Id = LevelId;
            target.VariantId = VariantId;
            target.Width = width;
            target.Height = height;
            target.StartingMoves = startingMoves;
            target.TargetScore = targetScore;
            target.BlockedCells = new List<GridPosition>(blockedCells ?? new List<GridPosition>());
            target.InitialPieces = new List<InitialPieceSetup>(initialPieces ?? new List<InitialPieceSetup>());
        }

        public void SetBaseConfig(string id, int w, int h, int moves, int score, List<GridPosition> blocked = null, List<InitialPieceSetup> pieces = null)
        {
            levelId = id;
            width = w;
            height = h;
            startingMoves = moves;
            targetScore = score;
            blockedCells = blocked ?? new List<GridPosition>();
            initialPieces = pieces ?? new List<InitialPieceSetup>();
        }

        [ContextMenu("Validate Level")]
        public ValidationResult ValidateLevel()
        {
            var data = ToLevelData();
            var validator = new LevelValidator();
            var result = validator.Validate(data);

            if (result.IsValid)
            {
                Debug.Log($"[LevelValidation] '{levelId}' ({VariantId}): VALID. 0 errors, {result.Warnings.Count} warning(s).", this);
            }
            else
            {
                Debug.LogError($"[LevelValidation] '{levelId}' ({VariantId}): INVALID.\n{result}", this);
            }

            return result;
        }

        private void OnValidate()
        {
            // Lightweight clamp to keep inspector inputs sane
            if (width < 1) width = 1;
            if (height < 1) height = 1;
            if (startingMoves < 1) startingMoves = 1;
            if (targetScore < 1) targetScore = 1;
        }
    }
}
