using UnityEngine;

/// <summary>
/// E — предмет рядом.
/// </summary>
[RequireComponent(typeof(PlayerMove))]
public class PlayerInteract : MonoBehaviour
{
    public float interactRadius = 1.8f;

    private RoomManager _rooms;

    private void Awake()
    {
        _rooms = FindObjectOfType<RoomManager>();
    }

    private void Update()
    {
        if (_rooms == null) return;
        if (ReadPanel.IsOpen || ReadPanel.ClosedThisFrame) return;
        if (!Input.GetKeyDown(KeyCode.E)) return;

        var loot = FindNearestLoot();
        loot?.PickUp(_rooms);
    }

    public LootPickup FindNearestLoot()
    {
        var items = FindObjectsOfType<LootPickup>();
        LootPickup best = null;
        float bestDist = interactRadius;

        foreach (var item in items)
        {
            float d = Vector2.Distance(transform.position, item.transform.position);
            if (d <= bestDist)
            {
                bestDist = d;
                best = item;
            }
        }

        return best;
    }

    public string GetNearbyHint()
    {
        var loot = FindNearestLoot();
        return loot != null ? loot.HintText : null;
    }
}
