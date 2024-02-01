using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class gameManager : MonoBehaviour
{
    public static gameManager instance;

    [Header("UI")]
    [SerializeField] GameObject menuActive;
    [SerializeField] GameObject menuPause;
    [SerializeField] GameObject menuWin;
    [SerializeField] GameObject menuLose;

    [SerializeField] TextMeshProUGUI hintText;
    [SerializeField] float hintDuration;
    [SerializeField] TextMeshProUGUI enemyCountText;
    public Image playerHealthbar;
    public Image playerEnergybar;
    public GameObject playerDamageScreenFlash;


    [Header("Non-children")]
    public GameObject playerSpawnPosition;
    public GameObject player;
    public playerController playerScript;
    GameObject barrier; // TODO: delete this. Obsolete because we are using an event system to unlock barrier on key pickup

    [Header("Functional settings")]
    public bool isPaused;

    public bool IsKeyPicked { get; private set; }
    public delegate void KeyPickedAction();
    public static event KeyPickedAction OnKeyPicked;


    void Awake()
    {
        instance = this;

        player = GameObject.FindGameObjectWithTag("Player");
        playerScript = player.GetComponent<playerController>();
        playerSpawnPosition = GameObject.FindGameObjectWithTag("Player Spawn Position");
        barrier = GameObject.FindGameObjectWithTag("LevelBarrier");
    }

    void Start()
    {
        UpdateEnemyCountText();
        ShowHint("Objective:\n- Grab Your Staff -\n- Kill All Enemies -\n- Find The Key -\n- Escape -");
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
            // toggle the menu on
            menuActive.SetActive(isPaused);
        }
    }
    public void statePaused()
    {
        isPaused = true;
        // Stop all time based Actions from happening in the background
        Time.timeScale = 0.0f;
        // Make Cursor Visible
        Cursor.visible = true;
        // Confine Cursor to Pause window boundaries
        Cursor.lockState = CursorLockMode.Confined;
        //stop all coroutines
        StopAllCoroutines();

    }
    public void stateUnpaused()
    {
        isPaused = false;
        // Resumes time based actions 
        Time.timeScale = 1.0f;
        // Hides Cursor
        Cursor.visible = false;
        // Locks Cursor
        Cursor.lockState = CursorLockMode.Locked;
        // Toggle Menu Off
        menuActive.SetActive(false);
        // resets and removes pause menu from active Menu
        menuActive = null;
    }

    public void scenarioPlayerWins()
    {
        statePaused();
        menuActive = menuWin;
        menuActive.SetActive(true);
    }

    public void scenarioPlayerLoses()
    {
        statePaused();
        menuActive = menuLose;
        menuActive.SetActive(true);
    }

    public void keyPicked()
    {
        IsKeyPicked = true;

        if (OnKeyPicked != null)
            OnKeyPicked();

        //ShowHint("Key Collected\nEscape");

        if (enemyManager.instance.ambushSpawner != null)
        {
            enemyManager.instance.ambushSpawner.gameObject.SetActive(true);
            enemyManager.instance.ambushSpawner.StartAmbush();
        }
    }

    public void UpdateEnemyCountText()
    {
        enemyCountText.SetText("Enemies Left: " + enemyManager.instance.EnemyCount.ToString());
    }

    /// <summary>
    /// Shows a specified hint for a specified time.
    /// Make sure to call this method using StartCoroutine(), as it won't be executed otherwise.
    /// </summary>
    /// <param name="message">The hint to show.</param>
    /// <param name="duration">The duration of the message.</param>
    /// <returns></returns>
    public IEnumerator ShowHint(string message, float duration)
    {
        hintText.text = message;
        hintText.gameObject.SetActive(true);
        yield return new WaitForSeconds(duration);
        HideHint();
    }

    /// <summary>
    /// Shows a specified hint for a default hint time.
    /// </summary>
    /// <param name="message">The hint to show.</param>
    public void ShowHint(string message)
    {
        StartCoroutine(ShowHint(message, hintDuration));
    }

    public void HideHint()
    {
        hintText.gameObject.SetActive(false);
    }
}
