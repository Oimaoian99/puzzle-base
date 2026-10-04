using Puzzle.Variants.Link.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace PuzzleBase.Editor.LinkPuzzle
{
    /// <summary>
    /// Editor helper to configure a playable Link Puzzle demo scene in Unity with a single click.
    /// </summary>
    public static class LinkPuzzleSetup
    {
        [MenuItem("Tools/Puzzle Base/Setup Link Puzzle Demo Scene", priority = 200)]
        public static void SetupDemoScene()
        {
            // 1. Setup Camera if missing
            var camera = Camera.main;
            if (camera == null)
            {
                var camGo = new GameObject("Main Camera");
                camera = camGo.AddComponent<Camera>();
                camera.tag = "MainCamera";
                camGo.transform.position = new Vector3(2.5f, 2.5f, -10f);
                camera.orthographic = true;
                camera.orthographicSize = 5f;
            }
            else
            {
                camera.transform.position = new Vector3(2.5f, 2.5f, -10f);
                camera.orthographic = true;
                camera.orthographicSize = 5f;
            }

            // 2. Setup Link Board View
            var boardView = Object.FindFirstObjectByType<LinkBoardView>();
            if (boardView == null)
            {
                var boardGo = new GameObject("LinkBoardView");
                boardView = boardGo.AddComponent<LinkBoardView>();
                boardView.CellSpacing = new Vector2(1.1f, 1.1f);
                boardView.Origin = Vector2.zero;
            }

            // 3. Setup Link Game Runner
            var runner = Object.FindFirstObjectByType<LinkGameRunner>();
            if (runner == null)
            {
                var runnerGo = new GameObject("LinkGameRunner");
                runner = runnerGo.AddComponent<LinkGameRunner>();
            }

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log("[LinkPuzzleSetup] Link Puzzle demo configured in active scene! Press 'Play' in Unity to play.");
        }
    }
}
