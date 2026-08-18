using System.Collections.Generic;

public static class GameState
{
    public static string CurrentRoomId = "a_plaza";
    public static readonly HashSet<string> Keys = new();
    public static readonly HashSet<string> CollectedLoot = new();

    public static bool HasKey(string keyId) =>
        !string.IsNullOrEmpty(keyId) && Keys.Contains(keyId);

    public static void AddKey(string keyId)
    {
        if (!string.IsNullOrEmpty(keyId))
            Keys.Add(keyId);
    }

    public static bool IsLootCollected(string lootId) =>
        !string.IsNullOrEmpty(lootId) && CollectedLoot.Contains(lootId);

    public static void MarkLootCollected(string lootId)
    {
        if (!string.IsNullOrEmpty(lootId))
            CollectedLoot.Add(lootId);
    }
}
