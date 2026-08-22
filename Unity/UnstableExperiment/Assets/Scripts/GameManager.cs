using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public PlayerData playerData;

    public List<string> defeatedEnemies = new List<string>();

    public string currentEnemyID;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        playerData = new PlayerData();

        DontDestroyOnLoad(gameObject);
    }
    public void StartBattle(string enemyID)
    {
        currentEnemyID = enemyID;
    }

    public void DefeatEnemy()
    {
        if (!defeatedEnemies.Contains(currentEnemyID))
        {
            defeatedEnemies.Add(currentEnemyID);
        }
    }

    public bool IsEnemyDefeated(string enemyID)
    {
        return defeatedEnemies.Contains(enemyID);
    }

    public void StartNewGame()
    {
        playerData = new PlayerData();
        GameState.ResetRun();

        defeatedEnemies.Clear();

        currentEnemyID = "";
    }
}
