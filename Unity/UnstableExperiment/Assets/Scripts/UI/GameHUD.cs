using UnityEngine;

/// <summary>
/// Простой HUD — комната, подсказки, управление.
/// </summary>
public class GameHUD : MonoBehaviour
{
    private RoomManager _rooms;
    private PlayerInteract _player;

    private void Awake()
    {
        _rooms = FindObjectOfType<RoomManager>();
        var player = GameObject.Find("Player");
        if (player != null)
            _player = player.GetComponent<PlayerInteract>();
    }

    private void OnGUI()
    {
        if (_rooms == null) return;

        var room = _rooms.CurrentRoom;
        string roomName = room != null ? room.nameRu : "?";

        int boxHeight = GameState.Keys.Count > 0 ? 154 : 132;
        GUI.Box(new Rect(10, 10, 320, boxHeight), "");
        GUI.Label(new Rect(20, 18, 300, 22), $"Комната: {roomName}");
        GUI.Label(
            new Rect(20, 40, 300, 22),
            $"HP: {GameState.PlayerHealth}/{GameState.PlayerMaxHealth}   " +
            $"LVL: {GameState.PlayerLevel}   " +
            $"Золото: {GameState.PlayerMoney}");
        GUI.Label(new Rect(20, 62, 300, 22), "WASD — ходьба · E — предмет");

        string hint = _player != null ? _player.GetNearbyHint() : null;
        if (!string.IsNullOrEmpty(hint))
            GUI.Label(new Rect(20, 84, 300, 22), hint);

        string status = _rooms.GetActiveHint();
        if (!string.IsNullOrEmpty(status))
            GUI.Label(new Rect(20, 106, 300, 22), status);

        if (GameState.Keys.Count > 0)
            GUI.Label(new Rect(20, 128, 300, 22), $"Ключи: {string.Join(", ", GameState.Keys)}");
    }
}
