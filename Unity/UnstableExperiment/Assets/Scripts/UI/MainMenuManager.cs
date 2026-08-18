using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject settingPanel;
    [SerializeField] private GameObject mainMenuPanel;
    void Start()
    {
        mainMenuPanel.SetActive(true);
        settingPanel.SetActive(false);
    }

    public void Play()
    {
        SceneManager.LoadScene("Main");
    }

    public void Exit()
    {
        Application.Quit();
    }

    public void OpenSetting()
    {
        settingPanel.SetActive(true);
        mainMenuPanel.SetActive(false);
    }

    public void CloseSetting()
    {
       settingPanel.SetActive(false); 
       mainMenuPanel.SetActive(true);
    }
}
