using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Текущая комната: фон, лут, позиция игрока.
/// </summary>
public class RoomManager : MonoBehaviour
{
    public Transform roomBackground;
    public Transform player;
    public Transform lootRoot;

    [Header("Настройки")]
    public float roomHeight = 16f;

    [Header("Старт игрока")]
    public Vector3 spawnPoint;

    public string StatusMessage { get; private set; }
    public RoomDef CurrentRoom { get; private set; }

    private float _messageUntil;

    private void Start()
    {
        if (lootRoot == null)
        {
            var loot = new GameObject("Loot");
            loot.transform.SetParent(transform);
            lootRoot = loot.transform;
        }

        LoadRoom(GameState.CurrentRoomId);
    }

    public void LoadRoom(string roomId)
    {
        var room = RoomDatabase.GetRoom(roomId);
        if (room == null)
        {
            ShowMessage($"Комната не найдена: {roomId}", 3f);
            return;
        }

        CurrentRoom = room;
        GameState.CurrentRoomId = roomId;

        ApplyBackground(room);
        RebuildLoot(room);
        PlacePlayer();

        ShowMessage(room.nameRu, 2f);
    }

    public void ShowMessage(string text, float seconds = 2.5f)
    {
        StatusMessage = text;
        _messageUntil = Time.time + seconds;
    }

    public string GetActiveHint()
    {
        if (Time.time > _messageUntil)
            StatusMessage = null;
        return StatusMessage;
    }

    private void ApplyBackground(RoomDef room)
    {
        if (roomBackground == null) return;

        // Tilemap-сцена: фон уже нарисован на Grid
        if (roomBackground.GetComponentInChildren<Tilemap>() != null)
            return;

        var sr = roomBackground.GetComponent<SpriteRenderer>();
        if (sr == null) sr = roomBackground.gameObject.AddComponent<SpriteRenderer>();

        var sprite = Resources.Load<Sprite>($"Art/Rooms/{room.id}_room");
        sr.sprite = sprite != null ? sprite : RoomSprites.Placeholder(room.type);
        sr.sortingOrder = 0;
        FitSpriteHeight(roomBackground, sr, roomHeight);
    }

    private void RebuildLoot(RoomDef room)
    {
        if (lootRoot == null) return;

        for (int i = lootRoot.childCount - 1; i >= 0; i--)
            Destroy(lootRoot.GetChild(i).gameObject);

        if (room.loot == null) return;

        int index = 0;
        foreach (var def in room.loot)
        {
            if (GameState.IsLootCollected(def.id)) continue;

            var go = new GameObject($"Loot_{def.id}");
            go.transform.SetParent(lootRoot);
            go.transform.position = LootWorldPosition(index);

            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.6f;

            var loot = go.AddComponent<LootPickup>();
            loot.lootId = def.id;
            loot.lootType = def.type;

            index++;
        }
    }

    private void PlacePlayer()
    {
        if (player == null) return;
        player.position = spawnPoint;
    }

    private static Vector3 LootWorldPosition(int index)
    {
        return index switch
        {
            0 => new Vector3(0f, 0.5f, 0f),
            1 => new Vector3(1.5f, 0f, 0f),
            _ => new Vector3(-1.5f, 0f, 0f)
        };
    }

    private static void FitSpriteHeight(Transform t, SpriteRenderer sr, float height)
    {
        float h = sr.sprite.bounds.size.y;
        if (h <= 0.001f) return;
        float scale = height / h;
        t.localScale = new Vector3(scale, scale, 1f);
    }
}
