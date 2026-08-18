using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



   
[Serializable] public class PlayerData
{
    public int maxHealth = 100;
    public int currentHealth = 100;

    public int level = 1;

    public int currentXP = 0;
    public int xpToNextLevel = 100;

    public int money = 100;

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (currentHealth < 0)
            currentHealth = 0;
    }

    public void Heal(int amount)
    {
        currentHealth += amount;

        if (currentHealth > maxHealth)
            currentHealth = maxHealth;
    }

    public void AddMoney(int amount)
    {
        money += amount;
    }

    public void AddXP(int amount)
    {
        currentXP += amount;

        while (currentXP >= xpToNextLevel)
        {
            currentXP -= xpToNextLevel;

            level++;

            xpToNextLevel += 50;
        }
    }
}

