#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SetupStep2Fog
{
    [MenuItem("Unstable Experiment/Шаг 2 — Туман вокруг героя")]
    public static void AddFog()
    {
        var player = GameObject.Find("Player")?.transform;
        if (player == null)
        {
            EditorUtility.DisplayDialog("Шаг 2",
                "Не найден объект Player на сцене.\nСначала сделай Шаг 1.", "OK");
            return;
        }

        var cam = Camera.main;
        if (cam == null)
        {
            EditorUtility.DisplayDialog("Шаг 2", "Не найдена Main Camera.", "OK");
            return;
        }

        var fog = cam.GetComponent<VisionFog>();
        if (fog == null)
            fog = cam.gameObject.AddComponent<VisionFog>();

        fog.player = player;
        fog.innerRadius = 1.6f;
        fog.outerRadius = 3.2f;
        fog.falloffPower = 2.5f;
        fog.fogColor = new Color(0f, 0f, 0f, 1f);

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

        EditorUtility.DisplayDialog("Готово!",
            "Туман добавлен на камеру.\n\nCtrl+S → Play ▶\n\nНастрой радиус на Main Camera → Vision Fog:\n• Inner Radius — светлый круг\n• Outer Radius — темнота",
            "OK");
    }
}
#endif
