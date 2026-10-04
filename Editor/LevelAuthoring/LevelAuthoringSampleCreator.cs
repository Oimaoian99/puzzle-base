using System.IO;
using Puzzle.Core.Gameplay.Data;
using Puzzle.Core.Gameplay.Grid;
using Puzzle.Core.Gameplay.Pieces;
using Puzzle.Core.Level;
using Puzzle.Variants.Link.Data;
using Puzzle.Variants.Match3.Data;
using UnityEditor;
using UnityEngine;

namespace Puzzle.Editor.LevelAuthoring
{
    /// <summary>
    /// Editor utility for generating representative sample level assets and level repositories.
    /// Run via menu: Tools > Puzzle Base > Generate Sample Level Assets
    /// </summary>
    public static class LevelAuthoringSampleCreator
    {
        [MenuItem("Tools/Puzzle Base/Generate Sample Level Assets")]
        public static void GenerateSampleAssets()
        {
            EnsureDirectory("Assets/Content/Levels/Link");
            EnsureDirectory("Assets/Content/Levels/Match3");
            EnsureDirectory("Assets/Content/Levels/Repositories");

            // 1. Create Link Level Asset
            string linkPath = "Assets/Content/Levels/Link/LinkLevel_001.asset";
            var linkAsset = AssetDatabase.LoadAssetAtPath<LinkLevelDataSO>(linkPath);
            if (linkAsset == null)
            {
                linkAsset = ScriptableObject.CreateInstance<LinkLevelDataSO>();
                AssetDatabase.CreateAsset(linkAsset, linkPath);
            }

            linkAsset.SetBaseConfig(
                id: "link_level_001",
                w: 6,
                h: 6,
                moves: 15,
                score: 1000);
            linkAsset.SetLinkConfig(linkLength: 3);
            EditorUtility.SetDirty(linkAsset);

            // 2. Create Match-3 Level Asset
            string match3Path = "Assets/Content/Levels/Match3/Match3Level_001.asset";
            var match3Asset = AssetDatabase.LoadAssetAtPath<Match3LevelDataSO>(match3Path);
            if (match3Asset == null)
            {
                match3Asset = ScriptableObject.CreateInstance<Match3LevelDataSO>();
                AssetDatabase.CreateAsset(match3Asset, match3Path);
            }

            match3Asset.SetBaseConfig(
                id: "match3_level_001",
                w: 6,
                h: 6,
                moves: 20,
                score: 1200);
            match3Asset.SetMatch3Config(matchLength: 3, refill: true);
            EditorUtility.SetDirty(match3Asset);

            // 3. Create Sample Repository Asset
            string repoPath = "Assets/Content/Levels/Repositories/MasterLevelRepository.asset";
            var repoAsset = AssetDatabase.LoadAssetAtPath<ScriptableObjectLevelRepository>(repoPath);
            if (repoAsset == null)
            {
                repoAsset = ScriptableObject.CreateInstance<ScriptableObjectLevelRepository>();
                AssetDatabase.CreateAsset(repoAsset, repoPath);
            }

            repoAsset.SetLevels(new LevelDataSO[] { linkAsset, match3Asset });
            EditorUtility.SetDirty(repoAsset);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[LevelAuthoring] Successfully created sample level assets:\n- {linkPath}\n- {match3Path}\n- {repoPath}");
        }

        private static void EnsureDirectory(string relativePath)
        {
            if (!Directory.Exists(relativePath))
            {
                Directory.CreateDirectory(relativePath);
            }
        }
    }
}
