using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class BattleManager : MonoBehaviour
{
    [SerializeField] private EnemyStats enemy;
    public enum BattleState
    {
        PlayerTurn,
        EnemyTurn,
        BattleWon,
        BattleLost
    }

    [SerializeField] private TMP_Text energyText;
    [SerializeField] private int maxEnergy = 3;
    private int currentEnergy;

    [SerializeField] private TMP_Text blockText;
    private int currentBlock = 0;
    [SerializeField] private int blockAmount = 5;

    public BattleState currentState;

    private void Start()
    {
        currentState = BattleState.PlayerTurn;
        currentEnergy = maxEnergy;
        UpdateEnergyUI();

        Debug.Log("=== BATTLE STARTED ===");
        Debug.Log("Player Turn");
    }

    public void EndPlayerTurn()
    {
        Debug.Log("End turn botton pressed");

        if (currentState != BattleState.PlayerTurn)
            return;

            Debug.Log("end player turn");

            StartEnemyTurn();
    }

    private void StartEnemyTurn()
    {
        currentState = BattleState.EnemyTurn;

        Debug.Log("Enemy Turn");

        enemy.Attack();

        // Проверяем, умер ли игрок
        if (GameManager.Instance.playerData.currentHealth <= 0)
        {
            BattleLost();
            return;
        }

        StartPlayerTurn();
    }

    private void StartPlayerTurn()
    {
        currentState = BattleState.PlayerTurn;

        currentEnergy = maxEnergy;
        
        currentBlock = 0;
        
        UpdateEnergyUI();
        UpdateBlockUI();

        Debug.Log("Player Turn");
        Debug.Log("Energy: " + currentEnergy + "/" + maxEnergy);
    }

    // ПОБЕДА
    public void BattleWon()
    {
        if (currentState == BattleState.BattleWon ||
            currentState == BattleState.BattleLost)
            return;

        currentState = BattleState.BattleWon;

        Debug.Log("=== BATTLE WON ===");

        // Запоминаем, что враг побеждён
        GameManager.Instance.DefeatEnemy();

        // Возвращаемся на карту
        SceneManager.LoadScene("Main");
    }

    // =========================
    // ПОРАЖЕНИЕ
    // =========================

    public void BattleLost()
    {
        if (currentState == BattleState.BattleWon ||
            currentState == BattleState.BattleLost)
            return;

        currentState = BattleState.BattleLost;

        Debug.Log("=== BATTLE LOST ===");

        // Сбрасываем текущий забег
        GameManager.Instance.StartNewGame();

        // Возвращаемся в главное меню
        SceneManager.LoadScene("MainMenu");
    }

    public void PlayerAttack()
    {
        if (currentState != BattleState.PlayerTurn)
            return;

        if (currentEnergy <= 0)
        {
            Debug.Log("Not enough energy!");
            return;
        }

        int damage = 10;

        currentEnergy--;
        UpdateEnergyUI();

        Debug.Log("Player attacks for " + damage + " damage!");
        Debug.Log("Energy: " + currentEnergy + "/" + maxEnergy);

        enemy.TakeDamage(damage);
    }

    private void UpdateEnergyUI()
    {
        energyText.text = $"Energy: {currentEnergy} / {maxEnergy}";
    }

    public void PlayerBlock()
    {
        if (currentState != BattleState.PlayerTurn)
        return;

    if (currentEnergy <= 0)
    {
        Debug.Log("Not enough energy!");
        return;
    }

    currentEnergy--;

    currentBlock += blockAmount;

    Debug.Log("Player gained " + blockAmount + " Block!");
    Debug.Log("Current Block: " + currentBlock);
    Debug.Log("Energy: " + currentEnergy + "/" + maxEnergy);

    UpdateBlockUI();
    UpdateEnergyUI();
    }

    public void TakePlayerDamage(int damage)
    {
        if (currentBlock > 0)
        {
            int damageAfterBlock = damage - currentBlock;

            if (damageAfterBlock < 0)
                damageAfterBlock = 0;

            Debug.Log("Block absorbed " + Mathf.Min(damage, currentBlock) + " damage!");

            currentBlock -= damage;

            if (currentBlock < 0)
                currentBlock = 0;

            if (damageAfterBlock > 0)
                {
                    GameManager.Instance.playerData.TakeDamage(damageAfterBlock);
                }
        }
        else
        {
            GameManager.Instance.playerData.TakeDamage(damage);
        }

        Debug.Log
        (
        "Player HP: " +
        GameManager.Instance.playerData.currentHealth +
        " / " +
        GameManager.Instance.playerData.maxHealth
        );

        Debug.Log("Player Block: " + currentBlock);

        if (GameManager.Instance.playerData.currentHealth <= 0)
        {
            BattleLost();
        }

        UpdateBlockUI();
    }

    private void UpdateBlockUI()
    {
        blockText.text = $"Block: {currentBlock}";
    }
}
