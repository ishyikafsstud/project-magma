using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class gameManager : MonoBehaviour
{
    public enum LevelIdEnum
    {
        Other = 0,
        Level1 = 1,
        Level2,
        Level3,
        Level4,
        Level5,
        LAST_LEVEL = Level5
    }

    public static gameManager instance;

    [Header("---- Level Data ----")]
    [SerializeField] LevelIdEnum levelId = LevelIdEnum.Other;
    public LevelIdEnum LevelId { get => levelId; }

    [Header("---- UI ----")]
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

    [Header("---- Prefabs ----")]
    [SerializeField] GameObject keyPrefab;
    [SerializeField] GameObject ambushRewardPrefab;

    [Header("---- Automatic set ----")]
    public GameObject playerSpawnPosition;
    public GameObject player;
    public playerController playerScript;
    helperClass helper;
    public helperClass Helper { get { return helper; } }

    [Header("Functional settings")]
    public bool isPaused;

    [Header("Mouse and Keyboard Menu Controls")]
    [Tooltip("Eventsystem will highlight this button first on the Win Menu.")]
    [SerializeField] private GameObject highlightWinButton;
    [Tooltip("Eventsystem will highlight this button first on the Lose Menu.")]
    [SerializeField] private GameObject highlightLoseButton;
    [Tooltip("Eventsystem will highlight this button first on the Pause Menu.")]
    [SerializeField] private GameObject highlightPauseButton;

    public bool IsKeyDropped { get; private set; }
    public bool IsKeyPicked { get; private set; }
    public bool IsAmbushRewardDropped { get; private set; }
    public bool IsAmbushRewardPicked { get; private set; }

    public delegate void ItemPicked();
    public static event ItemPicked OnKeyPicked;
    public static event ItemPicked AmbushRewardPickedEvent;


    void Awake()
    {
        instance = this;
        helper = GetComponent<helperClass>();

        player = GameObject.FindGameObjectWithTag("Player");
        playerScript = player.GetComponent<playerController>();
        playerSpawnPosition = GameObject.FindGameObjectWithTag("Player Spawn Position");

        playerScript.PlayerSpawnedEvent += OnPlayerSpawned;

        LoadGeneralSettings();

        if (levelId != LevelIdEnum.Other)
            LoadLevelStartData();
    }

    void LoadGeneralSettings()
    {
        GeneralSettingsData generalSettingsData = saveSystem.LoadGeneralSettings();

        // TODO: implement camera tilt update on player
        //playerScript.SetTilt(generalSettingsData.tiltEnabled);

        // TODO: implement bus volume change
    }

    void LoadLevelStartData()
    {
        saveSystem.ResetLevelProgression();
        LevelSaveData levelData = saveSystem.LoadLevelData(levelId);

        weaponStats firstWeapon = levelData.startWeapons[0] != -1 ? helper.weaponList[(levelData.startWeapons[0])] : null;
        weaponStats secondWeapon = levelData.startWeapons[1] != -1 ? helper.weaponList[(levelData.startWeapons[1])] : null;

        playerScript.pickupWeapon(firstWeapon);
        playerScript.pickupWeapon(secondWeapon);
    }

    void Start()
    {
        UpdateEnemyCountText();
        ShowHint("Good Luck!");
    }

    void OnPlayerSpawned()
    {
        int ambushesDefeated = saveSystem.CountAmbushesDefeated(levelId);
        playerScript.ApplyAmbushDefeatPowerup(ambushesDefeated);
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
        // Event System Highlights/Selects button to enable keyboard controls on menu
        EventSystem.current.SetSelectedGameObject(highlightPauseButton);
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
        // Event System Highlights/Selects button to enable keyboard controls on menu
        EventSystem.current.SetSelectedGameObject(highlightWinButton);

        // Save data if playing on a regular level
        if (levelId != LevelIdEnum.Other)
        {
            saveSystem.SaveLevelData(levelId, this);
        }
    }

    public void scenarioPlayerLoses()
    {
        statePaused();
        menuActive = menuLose;
        menuActive.SetActive(true);
        // Event System Highlights/Selects button to enable keyboard controls on menu
        EventSystem.current.SetSelectedGameObject(highlightLoseButton);
    }

    /// <summary>
    /// Spawn the activator stone and inform the player about it.
    /// </summary>
    /// <param name="pos">Stone spawn position.</param>
    public void SpawnKey(Vector3 pos)
    {
        IsKeyDropped = true;

        if (keyPrefab != null)
            Instantiate(keyPrefab, pos, Quaternion.identity);

        gameManager.instance.ShowHint("Enemy Dropped Activator Stone");
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

    /// <summary>
    /// Spawn the ambush reward and inform the player about it.
    /// </summary>
    /// <param name="pos">Spawn position.</param>
    public void SpawnAmbushReward(Vector3 pos)
    {
        IsAmbushRewardDropped = true;

        if (ambushRewardPrefab != null)
            Instantiate(ambushRewardPrefab, pos, Quaternion.identity);

        gameManager.instance.ShowHint("Enemy Dropped Ambush Reward");
    }

    public void ambushRewardPicked()
    {
        IsAmbushRewardPicked = true;

        playerScript.ApplyAmbushDefeatPowerup(1);

        AmbushRewardPickedEvent?.Invoke();
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
