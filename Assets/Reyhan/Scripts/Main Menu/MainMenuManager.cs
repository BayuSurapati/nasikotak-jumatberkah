using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private SettingsUIManager _settingsUI;
    [SerializeField] private CreditUIManager _creditUI;

    public void ChangeScene(string sceneName)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit Game");
    }

    public void OptionsButton()
    {
        Debug.Log("Tombol Options Diklik!");
        if (_settingsUI != null)
        {
            _settingsUI.OpenSettings();
        }
        else
        {
            Debug.LogError("SettingsUI belum dipasang di Inspector MainMenuManager!");
        }
    }

    public void CreditsButton()
    {
        Debug.Log("Tombol Credit Diklik!");
        if (_creditUI != null)
        {
            _creditUI.OpenCredits();
        }
        else
        {
            Debug.LogError("CreditUI belum dipasang di Inspector MainMenuManager!");
        }
    }
}

