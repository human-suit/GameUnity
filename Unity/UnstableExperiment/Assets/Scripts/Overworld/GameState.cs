using System.Collections.Generic;

public static class GameState
{
    public const int PlayerMaxHealth = 30;

    public static string CurrentRoomId = "a_plaza";
    public static readonly HashSet<string> Keys = new();
    public static readonly HashSet<string> CollectedLoot = new();
    public static readonly HashSet<string> DefeatedEnemies = new();
    public static readonly List<BattleCardData> CardDeck = new();

    public static int PlayerHealth { get; private set; } = PlayerMaxHealth;
    public static int PlayerLevel { get; private set; } = 1;
    public static int PlayerXP { get; private set; }
    public static int XpToNextLevel { get; private set; } = 100;
    public static int PlayerMoney { get; private set; }

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
        SetPlayerHealth(PlayerHealth - System.Math.Max(0, damage));
    }

    public static void HealPlayer(int amount)
    {
        SetPlayerHealth(PlayerHealth + System.Math.Max(0, amount));
    }

    public static void SetPlayerHealth(int health)
    {
        PlayerHealth = System.Math.Clamp(health, 0, PlayerMaxHealth);
    }

    public static void RestorePlayerFullHealth()
    {
        PlayerHealth = PlayerMaxHealth;
    }

    public static void SetPlayerLevel(int level)
    {
        PlayerLevel = System.Math.Max(1, level);
    }

    public static void SetPlayerMoney(int money)
    {
        PlayerMoney = System.Math.Max(0, money);
    }

    public static void SetPlayerXP(int xp)
    {
        PlayerXP = System.Math.Max(0, xp);
    }

    public static void SetXpToNextLevel(int value)
    {
        XpToNextLevel = System.Math.Max(1, value);
    }

    public static void AddMoney(int amount)
    {
        SetPlayerMoney(PlayerMoney + amount);
    }

    public static void AddXP(int amount)
    {
        if (amount <= 0)
            return;

        PlayerXP += amount;
        while (PlayerXP >= XpToNextLevel)
        {
            PlayerXP -= XpToNextLevel;
            PlayerLevel++;
            XpToNextLevel += 50;
        }
    }

    public static bool IsEnemyDefeated(string encounterId) =>
        !string.IsNullOrEmpty(encounterId) && DefeatedEnemies.Contains(encounterId);

    public static void MarkEnemyDefeated(string encounterId)
    {
        if (!string.IsNullOrEmpty(encounterId))
            DefeatedEnemies.Add(encounterId);
    }

    public static void InitializeCardDeck(IEnumerable<BattleCardData> cards)
    {
        if (CardDeck.Count > 0 || cards == null)
            return;

        foreach (BattleCardData card in cards)
        {
            if (card != null)
                CardDeck.Add(card);
        }
    }

    public static void AddCard(BattleCardData card)
    {
        if (card != null)
            CardDeck.Add(card);
    }

    public static void ResetRun()
    {
        CurrentRoomId = "a_plaza";
        Keys.Clear();
        CollectedLoot.Clear();
        DefeatedEnemies.Clear();
        CardDeck.Clear();
        RestorePlayerFullHealth();
        PlayerLevel = 1;
        PlayerXP = 0;
        XpToNextLevel = 100;
        PlayerMoney = 0;
        BattleEncounterData.Clear();
    }
}
