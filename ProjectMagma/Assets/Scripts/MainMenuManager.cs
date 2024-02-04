using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("---- Load Scene ----")]
    [SerializeField] SceneAsset sceneToLoad;

    [Header("---- Windows ----")]
    [SerializeField] GameObject ControlsWindow;
    [SerializeField] GameObject CreditsWindow;


    public void PlayGame()
    {
        if (sceneToLoad != null)
        {
            SceneManager.LoadScene(sceneToLoad.name);
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

    public void QuitGame()
    {
        Application.Quit();
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

    private void ToggleWindow(GameObject window)
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
}
