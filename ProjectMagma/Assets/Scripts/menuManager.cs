using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class menuManager : MonoBehaviour
{
    public static menuManager instance;

    [Header("---- Elements ----")]
    [SerializeField] Slider masterVolumeSlider;
    [SerializeField] Slider sfxVolumeSlider;
    [SerializeField] Slider musicVolumeSlider;
    [SerializeField] Slider uiVolumeSlider;
    [SerializeField] Toggle tiltToggle;


    [Header("---- Load Scene ----")]
    [SerializeField] string mainMenu;
    [SerializeField] string levelOne;
    [SerializeField] string levelTwo;
    [SerializeField] string levelThree;
    [SerializeField] string levelFour;
    [SerializeField] string levelFive;


    private void Start()
    {
        GeneralSettingsData savedSettings = saveSystem.LoadGeneralSettings();
        ApplyGeneralSettings(savedSettings);
    }

    void ApplyGeneralSettings(GeneralSettingsData settingsData)
    {
        if (masterVolumeSlider == null)
            return;

        masterVolumeSlider.value = settingsData.masterVolume;
        sfxVolumeSlider.value = settingsData.sfxVolume;
        musicVolumeSlider.value = settingsData.musicVolume;
        uiVolumeSlider.value = settingsData.uiVolume;
        tiltToggle.isOn = settingsData.tiltEnabled;
    }

    public void MainMenu()
    {
        if (mainMenu != null)
        {
            SceneManager.LoadScene(mainMenu);
            Time.timeScale = 1.0f;
        }
        else
        {
            Debug.LogWarning("Scene is not set in inspector.");
        }
    }
    public void ContinueGame()
    {
        //Add save system code here to resume where player left off
        Debug.Log("Button Test: Continue Button Presssed.");
    }

    // Settings
    public void MasterVolumeChanged()
    {
        Debug.Log("Slider Test: Master Volume Changed.");
        saveSystem.SaveMasterVolume((int)masterVolumeSlider.value);
        // TODO: update it in MagmaAudioMixer
    }
    public void SFXVolumeChanged()
    {
        Debug.Log("Slider Test: SFX Volume Changed.");
        saveSystem.SaveSfxVolume((int)sfxVolumeSlider.value);
        // TODO: update it in MagmaAudioMixer
    }
    public void MusicVolumeChanged()
    {
        Debug.Log("Slider Test: Music Volume Changed.");
        saveSystem.SaveMusicVolume((int)musicVolumeSlider.value);
        // TODO: update it in MagmaAudioMixer
    }
    public void UIVolumeChanged()
    {
        Debug.Log("Slider Test: UI Volume Changed.");
        saveSystem.SaveUiVolume((int)uiVolumeSlider.value);
        // TODO: update it in MagmaAudioMixer
    }
    public void TiltCameraChanged()
    {
        Debug.Log("Toggle Test: Camera Tilt Changed.");
        saveSystem.SaveTilt(tiltToggle.isOn);
    }

    // Need to implement code so that Level select buttons stay disabled until the player completes that level
    public void LoadLevelOne()
    {
        if (levelOne != null)
        {
            SceneManager.LoadScene(levelOne);
            Time.timeScale = 1.0f;
        }
        else
        {
            Debug.LogWarning("Scene is not set in inspector.");
        }
    }
    public void LoadLevelTwo()
    {
        if (levelTwo != null)
        {
            SceneManager.LoadScene(levelTwo);
            Time.timeScale = 1.0f;
        }
        else
        {
            Debug.LogWarning("Scene is not set in inspector.");
        }
    }
    public void LoadLevelThree()
    {
        if (levelThree != null)
        {
            SceneManager.LoadScene(levelThree);
            Time.timeScale = 1.0f;
        }
        else
        {
            Debug.LogWarning("Scene is not set in inspector.");
        }
    }
    public void LoadLevelFour()
    {
        if (levelFour != null)
        {
            SceneManager.LoadScene(levelFour);
            Time.timeScale = 1.0f;
        }
        else
        {
            Debug.LogWarning("Scene is not set in inspector.");
        }
    }
    public void LoadLevelFive()
    {
        if (levelFive != null)
        {
            SceneManager.LoadScene(levelFive);
            Time.timeScale = 1.0f;
        }
        else
        {
            Debug.LogWarning("Scene is not set in inspector.");
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void ToggleWindow(GameObject window)
    {
        if (window != null)
        {
            window.SetActive(!window.activeSelf);
        }
        else
        {
            Debug.LogWarning("Reference not set in inspector.");
        }
    }
    public void CloseWindow(GameObject window)
    {
        if (window != null)
        {
            window.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Reference not set in inspector.");
        }
    }
}
