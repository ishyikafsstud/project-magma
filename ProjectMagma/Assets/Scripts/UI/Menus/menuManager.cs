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

    [Header("---- Message ----")]
    [SerializeField] GameObject SaveMessagePrefab;

    private void Start()
    {
        // Sync all window toggle / close delay time
        buttonFunctions[] buttonFunctionsComponents = FindObjectsOfType<buttonFunctions>(true);
        foreach (var buttonFunctionsComponent in buttonFunctionsComponents)
        {
            buttonFunctionsComponent.ToggleWindowDelay = toggleWindowDelay;
            buttonFunctionsComponent.CloseWindowDelay = closeWindowDelay;
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

    public void ContinueGame()
    {
        //Add save system code here to resume where player left off
        //Debug.Log("Button Test: Continue Button Presssed.");
    }

    public void StartNewGame()
    {
        StartCoroutine(LoadSceneWithDelay(alphaShowcaseLevel, transitionDelay));
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
