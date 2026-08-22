using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatusView : MonoBehaviour
{
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text moneyText;
    [SerializeField] private Slider healthSlider;

    private void OnEnable()
    {
        Refresh();
    }

    private void Update()
    {
        Refresh();
    }

    public void Refresh()
    {
        if (healthText != null)
            healthText.text =
                $"{GameState.PlayerHealth}/{GameState.PlayerMaxHealth}";

        if (levelText != null)
            levelText.text = GameState.PlayerLevel.ToString();

        if (moneyText != null)
            moneyText.text = GameState.PlayerMoney.ToString();

        if (healthSlider != null)
        {
            healthSlider.minValue = 0;
            healthSlider.maxValue = GameState.PlayerMaxHealth;
            healthSlider.value = GameState.PlayerHealth;
        }
    }
}
