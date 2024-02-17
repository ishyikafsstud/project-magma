using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class buttonFunctions : MonoBehaviour
{
  
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

    public void Continue()
    {
        // Loads entire scene the user is currently in
        SceneManager.LoadScene(gameManager.instance.GetNextLevelName());
        // Reset Time scale
        gameManager.instance.stateUnpaused();
    }

    //public void Respawn()
    //{
    //    gameManager.instance.playerScript.respawn();
    //    gameManager.instance.stateUnpaused();
    //}
}
