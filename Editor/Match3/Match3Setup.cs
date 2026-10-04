using Puzzle.Variants.Match3.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace PuzzleBase.Editor.Match3
{
    /// <summary>
    /// Editor helper to configure a playable Match-3 Demo scene in Unity with a single click.
    /// </summary>
    public static class Match3Setup
    {
        [MenuItem("Tools/Puzzle Base/Setup Match-3 Demo Scene", priority = 210)]
        public static void SetupDemoScene()
        {
            // 1. Setup Camera
            var camera = Camera.main;
            if (camera == null)
            {
                var camGo = new GameObject("Main Camera");
                camera = camGo.AddComponent<Camera>();
                camera.tag = "MainCamera";
            }
            camera.transform.position = new Vector3(2.5f, 2.5f, -10f);
            camera.orthographic = true;
            camera.orthographicSize = 5f;

            // 2. Setup Match-3 Board View
            var boardView = Object.FindFirstObjectByType<Match3BoardView>();
            if (boardView == null)
            {
                var boardGo = new GameObject("Match3BoardView");
                boardView = boardGo.AddComponent<Match3BoardView>();
                boardView.CellSpacing = new Vector2(1.1f, 1.1f);
                boardView.Origin = Vector2.zero;
            }

            // 3. Setup Match-3 Game Runner
            var runner = Object.FindFirstObjectByType<Match3GameRunner>();
            if (runner == null)
            {
                var runnerGo = new GameObject("Match3GameRunner");
                runner = runnerGo.AddComponent<Match3GameRunner>();
            }

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log("[Match3Setup] Match-3 demo configured in active scene! Press 'Play' in Unity to play.");
        }
    }
}
