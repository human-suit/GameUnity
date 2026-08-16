#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace UnstableExperiment.Editor
{
    [InitializeOnLoad]
    public static class PlayModeHint
    {
        static PlayModeHint()
        {
            SceneView.duringSceneGui += OnSceneGui;
        }

        private static void OnSceneGui(SceneView view)
        {
            if (Application.isPlaying) return;

            Handles.BeginGUI();
            var rect = new Rect(12, 12, 420, 56);
            GUI.Box(rect, GUIContent.none);
            GUI.Label(new Rect(20, 18, 400, 44),
                "Unstable Experiment\n▶ Play  ·  вкладка Game (не Scene)  ·  WASD + E");
            Handles.EndGUI();
        }
    }
}
#endif
