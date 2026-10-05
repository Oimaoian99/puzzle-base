using System.IO;
using Puzzle.Composition;
using Puzzle.Presentation.Feedback;
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
    /// Editor helper to configure a complete production-grade 5-tier Scene Hierarchy and UI Canvas with a single click.
    /// Menu: Tools > Puzzle Base > Setup Complete Production Scene (World + UI Canvas)
    /// </summary>
    public static class ProductionSceneSetup
    {
        [MenuItem("Tools/Puzzle Base/Scene Setup/Setup 5-Tier Hierarchy in Current Scene", priority = 220)]
        public static void SetupCompleteProductionScene()
        {
            BuildProductionHierarchy();
        }

        public static void BuildProductionHierarchy()
        {
            // 1. [00_APP_SERVICES]
            var appServices = GetOrCreateRoot("[00_APP_SERVICES]");
            
            var rootScope = Object.FindFirstObjectByType<RootLifetimeScope>();
            if (rootScope == null)
            {
                var rootScopeGo = GetOrCreateChild(appServices.transform, "RootLifetimeScope");
                rootScope = rootScopeGo.AddComponent<RootLifetimeScope>();
            }
            else
            {
                rootScope.transform.SetParent(appServices.transform);
            }

            var eventSystem = Object.FindFirstObjectByType<EventSystem>();
            if (eventSystem == null)
            {
                var esGo = GetOrCreateChild(appServices.transform, "EventSystem");
                eventSystem = esGo.AddComponent<EventSystem>();
                esGo.AddComponent<StandaloneInputModule>();
            }
            else
            {
                eventSystem.transform.SetParent(appServices.transform);
            }

            // 2. [01_SCENE_FLOW]
            var sceneFlow = GetOrCreateRoot("[01_SCENE_FLOW]");
            var levelScope = Object.FindFirstObjectByType<LevelLifetimeScope>();
            if (levelScope == null)
            {
                var levelScopeGo = GetOrCreateChild(sceneFlow.transform, "LevelLifetimeScope");
                levelScope = levelScopeGo.AddComponent<LevelLifetimeScope>();
            }
            else
            {
                levelScope.transform.SetParent(sceneFlow.transform);
            }

            // 3. [02_ENVIRONMENT & CAMERAS]
            var envCameras = GetOrCreateRoot("[02_ENVIRONMENT & CAMERAS]");
            var camera = Camera.main;
            if (camera == null)
            {
                var camGo = GetOrCreateChild(envCameras.transform, "Main Camera");
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
            var boardView = boardRoot.GetComponent<Match3BoardView>();
            if (boardView == null)
            {
                boardView = boardRoot.AddComponent<Match3BoardView>();
                boardView.CellSpacing = new Vector2(1.1f, 1.1f);
                boardView.Origin = Vector2.zero;
            }

            // 5. [04_UI_ROOT]
            var uiRoot = GetOrCreateRoot("[04_UI_ROOT]");
            var canvas = Object.FindFirstObjectByType<Canvas>();
            GameObject canvasGo;
            if (canvas == null)
            {
                canvasGo = GetOrCreateChild(uiRoot.transform, "MainCanvas");
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

            // Create Screen, HUD, Popups with CanvasGroup attached and visible in Inspector
            GetOrCreateUIWindow<HomeScreenView>(screensLayer.transform, "HomeScreenView", startVisible: true);
            GetOrCreateUIWindow<GameplayHUDView>(hudLayer.transform, "GameplayHUDView", startVisible: false);
            GetOrCreateUIWindow<PausePopupView>(popupsLayer.transform, "PausePopupView", startVisible: false);
            GetOrCreateUIWindow<WinResultPopupView>(popupsLayer.transform, "WinResultPopupView", startVisible: false);
            GetOrCreateUIWindow<LoseResultPopupView>(popupsLayer.transform, "LoseResultPopupView", startVisible: false);
            GetOrCreateUIWindow<LoadingOverlayView>(overlaysLayer.transform, "LoadingOverlayView", startVisible: false);

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log("[ProductionSceneSetup] Complete 5-tier production Scene Hierarchy with all scripts attached created successfully!");
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

        private static T GetOrCreateUIWindow<T>(Transform parent, string windowName, bool startVisible) where T : UIWindow
        {
            var child = parent.Find(windowName);
            GameObject go;
            T window;

            if (child != null)
            {
                go = child.gameObject;
                window = go.GetComponent<T>() ?? go.AddComponent<T>();
            }
            else
            {
                go = new GameObject(windowName, typeof(RectTransform));
                go.transform.SetParent(parent, false);
                var rect = go.GetComponent<RectTransform>();
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                window = go.AddComponent<T>();
            }

            var canvasGroup = go.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = go.AddComponent<CanvasGroup>();
            }

            canvasGroup.alpha = startVisible ? 1f : 0f;
            canvasGroup.blocksRaycasts = startVisible;
            canvasGroup.interactable = startVisible;

            go.SetActive(true); // Keep GameObject active so it is clearly visible in Hierarchy and inspectable
            return window;
        }
    }
}
