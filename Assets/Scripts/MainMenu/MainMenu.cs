using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] Slider volumeSlider;
    [SerializeField] GameObject settingsPanel;
    public GameObject chooseDeckPanel;
    public GameObject blackPanel;

    void Start()
    {
        Time.timeScale = 1f; // Ensure time scale is normal when starting
        settingsPanel.SetActive(false);

        LoadSettings();

        volumeSlider.onValueChanged.AddListener(SetVolume);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;

        // Save fullscreen setting
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
        Debug.Log("Setting volume to " + volume);

        // Save volume setting
        PlayerPrefs.SetFloat("Volume", volume);
        PlayerPrefs.Save();
    }

    private void LoadSettings()
    {
        // Load volume setting
        if (PlayerPrefs.HasKey("Volume"))
        {
            float volume = PlayerPrefs.GetFloat("Volume");
            volumeSlider.value = volume;
            AudioListener.volume = volume;
        }
        else
        {
            volumeSlider.value = AudioListener.volume;
            PlayerPrefs.SetFloat("Volume", volumeSlider.value);
        }

        PlayerPrefs.Save();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (settingsPanel.activeSelf)
            {
                Debug.Log("Closing settings panel");
                ClosePanel();
            }
            else
            {
                Debug.Log("Opening settings panel");
                OpenTab();
            }
        }
    }

    public void OpenTab()
    {
        // AudioManager.instance.PlaySFX("MenuOpen");
        Time.timeScale = 0f;
        settingsPanel.SetActive(true);
        //AudioManager.instance.DimAudio(true);
    }

    public void ClosePanel()
    {
        if (GameObject.Find("Menu Popup") != null)
        {
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = 1f;
        }
        settingsPanel.SetActive(false);
        //AudioManager.instance.DimAudio(false);
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game");
        Application.Quit();
    }
}
