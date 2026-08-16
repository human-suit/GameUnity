#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnstableExperiment.Core;
using UnstableExperiment.World;

namespace UnstableExperiment.Editor
{
    public static class DemoSceneCreator
    {
        [MenuItem("Unstable Experiment/Create Demo Scene")]
        public static void CreateDemoSceneMenu() => CreateDemoScene(silent: false);

        public static void CreateDemoScene(bool silent = false)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            var cam = Camera.main;
            if (cam != null)
            {
                cam.orthographic = true;
                cam.orthographicSize = 8f;
                cam.backgroundColor = new Color(0.05f, 0.05f, 0.06f);
                cam.transform.position = new Vector3(0, 0, -10);
                cam.gameObject.AddComponent<CameraFollow>();
            }

            var go = new GameObject("Game");
            go.AddComponent<GameBootstrap>();

            const string path = "Assets/Scenes/Main.unity";
            System.IO.Directory.CreateDirectory("Assets/Scenes");
            EditorSceneManager.SaveScene(scene, path);
            AssetDatabase.Refresh();

            if (!silent)
                EditorUtility.DisplayDialog("Unstable Experiment",
                    "Demo scene saved to Assets/Scenes/Main.unity\nPress Play to start Sector A.",
                    "OK");
        }
    }
}
#endif
