using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleTrigger : MonoBehaviour
{
     [SerializeField] private string enemyID = "Enemy_001";

    private bool playerEntered = false;

    private void Start()
    {
        // Если этого врага уже победили,
        // убираем его с карты.
        if (GameManager.Instance.IsEnemyDefeated(enemyID))
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

        GameManager.Instance.StartBattle(enemyID);

        SceneManager.LoadScene("Battle");
    }
}
