using System;

public enum EnemyBodyPartType
{
    Head,
    Torso,
    LeftArm,
    RightArm,
    LeftLeg,
    RightLeg
}

[Serializable]
public sealed class EnemyBodyPart
{
    public EnemyBodyPartType Type { get; }
    public string DisplayName { get; }
    public int MaxHealth { get; }
    public int CurrentHealth { get; private set; }
    public int BaseHitChance { get; }

    public bool IsDestroyed => CurrentHealth <= 0;

    public EnemyBodyPart(
        EnemyBodyPartType type,
        string displayName,
        int maxHealth,
        int baseHitChance)
    {
        Type = type;
        DisplayName = displayName;
        MaxHealth = maxHealth;
        CurrentHealth = maxHealth;
        BaseHitChance = baseHitChance;
    }

    public int ApplyDamage(int damage)
    {
        if (IsDestroyed || damage <= 0)
            return 0;

        int appliedDamage = Math.Min(CurrentHealth, damage);
        CurrentHealth -= appliedDamage;
        return appliedDamage;
    }
}
