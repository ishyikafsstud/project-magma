using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class gameManager : MonoBehaviour
{
    //this may not work till the GameManager is in the scene. in class we did it by including it in the UI
    //if you need to test, use the UI prefab with a GameManager in it for your scene
    public static gameManager instance;

    [SerializeField] GameObject menuActive;
    [SerializeField] GameObject menuPause;

    public GameObject player;

    public bool isPaused;

    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
        player = GameObject.FindGameObjectWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        // If ESC button is pressed and nothing is currently in the active menu
        if (Input.GetButtonDown("Cancel") && menuActive == null)
        {
            // call pause function
            statePaused();
            // set pause menu as the active menu
            menuActive = menuPause;
            // toggel the menu on
            menuActive.SetActive(isPaused);
        }
    }
    public void statePaused()
    {
        // Toggel the isPaused Bool
        isPaused = !isPaused;
        // Stop all time based Actions from happening in the background
        Time.timeScale = 0;
        // Make Cursor Visible
        Cursor.visible = true;
        // Confine Cursor to Pause window boundaries
        Cursor.lockState = CursorLockMode.Confined;

    }
    public void stateUnpaused()
    {
        // Toggel the isPaused Bool
        isPaused = !isPaused;
        // Resumes time based actions 
        Time.timeScale = 1;
        // Hides Cursor
        Cursor.visible = false;
        // Locks Cursor
        Cursor.lockState = CursorLockMode.Locked;
        // Toggle Menu Off
        menuActive.SetActive(false);
        // resets and removes pause menu from active Menu
        menuActive = null;
    }
}
