using System;

[Serializable]
public class PlayerData
{
    public int maxHealth => GameState.PlayerMaxHealth;
    public int currentHealth
    {
        get => GameState.PlayerHealth;
        set => GameState.SetPlayerHealth(value);
    }

    public int level
    {
        get => GameState.PlayerLevel;
        set => GameState.SetPlayerLevel(value);
    }

    public int currentXP
    {
        get => GameState.PlayerXP;
        set => GameState.SetPlayerXP(value);
    }

    public int xpToNextLevel
    {
        get => GameState.XpToNextLevel;
        set => GameState.SetXpToNextLevel(value);
    }

    public int money
    {
        get => GameState.PlayerMoney;
        set => GameState.SetPlayerMoney(value);
    }

    public void TakeDamage(int damage)
    {
        GameState.DamagePlayer(damage);
    }

    public void Heal(int amount)
    {
        GameState.HealPlayer(amount);
    }

    public void AddMoney(int amount)
    {
        GameState.AddMoney(amount);
    }

    public void AddXP(int amount)
    {
        GameState.AddXP(amount);
    }
}
