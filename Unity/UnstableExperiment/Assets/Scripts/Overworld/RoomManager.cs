using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Текущая комната: фон, двери, лут, позиция игрока.
/// </summary>
public class RoomManager : MonoBehaviour
{
    public Transform roomBackground;
    public Transform player;
    public Transform doorsRoot;
    public Transform lootRoot;

    [Header("Настройки")]
    public float roomHeight = 16f;
    public float doorTriggerSize = 1.8f;

    [Header("Двери — координаты задаёшь в Inspector")]
    public Vector3 doorNorth;
    public Vector3 doorSouth;
    public Vector3 doorEast;
    public Vector3 doorWest;
    public Vector3 spawnPoint;

    public string StatusMessage { get; private set; }
    public RoomDef CurrentRoom { get; private set; }

    private float _messageUntil;

    private void Start()
    {
        if (doorsRoot == null)
        {
            var doors = new GameObject("Doors");
            doors.transform.SetParent(transform);
            doorsRoot = doors.transform;
        }

        if (lootRoot == null)
        {
            var loot = new GameObject("Loot");
            loot.transform.SetParent(transform);
            lootRoot = loot.transform;
        }

        LoadRoom(GameState.CurrentRoomId, null);
    }

    public void LoadRoom(string roomId, string entryDoorId)
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
        RebuildDoors(room);
        RebuildLoot(room);
        PlacePlayer(entryDoorId);

        ShowMessage(room.nameRu, 2f);
    }

    public void TryUseDoor(DoorTrigger door)
    {
        if (door == null) return;

        if (door.IsLocked)
        {
            ShowMessage(door.lockedHintRu ?? "Заперто", 2.5f);
            return;
        }

        if (string.IsNullOrEmpty(door.targetRoom))
        {
            ShowMessage("Дверь никуда не ведёт", 2f);
            return;
        }

        LoadRoom(door.targetRoom, door.doorId);
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

    private void RebuildDoors(RoomDef room)
    {
        if (doorsRoot == null) return;

        for (int i = doorsRoot.childCount - 1; i >= 0; i--)
            Destroy(doorsRoot.GetChild(i).gameObject);

        if (room.doors == null) return;

        foreach (var def in room.doors)
        {
            var go = new GameObject($"Door_{def.id}");
            go.transform.SetParent(doorsRoot);
            go.transform.position = DoorWorldPosition(def.id);

            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(doorTriggerSize, doorTriggerSize * 0.75f);

            var door = go.AddComponent<DoorTrigger>();
            door.doorId = def.id;
            door.labelRu = def.labelRu;
            door.targetRoom = def.targetRoom;
            door.requiresKey = def.requiresKey;
            door.lockedHintRu = def.lockedHintRu;
        }
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

    private void PlacePlayer(string entryDoorId)
    {
        if (player == null) return;

        if (string.IsNullOrEmpty(entryDoorId))
        {
            player.position = spawnPoint;
            return;
        }

        var doorPos = DoorWorldPosition(entryDoorId);
        var offset = entryDoorId switch
        {
            "north" => Vector3.down,
            "south" => Vector3.up,
            "east" => Vector3.left,
            "west" => Vector3.right,
            _ => Vector3.zero
        };
        player.position = doorPos + offset * 1.4f;
    }

    public Vector3 DoorWorldPosition(string doorId)
    {
        return doorId switch
        {
            "north" => doorNorth,
            "south" => doorSouth,
            "east" => doorEast,
            "west" => doorWest,
            _ => Vector3.zero
        };
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
