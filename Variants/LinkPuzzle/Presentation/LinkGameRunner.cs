using Puzzle.Core.Gameplay.Data;
using Puzzle.Core.Gameplay.Grid;
using Puzzle.Core.Gameplay.Runtime;
using Puzzle.Core.Level;
using Puzzle.Core.Logging;
using Puzzle.Variants.Link.Data;
using Puzzle.Variants.Link.Logic;
using UnityEngine;

namespace Puzzle.Variants.Link.Presentation
{
    /// <summary>
    /// Standalone runner component to run and test the Link/Line puzzle variant directly in Unity.
    /// Demonstrates full end-to-end integration: LevelData -> PuzzleRuntime -> LinkPuzzleLogic -> Board -> LinkBoardView.
    /// </summary>
    public class LinkGameRunner : MonoBehaviour
    {
        [Header("Authoring Asset (Optional)")]
        [SerializeField] private LinkLevelDataSO customLevelAsset;

        [Header("Level Settings (Used if customLevelAsset is null)")]
        [SerializeField] private string levelId = "link_sample_01";
        [SerializeField] private int width = 6;
        [SerializeField] private int height = 6;
        [SerializeField] private int startingMoves = 15;
        [SerializeField] private int targetScore = 1000;
        [SerializeField] private int randomSeed = 42;

        [Header("Presentation")]
        [SerializeField] private LinkBoardView boardView;
        [SerializeField] private Camera mainCamera;

        public PuzzleRuntime Runtime { get; private set; }
        public LinkPuzzleLogic Logic { get; private set; }
        public LevelRuntime LevelRuntime { get; private set; }

        private bool _isPointerDragging;

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
                    CoreLogger.LogError($"[LinkGameRunner] Cannot run level: {validation}");
                    return;
                }
                levelData = customLevelAsset.ToLevelData();
            }
            else
            {
                levelData = LinkLevelFactory.CreateSampleLevel(
                    new LevelId(levelId),
                    width,
                    height,
                    startingMoves,
                    targetScore,
                    randomSeed);
            }

            // 2. Initialize Core components
            Logic = new LinkPuzzleLogic();
            Logic.SetRandomSeed(randomSeed);

            LevelRuntime = new LevelRuntime(new LevelId(levelId), startingMoves);
            LevelRuntime.OnLevelFinished += HandleLevelFinished;

            Runtime = new PuzzleRuntime(levelData, Logic, LevelRuntime);

            // 3. Connect presentation
            if (boardView == null)
            {
                var go = new GameObject("LinkBoardView");
                boardView = go.AddComponent<LinkBoardView>();
            }

            boardView.AttachLogic(Logic);
            boardView.Bind(Runtime.Board, command => Runtime.HandleInput(command));

            CoreLogger.Log($"[LinkGameRunner] Started level '{levelId}'. Target Score: {targetScore}, Moves: {startingMoves}");
        }

        private void Update()
        {
            if (Runtime != null && !Runtime.IsFinished)
            {
                Runtime.Tick(Time.deltaTime);
                HandlePointerInput();
            }
        }

        private void HandlePointerInput()
        {
            if (mainCamera == null || boardView == null) return;

            // Touch / Mouse Drag Detection
            if (Input.GetMouseButtonDown(0))
            {
                var gridPos = GetGridPositionUnderPointer();
                if (gridPos.HasValue)
                {
                    _isPointerDragging = true;
                    boardView.StartDrag(gridPos.Value);
                }
            }
            else if (Input.GetMouseButton(0) && _isPointerDragging)
            {
                var gridPos = GetGridPositionUnderPointer();
                if (gridPos.HasValue)
                {
                    boardView.ExtendDrag(gridPos.Value);
                }
            }
            else if (Input.GetMouseButtonUp(0) && _isPointerDragging)
            {
                _isPointerDragging = false;
                boardView.EndDrag();
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
                // Invert GridToWorldPosition
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
                CoreLogger.Log($"[LinkGameRunner] VICTORY! Stars: {result.Stars}, Score: {result.Score}");
            }
            else
            {
                CoreLogger.Log($"[LinkGameRunner] DEFEAT! Out of moves. Final Score: {result.Score}");
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
