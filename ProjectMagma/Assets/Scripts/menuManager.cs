using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class menuManager : MonoBehaviour
{
    public static menuManager instance;

    [Header("- Load Scene -")]
    [SerializeField] SceneAsset levelOne;
    [SerializeField] SceneAsset mainMenu;

    [Header("- Sub Windows -")]
    [SerializeField] GameObject ControlsWindow;
    [SerializeField] GameObject CreditsWindow;

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
    public void PlayGame()
    {
        if (levelOne != null)
        {
            SceneManager.LoadScene(levelOne.name);
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

    public void MainMenu()
    {
        if (mainMenu != null)
        {
            SceneManager.LoadScene(mainMenu.name);
        }
        else
        {
            Debug.LogWarning("Scene is not set in inspector.");
        }
    }

    public void Controls()
    {
        ToggleWindow(ControlsWindow);
    }
    public void Credits()
    {
        ToggleWindow(CreditsWindow);
    }
}
