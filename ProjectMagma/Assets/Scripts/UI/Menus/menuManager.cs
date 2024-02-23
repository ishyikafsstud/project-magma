using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class menuManager : MonoBehaviour
{
    public static menuManager instance;

    [Header("---- Load Scene ----")]
    [SerializeField] float transitionDelay;
    [SerializeField] string mainMenu;
    [SerializeField] string levelOne;
    [SerializeField] string levelTwo;
    [SerializeField] string levelThree;
    [SerializeField] string levelFour;
    [SerializeField] string levelFive;
    [SerializeField] string alphaShowcaseLevel;

    [Header("---- Transition ----")]
    [SerializeField] float toggleWindowDelay;
    public float ToggleWindowDelay { get { return toggleWindowDelay; } }
    [SerializeField] float closeWindowDelay;
    public float CloseWindowDelay { get { return closeWindowDelay; } }


    private void Start()
    {
        IsWebGLBuild();
        // Sync all window toggle / close delay time
        buttonFunctions[] buttonFunctionsComponents = FindObjectsOfType<buttonFunctions>(true);
        foreach (var buttonFunctionsComponent in buttonFunctionsComponents)
        {
            buttonFunctionsComponent.ToggleWindowDelay = toggleWindowDelay;
            buttonFunctionsComponent.CloseWindowDelay = closeWindowDelay;
        }
    }

    public void IsWebGLBuild()
    {
        // Toggle off all quit buttons if the build is WebGL
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            GameObject[] quitButtons = GameObject.FindGameObjectsWithTag("QuitButton");
            foreach (GameObject quitButton in quitButtons)
            {
                quitButton.SetActive(false);
            }
        }
    }

    public IEnumerator LoadSceneWithDelay(string sceneName, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        Time.timeScale = 1.0f;
        SceneManager.LoadScene(sceneName);
    }

    public void LoadMainMenu()
    {
        LoadLevel(mainMenu);
    }

    /// <summary>
    /// Load the furthest level the player has unlocked.
    /// </summary>
    public void ContinueGame()
    {
        //Debug.Log("Button Test: Continue Button Presssed.");
        List<gameManager.LevelIdEnum> unlockedLevels = saveSystem.GetUnlockedLevels();

        switch(unlockedLevels.Count)
        {
            case 2:
                LoadLevelTwo();
                break;
            case 3:
                LoadLevelThree();
                break;
            case 4:
                LoadLevelFour();
                break;
            case 5:
                LoadLevelFive();
                break;
            case 1:
            default:
                LoadLevelOne();
                break;
        }
    }

    public void StartNewGame()
    {
        StartCoroutine(LoadSceneWithDelay(levelOne, transitionDelay));
        saveSystem.ResetLevelProgression();
    }

    // Need to implement code so that Level select buttons stay disabled until the player completes that level
    public void LoadLevelOne()
    {
        LoadLevel(levelOne);
    }
    public void LoadLevelTwo()
    {
        LoadLevel(levelTwo);
    }
    public void LoadLevelThree()
    {
        LoadLevel(levelFour);
    }
    public void LoadLevelFour()
    {
        LoadLevel(levelFour);
    }
    public void LoadLevelFive()
    {
        LoadLevel(levelFive);
    }

    public void LoadLevel(string levelName)
    {
        if (levelName != null)
        {
            StartCoroutine(LoadSceneWithDelay(levelName, transitionDelay));
        }
        else
        {
            Debug.LogWarning("Trying to load an unnamed scene.");
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
