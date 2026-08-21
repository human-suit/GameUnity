using System.Collections.Generic;

public static class GameState
{
    public const int PlayerMaxHealth = 30;

    public static string CurrentRoomId = "a_plaza";
    public static readonly HashSet<string> Keys = new();
    public static readonly HashSet<string> CollectedLoot = new();
    public static readonly HashSet<string> DefeatedEnemies = new();

    public static int PlayerHealth { get; private set; } = PlayerMaxHealth;

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

    public static void DamagePlayer(int damage)
    {
        PlayerHealth = System.Math.Max(0, PlayerHealth - System.Math.Max(0, damage));
    }

    public static void RestorePlayerFullHealth()
    {
        PlayerHealth = PlayerMaxHealth;
    }

    public static bool IsEnemyDefeated(string encounterId) =>
        !string.IsNullOrEmpty(encounterId) && DefeatedEnemies.Contains(encounterId);

    public static void MarkEnemyDefeated(string encounterId)
    {
        if (!string.IsNullOrEmpty(encounterId))
            DefeatedEnemies.Add(encounterId);
    }

    public static void ResetRun()
    {
        CurrentRoomId = "a_plaza";
        Keys.Clear();
        CollectedLoot.Clear();
        DefeatedEnemies.Clear();
        RestorePlayerFullHealth();
        BattleEncounterData.Clear();
    }
}
