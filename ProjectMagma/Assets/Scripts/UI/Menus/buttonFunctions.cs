using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class buttonFunctions : MonoBehaviour
{
    float toggleWindowDelay = 0.1f;
    public float ToggleWindowDelay { get => toggleWindowDelay; set => toggleWindowDelay = value; }
    float closeWindowDelay = 0.1f;
    public float CloseWindowDelay { get => closeWindowDelay; set => closeWindowDelay = value; }

    public void Resume()
    {
        // Call Unpause within game manager
        gameManager.instance.stateUnpaused();
    }

    public void Restart() // Will need to find another more efficient way to implement this
    {
        // Loads entire scene the user is currently in
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        // Reset Time scale
        gameManager.instance.stateUnpaused();
    }

    public void Quit()
    {
        // Close application
        Application.Quit();
    }

    public void ContinueToNextLevel()
    {
        // Loads entire scene the user is currently in
        SceneManager.LoadScene(gameManager.instance.GetNextLevelName());
        // Reset Time scale
        gameManager.instance.stateUnpaused();
    }

    public void ToggleWindow(GameObject window)
    {
        StartCoroutine(ToggleWindowWithDelay(window, toggleWindowDelay));
    }

    public void CloseWindow(GameObject window)
    {
        StartCoroutine(CloseWindowWithDelay(window, closeWindowDelay));
    }

    IEnumerator ToggleWindowWithDelay(GameObject window, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        if (window != null)
        {
            window.SetActive(!window.activeSelf);
        }
        else
        {
            Debug.LogWarning("Reference not set in inspector.");
        }
    }

    IEnumerator CloseWindowWithDelay(GameObject window, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
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
