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
