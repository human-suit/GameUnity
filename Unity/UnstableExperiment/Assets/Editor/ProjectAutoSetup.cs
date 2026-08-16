#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace UnstableExperiment.Editor
{
    [InitializeOnLoad]
    public static class ProjectAutoSetup
    {
        private const string ScenePath = "Assets/Scenes/Main.unity";

        static ProjectAutoSetup()
        {
            EditorApplication.delayCall += EnsureProjectReady;
        }

        private static void EnsureProjectReady()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (!Directory.Exists("Assets/Scenes"))
                Directory.CreateDirectory("Assets/Scenes");

            if (!File.Exists(ScenePath))
                DemoSceneCreator.CreateDemoScene(silent: true);

            EnsureBuildSettings();
            ConfigureImportedArt();
        }

        private static void EnsureBuildSettings()
        {
            var scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
            if (scene == null) return;
            var scenes = EditorBuildSettings.scenes;
            if (scenes.Length == 1 && scenes[0].path == ScenePath) return;
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(ScenePath, true)
            };
        }

        private static void ConfigureImportedArt()
        {
            SetTexture("Assets/Resources/Art/Rooms/a_plaza_room.png", 100f, FilterMode.Bilinear);
            SetTexture("Assets/Resources/Art/UI/route_map_sector_a.png", 100f, FilterMode.Bilinear);
            SetTexture("Assets/Resources/Art/Environment/combat_bg_sector_a.png", 100f, FilterMode.Bilinear);
            SetSpriteSheet("Assets/Resources/Art/Characters/subject_07_unified_sheet.png", 100f);
            SetSpriteSheet("Assets/Resources/Art/Characters/subject_03_unified_sheet.png", 100f);

            CreateIconFromSheet(
                "Assets/Resources/Art/Characters/subject_07_unified_sheet.png",
                "Assets/Resources/Art/Characters/subject_07_icon.png",
                new Rect(0.55f, 0.72f, 0.12f, 0.18f));

            CreateIconFromSheet(
                "Assets/Resources/Art/Characters/subject_03_unified_sheet.png",
                "Assets/Resources/Art/Characters/subject_03_icon.png",
                new Rect(0.55f, 0.72f, 0.12f, 0.18f));
        }

        private static void SetTexture(string path, float ppu, FilterMode filter)
        {
            if (!File.Exists(path)) return;
            var imp = AssetImporter.GetAtPath(path) as TextureImporter;
            if (imp == null) return;
            imp.textureType = TextureImporterType.Sprite;
            imp.spriteImportMode = SpriteImportMode.Single;
            imp.spritePixelsPerUnit = ppu;
            imp.filterMode = filter;
            imp.SaveAndReimport();
        }

        private static void SetSpriteSheet(string path, float ppu)
        {
            if (!File.Exists(path)) return;
            var imp = AssetImporter.GetAtPath(path) as TextureImporter;
            if (imp == null) return;
            imp.textureType = TextureImporterType.Sprite;
            imp.spriteImportMode = SpriteImportMode.Single;
            imp.spritePixelsPerUnit = ppu;
            imp.filterMode = FilterMode.Bilinear;
            imp.isReadable = true;
            imp.SaveAndReimport();
        }

        private static void CreateIconFromSheet(string sheetPath, string iconPath, Rect uvRect)
        {
            if (File.Exists(iconPath)) return;
            var sheet = AssetDatabase.LoadAssetAtPath<Texture2D>(sheetPath);
            if (sheet == null) return;

            int x = Mathf.RoundToInt(uvRect.x * sheet.width);
            int y = Mathf.RoundToInt(uvRect.y * sheet.height);
            int w = Mathf.Max(8, Mathf.RoundToInt(uvRect.width * sheet.width));
            int h = Mathf.Max(8, Mathf.RoundToInt(uvRect.height * sheet.height));
            y = Mathf.Clamp(sheet.height - y - h, 0, sheet.height - h);
            x = Mathf.Clamp(x, 0, sheet.width - w);

            var pixels = sheet.GetPixels(x, y, w, h);
            var icon = new Texture2D(w, h, TextureFormat.RGBA32, false);
            icon.SetPixels(pixels);
            icon.Apply();
            File.WriteAllBytes(iconPath, icon.EncodeToPNG());
            AssetDatabase.Refresh();
            SetTexture(iconPath, 64f, FilterMode.Bilinear);
        }

        [MenuItem("Unstable Experiment/Reimport Art")]
        public static void ReimportArtMenu()
        {
            ConfigureImportedArt();
            EditorUtility.DisplayDialog("Unstable Experiment", "Art reimported.", "OK");
        }
    }
}
#endif
