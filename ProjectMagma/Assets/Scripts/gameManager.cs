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

    public TextMeshProUGUI hintText;
    public float hintDuration;
    public TextMeshProUGUI enemyCountText;
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
    public bool isKeyPicked;

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
        isKeyPicked = false;
        ShowHint("Good Luck!");

        enemyCount = 0;
    }

    void Start()
    {
        UpdateEnemyCountText();
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
        barrier.GetComponent<levelBarrier>().Unlock();
        ShowHint("Key Card Picked Up \nEscape");
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
    IEnumerator ShowHint(string message, float duration)
    {
        hintText.text = message;
        hintText.gameObject.SetActive(true);
        yield return new WaitForSeconds(duration);
        HideHint();
    }

    public void ShowHint(string message)
    {
        StartCoroutine(ShowHint(message, hintDuration));
    }

    public void HideHint()
    {
        hintText.gameObject.SetActive(false);
    }
}
