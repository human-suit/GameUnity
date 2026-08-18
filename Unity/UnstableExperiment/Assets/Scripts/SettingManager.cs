using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingManager : MonoBehaviour
{

    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Toggle fullscreenToggle;

    private void Start()
    {
        LoadSetting();
        volumeSlider.value = 1f;
        fullscreenToggle.isOn = true;
    }

    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;

        PlayerPrefs.SetFloat("Volume", volume);
        PlayerPrefs.Save();
    }

    public void SetFullscreen(bool fullscreen)
    {
        Screen.fullScreen = fullscreen;

        PlayerPrefs.SetInt("Fullscreen", fullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void LoadSetting()
    {
        float volume = PlayerPrefs.GetFloat("Volume", 1f);
        bool fullScreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;

        AudioListener.volume = volume;
        Screen.fullScreen = fullScreen;

        volumeSlider.value = volume;
        fullscreenToggle.isOn = fullScreen;
    }
}

