using System.Collections.Generic;

public sealed class EnemyBattleStats
{
    public string DisplayName { get; }
    public int MaxHealth { get; }
    public int MinDamage { get; }
    public int MaxDamage { get; }

    public EnemyBattleStats(
        string displayName,
        int maxHealth,
        int minDamage,
        int maxDamage)
    {
        DisplayName = displayName;
        MaxHealth = maxHealth;
        MinDamage = minDamage;
        MaxDamage = maxDamage;
    }
}

/// <summary>
/// Базовые характеристики врагов по enemyId.
/// </summary>
public static class EnemyBattleDatabase
{
    private static readonly Dictionary<string, EnemyBattleStats> Enemies =
        new Dictionary<string, EnemyBattleStats>
        {
            {
                "test_enemy",
                new EnemyBattleStats(
                    "Бродяга",
                    maxHealth: 24,
                    minDamage: 4,
                    maxDamage: 7)
            }
        };

    private static readonly EnemyBattleStats Fallback =
        new EnemyBattleStats(
            "Неизвестное существо",
            maxHealth: 20,
            minDamage: 3,
            maxDamage: 6);

    public static EnemyBattleStats Get(string enemyId)
    {
        if (!string.IsNullOrEmpty(enemyId) &&
            Enemies.TryGetValue(enemyId, out EnemyBattleStats stats))
        {
            return stats;
        }

        return Fallback;
    }
}
