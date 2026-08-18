using UnityEngine;

/// <summary>
/// Предмет в комнате — подобрать клавишей E.
/// </summary>
[RequireComponent(typeof(CircleCollider2D))]
public class LootPickup : MonoBehaviour
{
    public string lootId;
    public string lootType;

    public string HintText => lootType switch
    {
        "key" => "[E] Подобрать ключ",
        "map_unlock" => "[E] Подобрать карту",
        _ => "[E] Подобрать"
    };

    public void PickUp(RoomManager rooms)
    {
        if (GameState.IsLootCollected(lootId)) return;

        GameState.MarkLootCollected(lootId);

        if (lootType == "key")
            GameState.AddKey(lootId);

        string msg = lootType switch
        {
            "key" => "Получен ключ",
            "map_unlock" => "Получена карта сектора",
            _ => "Подобрано"
        };

        rooms?.ShowMessage(msg, 2.5f);
        Destroy(gameObject);
    }
}
