using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] TMP_Dropdown resolutionDropdown;
    [SerializeField] Toggle fullscreenToggle;
    [SerializeField] Slider volumeSlider;
    [SerializeField] GameObject settingsPanel;

    private Resolution[] supportedResolutions = {
        new Resolution {width = 640, height = 360},
        new Resolution {width = 854, height = 480},
        new Resolution {width = 960, height = 540},
        new Resolution {width = 1024, height = 576},
        new Resolution {width = 1280, height = 720},
        new Resolution {width = 1366, height = 768},
        new Resolution {width = 1600, height = 900},
        new Resolution {width = 1920, height = 1080},
        new Resolution {width = 2560, height = 1440},
        new Resolution {width = 3200, height = 1800},
        new Resolution {width = 3840, height = 2160},
    };
    Resolution[] nativeResolutions;
    List<Resolution> finalResolutions;

    void Start()
    {
        Time.timeScale = 1f; // Ensure time scale is normal when starting
        settingsPanel.SetActive(false);
        nativeResolutions = Screen.resolutions;
        finalResolutions = new List<Resolution>();
        List<string> options = new List<string>();
        int currentResolutionIndex = 0;

        for (int i = 0; i < nativeResolutions.Length; i++)
        {
            for (int j = 0; j < supportedResolutions.Length; j++)
            {
                if (nativeResolutions[i].width == supportedResolutions[j].width && nativeResolutions[i].height == supportedResolutions[j].height)
                {
                    if (!finalResolutions.Contains(supportedResolutions[j]))
                    {
                        string resolutionString = supportedResolutions[j].width + " x " + supportedResolutions[j].height;
                        finalResolutions.Add(supportedResolutions[j]);
                        options.Add(resolutionString);

                        if (supportedResolutions[j].width == Screen.currentResolution.width && supportedResolutions[j].height == Screen.currentResolution.height)
                        {
                            currentResolutionIndex = options.Count - 1;
                        }
                    }
                }
            }
        }

        resolutionDropdown.ClearOptions();
        resolutionDropdown.AddOptions(options);
        LoadSettings();

        // Add listeners
        resolutionDropdown.onValueChanged.AddListener(SetResolution);
        fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
        volumeSlider.onValueChanged.AddListener(SetVolume);
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = finalResolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);

        // Save resolution setting
        PlayerPrefs.SetInt("ResolutionIndex", resolutionIndex);
        PlayerPrefs.Save();
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
        // Load resolution setting
        if (PlayerPrefs.HasKey("ResolutionIndex"))
        {
            int resolutionIndex = PlayerPrefs.GetInt("ResolutionIndex");
            resolutionDropdown.value = resolutionIndex;
            resolutionDropdown.RefreshShownValue();
            SetResolution(resolutionIndex);
        }
        else
        {
            resolutionDropdown.value = finalResolutions.Count - 1;
            resolutionDropdown.RefreshShownValue();
            SetResolution(finalResolutions.Count - 1);
            PlayerPrefs.SetInt("ResolutionIndex", finalResolutions.Count - 1);
        }

        // Load fullscreen setting
        if (PlayerPrefs.HasKey("Fullscreen"))
        {
            bool isFullscreen = PlayerPrefs.GetInt("Fullscreen") == 1;
            fullscreenToggle.isOn = isFullscreen;
            Screen.fullScreen = isFullscreen;
        }
        else
        {
            //default
            fullscreenToggle.isOn = Screen.fullScreen;
            PlayerPrefs.SetInt("Fullscreen", 1);
        }

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

    public void ReloadScene()
    {
        Debug.Log("Reloading Main Menu");
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
        //AudioManager.instance.PlayMusic("MainMenu");
    }
}
