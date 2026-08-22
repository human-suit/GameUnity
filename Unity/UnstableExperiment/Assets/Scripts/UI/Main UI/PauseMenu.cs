using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private SettingManager settingManager;

    private bool isPaused = false;

    private void Start()
    {
        pausePanel.SetActive(false);

        // Загружаем сохранённую громкость
        float volume = PlayerPrefs.GetFloat("Volume", 1f);

        volumeSlider.minValue = 0f;
        volumeSlider.maxValue = 1f;
        volumeSlider.value = volume;

        // Передаём изменение громкости в SettingManager
        volumeSlider.onValueChanged.AddListener(ChangeVolume);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                Resume();
            else
                Pause();
        }
    }

    public void Pause()
    {
        isPaused = true;
        pausePanel.SetActive(true);

        Time.timeScale = 0f;
    }

    public void Resume()
    {
        isPaused = false;
        pausePanel.SetActive(false);

        Time.timeScale = 1f;
    }

    private void ChangeVolume(float value)
    {
        settingManager.SetVolume(value);
    }

    public void MainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}