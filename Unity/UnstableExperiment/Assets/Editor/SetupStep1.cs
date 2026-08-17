#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Меню Unity: Unstable Experiment → Шаг 1...
/// Собирает сцену автоматически — не нужно ничего перетаскивать руками.
/// </summary>
public static class SetupStep1
{
    private const string RoomSpritePath = "Assets/Resources/Art/Rooms/a_plaza_room.png";
    private const string PlayerSpritePath = "Assets/Resources/Art/Characters/subject_07_icon.png";

    [MenuItem("Unstable Experiment/Шаг 1 — Площадь и персонаж")]
    public static void BuildPlazaScene()
    {
        if (!EditorUtility.DisplayDialog(
                "Шаг 1",
                "Соберу сцену:\n• фон площади\n• персонаж Subject 07\n• камера следует за ним\n• WASD для ходьбы\n\nПродолжить?",
                "Да", "Отмена"))
            return;

        var roomSprite = AssetDatabase.LoadAssetAtPath<Sprite>(RoomSpritePath);
        var playerSprite = AssetDatabase.LoadAssetAtPath<Sprite>(PlayerSpritePath);

        if (roomSprite == null || playerSprite == null)
        {
            EditorUtility.DisplayDialog("Ошибка",
                "Не найдены спрайты в Resources/Art/.\nПроверь что PNG на месте.", "OK");
            return;
        }

        RemoveIfExists("Room");
        RemoveIfExists("Player");

        // --- Комната (фон) ---
        var room = new GameObject("Room");
        var roomSr = room.AddComponent<SpriteRenderer>();
        roomSr.sprite = roomSprite;
        roomSr.sortingOrder = 0;
        FitSpriteHeight(room.transform, roomSr, 16f);

        // --- Игрок ---
        var player = new GameObject("Player");
        player.transform.position = new Vector3(0f, -1f, 0f);

        var playerSr = player.AddComponent<SpriteRenderer>();
        playerSr.sprite = playerSprite;
        playerSr.sortingOrder = 10;
        FitSpriteHeight(player.transform, playerSr, 1.4f);

        var rb = player.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        player.AddComponent<CircleCollider2D>().radius = 0.25f;
        player.AddComponent<PlayerMove>();

        // --- Камера ---
        var cam = Camera.main;
        if (cam != null)
        {
            cam.orthographic = true;
            cam.orthographicSize = 8f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.05f, 0.05f, 0.06f);
            cam.transform.position = new Vector3(0f, 0f, -10f);

            var follow = cam.GetComponent<CameraFollow2D>();
            if (follow == null)
                follow = cam.gameObject.AddComponent<CameraFollow2D>();
            follow.target = player.transform;
            follow.smooth = 8f;

            var fog = cam.GetComponent<VisionFog>();
            if (fog == null)
                fog = cam.gameObject.AddComponent<VisionFog>();
            fog.player = player.transform;
            fog.innerRadius = 1.6f;
            fog.outerRadius = 3.2f;
            fog.falloffPower = 2.5f;
            fog.fogColor = new Color(0f, 0f, 0f, 1f);
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Selection.activeGameObject = player;

        EditorUtility.DisplayDialog("Готово!",
            "Шаг 1 собран.\n\n1. Сохрани сцену: Ctrl+S\n2. Нажми Play ▶\n3. Ходи WASD\n\nВ Game view (не Scene)!",
            "OK");
    }

    private static void RemoveIfExists(string name)
    {
        var old = GameObject.Find(name);
        if (old != null)
            Object.DestroyImmediate(old);
    }

    private static void FitSpriteHeight(Transform t, SpriteRenderer sr, float height)
    {
        float h = sr.sprite.bounds.size.y;
        if (h <= 0.001f) return;
        float scale = height / h;
        t.localScale = new Vector3(scale, scale, 1f);
    }
}
#endif
