using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleTrigger : MonoBehaviour
{
    [SerializeField] private string enemyID = "Enemy_001";
    [SerializeField] private string encounterID = "Enemy_001_Main";
    [SerializeField] private BattleEnemyDefinition battleDefinition;
    [SerializeField] private Sprite battleBackground;

    private bool playerEntered = false;

    private void Start()
    {
        // Если этого врага уже победили,
        // убираем его с карты.
        if (GameState.IsEnemyDefeated(encounterID))
        {
            gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter2D (Collider2D other)
    {  
        if (playerEntered)
            return;

        if (!other.CompareTag("Player"))
            return;

        playerEntered = true;

        Debug.Log("Player encountered enemy: " + enemyID);

        BattleEncounterData.Begin(
            enemyID,
            encounterID,
            other.transform.position,
            battleDefinition,
            battleBackground);

        SceneManager.LoadScene("Battle");
    }
}
