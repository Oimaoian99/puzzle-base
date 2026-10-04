using Puzzle.Core.Gameplay.Data;
using Puzzle.Core.Gameplay.Grid;
using Puzzle.Core.Gameplay.Runtime;
using Puzzle.Core.Level;
using Puzzle.Core.Logging;
using Puzzle.Variants.Match3.Data;
using Puzzle.Variants.Match3.Logic;
using UnityEngine;

namespace Puzzle.Variants.Match3.Presentation
{
    /// <summary>
    /// Standalone runner component to run and test the Match-3 puzzle variant directly in Unity.
    /// Demonstrates full end-to-end integration: LevelData -> PuzzleRuntime -> Match3PuzzleLogic -> Board -> Match3BoardView.
    /// </summary>
    public class Match3GameRunner : MonoBehaviour
    {
        [Header("Authoring Asset (Optional)")]
        [SerializeField] private Match3LevelDataSO customLevelAsset;

        [Header("Level Settings (Used if customLevelAsset is null)")]
        [SerializeField] private string levelId = "match3_sample_01";
        [SerializeField] private int width = 6;
        [SerializeField] private int height = 6;
        [SerializeField] private int startingMoves = 20;
        [SerializeField] private int targetScore = 1000;
        [SerializeField] private int randomSeed = 777;

        [Header("Presentation")]
        [SerializeField] private Match3BoardView boardView;
        [SerializeField] private Camera mainCamera;

        public PuzzleRuntime Runtime { get; private set; }
        public Match3PuzzleLogic Logic { get; private set; }
        public LevelRuntime LevelRuntime { get; private set; }

        private void Start()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }

            StartGame();
        }

        public void StartGame()
        {
            // 1. Generate or load level data
            LevelData levelData;
            if (customLevelAsset != null)
            {
                var validation = customLevelAsset.ValidateLevel();
                if (!validation.IsValid)
                {
                    CoreLogger.LogError($"[Match3GameRunner] Cannot run level: {validation}");
                    return;
                }
                levelData = customLevelAsset.ToLevelData();
            }
            else
            {
                levelData = Match3LevelFactory.CreateSampleLevel(
                    new LevelId(levelId),
                    width,
                    height,
                    startingMoves,
                    targetScore,
                    randomSeed);
            }

            // 2. Initialize Core components
            Logic = new Match3PuzzleLogic();
            Logic.SetRandomSeed(randomSeed);

            LevelRuntime = new LevelRuntime(new LevelId(levelId), startingMoves);
            LevelRuntime.OnLevelFinished += HandleLevelFinished;

            Runtime = new PuzzleRuntime(levelData, Logic, LevelRuntime);

            // 3. Connect presentation
            if (boardView == null)
            {
                var go = new GameObject("Match3BoardView");
                boardView = go.AddComponent<Match3BoardView>();
            }

            boardView.AttachLogic(Logic);
            boardView.Bind(Runtime.Board, command => Runtime.HandleInput(command));

            CoreLogger.Log($"[Match3GameRunner] Started level '{levelId}'. Target Score: {targetScore}, Moves: {startingMoves}");
        }

        private void Update()
        {
            if (Runtime != null && !Runtime.IsFinished)
            {
                Runtime.Tick(Time.deltaTime);
                HandleMouseInput();
            }
        }

        private void HandleMouseInput()
        {
            if (mainCamera == null || boardView == null || Runtime == null) return;

            if (Input.GetMouseButtonDown(0))
            {
                var gridPos = GetGridPositionUnderPointer();
                if (gridPos.HasValue)
                {
                    Logic.HandleTap(gridPos.Value);
                }
            }
        }

        private GridPosition? GetGridPositionUnderPointer()
        {
            if (mainCamera == null || boardView == null || Runtime == null) return null;

            var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            var plane = new Plane(Vector3.forward, Vector3.zero);

            if (plane.Raycast(ray, out float enter))
            {
                var worldHit = ray.GetPoint(enter);
                float colF = (worldHit.x - boardView.Origin.x) / boardView.CellSpacing.x;
                float rowF = (worldHit.y - boardView.Origin.y) / boardView.CellSpacing.y;

                int col = Mathf.RoundToInt(colF);
                int row = Mathf.RoundToInt(rowF);
                var pos = new GridPosition(col, row);

                if (Runtime.Board.IsInside(pos))
                {
                    return pos;
                }
            }

            return null;
        }

        private void HandleLevelFinished(LevelResultData result)
        {
            if (result.IsWin)
            {
                CoreLogger.Log($"[Match3GameRunner] VICTORY! Stars: {result.Stars}, Score: {result.Score}");
            }
            else
            {
                CoreLogger.Log($"[Match3GameRunner] DEFEAT! Out of moves. Final Score: {result.Score}");
            }
        }

        private void OnDestroy()
        {
            if (LevelRuntime != null)
            {
                LevelRuntime.OnLevelFinished -= HandleLevelFinished;
            }

            if (Runtime != null)
            {
                Runtime.Dispose();
                Runtime = null;
            }
        }
    }
}
