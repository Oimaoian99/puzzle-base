using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace PuzzleBase.Editor.ProjectStructure
{
    /// <summary>
    /// Generates the standard production '_Project' folder hierarchy and master GameScene.
    /// Perfectly matches the production-grade folder layout:
    /// Assets/_Project/
    ///   ├── Animations/
    ///   ├── Animators/
    ///   ├── Datas/
    ///   ├── Fonts/
    ///   ├── Materials/
    ///   ├── Prefabs/
    ///   ├── Resources/
    ///   ├── Scenes/ (GameScene.unity)
    ///   ├── Scripts/
    ///   ├── Shaders/
    ///   ├── Sounds/
    ///   └── Sprites/
    /// </summary>
    public static class ProjectStructureGenerator
    {
        public const string RootFolder = "Assets/_Project";

        public static readonly string[] SubFolders = new[]
        {
            "Animations",
            "Animators",
            "Datas",
            "Fonts",
            "Materials",
            "Prefabs",
            "Resources",
            "Scenes",
            "Scripts",
            "Shaders",
            "Sounds",
            "Sprites"
        };

        [MenuItem("Tools/Puzzle Base/Project Setup/1. Generate Project Structure (_Project)", priority = 100)]
        public static void GenerateFolders()
        {
            EnsureFolderStructure();
            AssetDatabase.Refresh();

            var rootObj = AssetDatabase.LoadAssetAtPath<Object>(RootFolder);
            if (rootObj != null)
            {
                EditorGUIUtility.PingObject(rootObj);
                Selection.activeObject = rootObj;
            }

            Debug.Log($"[ProjectStructureGenerator] Successfully generated production folder structure at '{RootFolder}' with {SubFolders.Length} standard subdirectories!");
        }

        [MenuItem("Tools/Puzzle Base/Project Setup/2. Create Master GameScene (_Project/Scenes/GameScene.unity)", priority = 101)]
        public static void CreateMasterGameScene()
        {
            EnsureFolderStructure();
            string scenePath = $"{RootFolder}/Scenes/GameScene.unity";

            // Create a clean new scene
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Build the standard 5-tier production hierarchy with all scripts attached
            PuzzleBase.Editor.SceneSetup.ProductionSceneSetup.BuildProductionHierarchy();

            // Save the scene to Assets/_Project/Scenes/GameScene.unity
            EditorSceneManager.SaveScene(scene, scenePath);

            // Register in Build Settings as the primary scene
            AddSceneToBuildSettings(scenePath);

            AssetDatabase.Refresh();
            var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
            if (sceneAsset != null)
            {
                EditorGUIUtility.PingObject(sceneAsset);
                Selection.activeObject = sceneAsset;
            }

            Debug.Log($"[ProjectStructureGenerator] Successfully created and saved master GameScene at '{scenePath}' and registered into Build Settings (Index 0)!");
        }

        [MenuItem("Tools/Puzzle Base/Project Setup/3. Quick Setup Complete Project (_Project Folders + GameScene)", priority = 102)]
        public static void SetupCompleteProject()
        {
            GenerateFolders();
            CreateMasterGameScene();
            Debug.Log("[ProjectStructureGenerator] Full production project initialized successfully! Ready for development.");
        }

        public static void EnsureFolderStructure()
        {
            string fullRootPath = Path.Combine(Application.dataPath, "_Project");
            if (!Directory.Exists(fullRootPath))
            {
                Directory.CreateDirectory(fullRootPath);
            }

            foreach (var folder in SubFolders)
            {
                string fullSubPath = Path.Combine(fullRootPath, folder);
                if (!Directory.Exists(fullSubPath))
                {
                    Directory.CreateDirectory(fullSubPath);
                }
            }

            AssetDatabase.Refresh();
        }

        public static void AddSceneToBuildSettings(string scenePath)
        {
            var originalScenes = EditorBuildSettings.scenes;
            for (int i = 0; i < originalScenes.Length; i++)
            {
                if (originalScenes[i].path == scenePath)
                    return; // Already added
            }

            var newScenes = new EditorBuildSettingsScene[originalScenes.Length + 1];
            newScenes[0] = new EditorBuildSettingsScene(scenePath, true);
            for (int i = 0; i < originalScenes.Length; i++)
            {
                newScenes[i + 1] = originalScenes[i];
            }
            EditorBuildSettings.scenes = newScenes;
        }
    }
}
