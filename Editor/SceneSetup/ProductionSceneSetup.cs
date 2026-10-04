using System.IO;
using Puzzle.Presentation.UI;
using Puzzle.Variants.Match3.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PuzzleBase.Editor.SceneSetup
{
    /// <summary>
    /// Editor helper to configure a production-grade 5-tier Scene Hierarchy and complete UI Canvas with a single click.
    /// Menu: Tools > Puzzle Base > Setup Complete Production Scene (World + UI Canvas)
    /// </summary>
    public static class ProductionSceneSetup
    {
        [MenuItem("Tools/Puzzle Base/Setup Complete Production Scene (World + UI Canvas)", priority = 220)]
        public static void SetupCompleteProductionScene()
        {
            // 1. [00_APP_SERVICES]
            var appServices = GetOrCreateRoot("[00_APP_SERVICES]");
            var eventSystem = Object.FindFirstObjectByType<EventSystem>();
            if (eventSystem == null)
            {
                var esGo = new GameObject("EventSystem");
                esGo.transform.SetParent(appServices.transform);
                eventSystem = esGo.AddComponent<EventSystem>();
                esGo.AddComponent<StandaloneInputModule>();
            }
            else
            {
                eventSystem.transform.SetParent(appServices.transform);
            }

            // 2. [01_SCENE_FLOW]
            var sceneFlow = GetOrCreateRoot("[01_SCENE_FLOW]");

            // 3. [02_ENVIRONMENT & CAMERAS]
            var envCameras = GetOrCreateRoot("[02_ENVIRONMENT & CAMERAS]");
            var camera = Camera.main;
            if (camera == null)
            {
                var camGo = new GameObject("Main Camera");
                camGo.transform.SetParent(envCameras.transform);
                camera = camGo.AddComponent<Camera>();
                camera.tag = "MainCamera";
            }
            else
            {
                camera.transform.SetParent(envCameras.transform);
            }
            camera.transform.position = new Vector3(2.5f, 2.5f, -10f);
            camera.orthographic = true;
            camera.orthographicSize = 5f;

            // 4. [03_GAMEPLAY_WORLD]
            var gameplayWorld = GetOrCreateRoot("[03_GAMEPLAY_WORLD]");
            var boardRoot = GetOrCreateChild(gameplayWorld.transform, "BoardRoot");
            var boardView = Object.FindFirstObjectByType<Match3BoardView>();
            if (boardView == null)
            {
                var bvGo = new GameObject("Match3BoardView");
                bvGo.transform.SetParent(boardRoot.transform);
                boardView = bvGo.AddComponent<Match3BoardView>();
                boardView.CellSpacing = new Vector2(1.1f, 1.1f);
                boardView.Origin = Vector2.zero;
            }
            else
            {
                boardView.transform.SetParent(boardRoot.transform);
            }

            // 5. [04_UI_ROOT]
            var uiRoot = GetOrCreateRoot("[04_UI_ROOT]");
            var canvas = Object.FindFirstObjectByType<Canvas>();
            GameObject canvasGo;
            if (canvas == null)
            {
                canvasGo = new GameObject("MainCanvas");
                canvasGo.transform.SetParent(uiRoot.transform);
                canvas = canvasGo.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                var scaler = canvasGo.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080, 1920);
                scaler.matchWidthOrHeight = 0.5f;
                canvasGo.AddComponent<GraphicRaycaster>();
            }
            else
            {
                canvasGo = canvas.gameObject;
                canvasGo.transform.SetParent(uiRoot.transform);
            }

            var uiManager = canvasGo.GetComponent<UIManager>();
            if (uiManager == null)
            {
                uiManager = canvasGo.AddComponent<UIManager>();
            }

            // Setup UI Layers inside Canvas
            var screensLayer = GetOrCreateChild(canvasGo.transform, "01_Screens");
            var hudLayer = GetOrCreateChild(canvasGo.transform, "02_HUD");
            var popupsLayer = GetOrCreateChild(canvasGo.transform, "03_Popups");
            var overlaysLayer = GetOrCreateChild(canvasGo.transform, "04_Overlays");

            // Create Screen, HUD, and Popups if missing
            GetOrCreateUIWindow<HomeScreenView>(screensLayer.transform, "HomeScreenView");
            GetOrCreateUIWindow<GameplayHUDView>(hudLayer.transform, "GameplayHUDView");
            GetOrCreateUIWindow<PausePopupView>(popupsLayer.transform, "PausePopupView");
            GetOrCreateUIWindow<WinResultPopupView>(popupsLayer.transform, "WinResultPopupView");
            GetOrCreateUIWindow<LoseResultPopupView>(popupsLayer.transform, "LoseResultPopupView");
            GetOrCreateUIWindow<LoadingOverlayView>(overlaysLayer.transform, "LoadingOverlayView");

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log("[ProductionSceneSetup] Complete 5-tier production Scene Hierarchy and UI Canvas created successfully!");
        }

        private static GameObject GetOrCreateRoot(string name)
        {
            var go = GameObject.Find(name);
            if (go == null)
            {
                go = new GameObject(name);
                go.transform.position = Vector3.zero;
            }
            return go;
        }

        private static GameObject GetOrCreateChild(Transform parent, string childName)
        {
            var child = parent.Find(childName);
            if (child != null) return child.gameObject;

            var go = new GameObject(childName);
            go.transform.SetParent(parent, false);
            return go;
        }

        private static T GetOrCreateUIWindow<T>(Transform parent, string windowName) where T : UIWindow
        {
            var child = parent.Find(windowName);
            if (child != null)
            {
                var existing = child.GetComponent<T>();
                if (existing != null) return existing;
                return child.gameObject.AddComponent<T>();
            }

            var go = new GameObject(windowName, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            var window = go.AddComponent<T>();
            go.SetActive(false); // Popups/Screens start inactive by default
            return window;
        }
    }
}
