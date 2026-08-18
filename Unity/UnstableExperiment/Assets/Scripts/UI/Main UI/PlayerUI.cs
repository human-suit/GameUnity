using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{ 
    [SerializeField] private Slider hpBar;
    [SerializeField] private Slider levelBar;
    [SerializeField] private TMP_Text hpText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text moneyText;

    private void Update()
    {
        if (GameManager.Instance == null)
            return;

            PlayerData player = GameManager.Instance.playerData;
        //HP
        hpBar.maxValue = player.maxHealth;
        hpBar.value = player.currentHealth;

        hpText.text = $"HP: {player.currentHealth} / {player.maxHealth}";

        // LVL
        levelBar.maxValue = player.xpToNextLevel;
        levelBar.value = player.currentXP;

        levelText.text = $"Level: {player.level}";

        // Money
        moneyText.text = $"Money: {player.money}";
    }
}
