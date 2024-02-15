using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class menuManager : MonoBehaviour
{
    public static menuManager instance;

    [Header("---- Load Scene ----")]
    [SerializeField] string mainMenu;
    [SerializeField] string levelOne;
    [SerializeField] string levelTwo;
    [SerializeField] string levelThree;
    [SerializeField] string levelFour;
    [SerializeField] string levelFive;

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
    public void Continue()
    {
        //Add save system code here to resume where player left off
        Debug.Log("Button Test: Continue Button Presssed.");
    }

    // Settings
    public void MasterVolumeChanged()
    {
        Debug.Log("Slider Test: Master Volume Changed.");
        // Controls Master Volume
    }
    public void SFXVolumeChanged()
    {
        Debug.Log("Slider Test: SFX Volume Changed.");
        // Controls Sound Effect Volume
    }
    public void MusicVolumeChanged()
    {
        Debug.Log("Slider Test: Music Volume Changed.");
        // Controls Music Volume
    }
    public void UIVolumeChanged()
    {
        Debug.Log("Slider Test: UI Volume Changed.");
        // Controls UI Volume. Menus/Button sounds
    }
    public void TiltCameraChanged()
    {
        Debug.Log("Toggle Test: Camera Tilt Changed.");
        // Calls/Toggles Tilt Camera off/on
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
