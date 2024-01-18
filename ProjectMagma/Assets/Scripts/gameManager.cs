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
    [SerializeField] GameObject barrier;

    [Header("Functional settings")]
    public bool isPaused;

    private int enemyCount;
    bool isKeyPicked;

    public bool IsKeyPicked { get; private set; }

    public int EnemyCount
    {
        get => enemyCount;
        set
        {
            if (enemyCount == value) return;

            enemyCount = value;
            UpdateEnemyCountText();
        }
    }

    void Awake()
    {
        instance = this;

        player = GameObject.FindGameObjectWithTag("Player");
        playerScript = player.GetComponent<playerController>();
        playerSpawnPosition = GameObject.FindGameObjectWithTag("Player Spawn Position");
        barrier = GameObject.FindGameObjectWithTag("LevelBarrier");

        enemyCount = 0;
    }

    void Start()
    {
        UpdateEnemyCountText();
        ShowHint("Objective:\n- Kill All Enemies\n- Find Key Card\n- Escape");
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
        isKeyPicked = true;

        if (barrier != null)
            barrier.GetComponent<levelBarrier>().Unlock();

        ShowHint("Key Card Picked Up\nEscape");
    }

    public void DecreaseEnemyCount()
    {
        enemyCount--;
        UpdateEnemyCountText();
    }

    public void UpdateEnemyCountText()
    {
        enemyCountText.SetText("Enemies Left: " + EnemyCount.ToString());
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
