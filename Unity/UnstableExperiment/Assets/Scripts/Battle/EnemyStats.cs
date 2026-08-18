using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    public int maxHealth = 30;
    public int currentHealth = 30;

    public int attackDamage = 10;

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (currentHealth < 0)
            currentHealth = 0;

        Debug.Log("Enemy HP: " + currentHealth + " / " + maxHealth);

        if (currentHealth <= 0)
        {
            BattleManager battleManager = FindFirstObjectByType<BattleManager>();

            battleManager.BattleWon();
        }
    }

    public void Attack()
    {
        Debug.Log("Enemy attacks for " + attackDamage + " damage!");

        BattleManager battleManager = FindFirstObjectByType<BattleManager>();

        battleManager.TakePlayerDamage(attackDamage);
    }
}
