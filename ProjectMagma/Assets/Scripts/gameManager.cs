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

    public enum GameStates
    {
        Calm,
        Combat,
        Ambush,
        AmbushDefeated
    }

    public static gameManager instance;

    [Header("---- Level Data ----")]
    [SerializeField] LevelIdEnum levelId = LevelIdEnum.Other;
    public LevelIdEnum LevelId { get => levelId; }
    [Tooltip("Keep empty if you do not wish to override the next level.\n\n" +
        "If you wish the next level to be different from what is automatically chosen, enter the name " +
        "of the target level.")]
    [SerializeField] string nextLevelOverride;

    public string GetNextLevelName()
    {
        return nextLevelOverride.Length == 0 ? helper.GetNextLevelName(levelId) : nextLevelOverride;
    }


    [Header("---- UI ----")]
    [SerializeField] GameObject menuActive;
    [SerializeField] GameObject menuPause;
    [SerializeField] GameObject menuLevelComplete;
    [SerializeField] GameObject menuWin;
    [SerializeField] GameObject menuLose;

    [SerializeField] TextMeshProUGUI itemPromptTitle;
    [SerializeField] TextMeshProUGUI itemPromptDescription;
    [SerializeField] TextMeshProUGUI itemPromptDirection;


    [SerializeField] List<string> pauseMenuTips = new List<string>();
    [Tooltip("Select to randomize the list of Tips")]
    public bool randomTips;
    [SerializeField] TextMeshProUGUI tipsText;
    private bool tipShown;
    private int currentTipIndex = 0;

    [SerializeField] TextMeshProUGUI hintText;
    [SerializeField] float hintDuration;

    [SerializeField] TextMeshProUGUI enemyCountText;

    public Image playerHealthbar;
    public TextMeshProUGUI playerHealthbarText;
    public Image playerEnergybar;
    public TextMeshProUGUI playerEnergybarText;
    public Image playerEnergybarBG;
    public GameObject playerDamageScreenFlash;

    [Header("---- Prefabs ----")]
    [SerializeField] GameObject keyPrefab;
    [SerializeField] GameObject ambushRewardPrefab;

    [Header("---- Automatic set ----")]
    GameObject playerSpawnPosition;
    public GameObject player;
    public playerController playerScript;
    helperClass helper;
    SoundtrackManager soundtrackManager;
    public helperClass Helper { get { return helper; } }

    [Header("---- Functional settings ----")]
    public bool isPaused;
    [SerializeField] GameStates defaultGameState;
    private GameStates curGameState;


    [Header("---- Mouse and Keyboard Menu Controls ----")]
    [Tooltip("Eventsystem will highlight this button first on the Continue Menu.")]
    [SerializeField] private GameObject highlightContinueButton;
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

        playerScript.HealthChanged += PlayerScript_HealthChanged;
        playerScript.EnergyChanged += PlayerScript_EnergyChanged;
        playerScript.WeaponSwitched += PlayerScript_WeaponSwitched;

        soundtrackManager = GameObject.FindGameObjectWithTag("SoundtrackManager").GetComponent<SoundtrackManager>();

        playerScript.SpawnedEvent += OnPlayerSpawned;

        LoadGeneralSettings();

        if (levelId != LevelIdEnum.Other)
            LoadLevelStartData();
    }

    private void PlayerScript_WeaponSwitched(weaponStats weapon)
    {
        playerEnergybar.gameObject.SetActive(weapon != null);
        playerEnergybarBG.gameObject.SetActive(weapon != null);
    }

    void LoadGeneralSettings()
    {
        GeneralSettingsData generalSettingsData = saveSystem.LoadGeneralSettings();

        playerScript.EnableTilt(generalSettingsData.tiltEnabled);
        Camera.main.GetComponent<cameraController>()?.SetMouseSensitivity(generalSettingsData.mouseSensitivity);
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

    IEnumerator Start()
    {
        UpdateEnemyCountText();
        ShowHint("Good Luck!");

        EnterGameState(defaultGameState);

        if (playerSpawnPosition != null)
        {
            player.transform.position = playerSpawnPosition.transform.position;
            player.transform.rotation = playerSpawnPosition.transform.rotation;
        }

        yield return new WaitForFixedUpdate();
        LateStart();
    }

    /// <summary>
    /// If for any reason something that must be in Start() is not ready at the time of Start(),
    /// use this method.
    /// </summary>
    void LateStart()
    {
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
        if (Input.GetButtonDown("Cancel") || Input.GetButtonDown("Pause") && menuActive == null)
        {
            // call pause function
            statePaused();
            // set pause menu as the active menu
            menuActive = menuPause;
            // toggle the menu on
            menuActive.SetActive(isPaused);
        }
    }

    public void EnterGameState(GameStates newState)
    {
        curGameState = newState;
        //Debug.Log("Entered " + newState + " state.");

        soundtrackManager.GetComponent<SoundtrackManager>().PlayGameStateMusic(curGameState);
    }

    public void statePaused()
    {
        isPaused = true;

        if (!randomTips)
            DisplayTipInOrder();
        else
            DisplayRandomTip();

        // Stop all time based Actions from happening in the background
        Time.timeScale = 0.0f;
        // Make Cursor Visible
        Cursor.visible = true;
        // Confine Cursor to Pause window boundaries
        Cursor.lockState = CursorLockMode.Confined;

        // TODO: should it really be here?
        //stop all coroutines
        StopAllCoroutines();

        soundtrackManager.PauseMusic();

        // Event System Highlights/Selects button to enable keyboard controls on menu
        EventSystem.current.SetSelectedGameObject(highlightPauseButton);
    }
    public void stateUnpaused()
    {
        isPaused = false;

        tipShown = false;
        // Resumes time based actions 
        Time.timeScale = 1.0f;
        // Hides Cursor
        Cursor.visible = false;
        // Locks Cursor
        Cursor.lockState = CursorLockMode.Locked;

        soundtrackManager.PauseMusic(false);

        // Toggle Menu Off
        menuActive.SetActive(false);
        // resets and removes pause menu from active Menu
        menuActive = null;
    }

    public void scenarioPlayerWins()
    {
        statePaused();

        menuActive = menuWin;
        // If the next level is overriden OR it is a normal not-final level, show the Level Complete menu instead
        if (nextLevelOverride.Length > 0
            || (levelId != LevelIdEnum.Other && levelId != LevelIdEnum.LAST_LEVEL))
            menuActive = menuLevelComplete;

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
        {
            EnterGameState(GameStates.Ambush);
            OnKeyPicked();
        }

        //ShowHint("Key Collected\nEscape");

        if (enemyManager.instance.ambushSpawner != null)
        {
            EnterGameState(GameStates.Ambush);
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
        EnterGameState(GameStates.AmbushDefeated);
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


    #region UI functionality
    void PlayerScript_HealthChanged(float value, float maxValue)
    {
        playerHealthbar.fillAmount = value / maxValue;
        playerHealthbarText.text = $"{((int)value).ToString()} / {maxValue.ToString()}";
    }

    void PlayerScript_EnergyChanged(float value, float maxValue)
    {
        playerEnergybar.fillAmount = value / maxValue;
        playerEnergybarText.text = $"{((int)value).ToString()}\n\n / \n\n{maxValue.ToString()}";
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
    public void ShowItemPrompt(string title, string description)
    {
        itemPromptTitle.text = title;
        itemPromptTitle.gameObject.SetActive(true);

        itemPromptDescription.text = description;
        itemPromptDescription.gameObject.SetActive(true);

        itemPromptDirection.text = "Press E to Pickup";
        itemPromptDirection.gameObject.SetActive(true);
    }
    public void StopShowingItemPrompt()
    {
        itemPromptTitle.gameObject.SetActive(false);
        itemPromptDescription.gameObject.SetActive(false);
        itemPromptDirection.gameObject.SetActive(false);
    }

    // ----- Display Tips in a Random Order -----
    public void DisplayRandomTip()
    {
        if (pauseMenuTips.Count > 0 && tipsText != null && !tipShown)
        {
            string randomTip = pauseMenuTips[Random.Range(0, pauseMenuTips.Count)];
            tipsText.text = randomTip;

            tipShown = true;
        }
    }
    // ----- Display Tips in Order -----
    public void DisplayTipInOrder()
    {
        if (pauseMenuTips.Count > 0 && tipsText != null && !tipShown)
        {
            tipsText.text = pauseMenuTips[currentTipIndex];
            currentTipIndex = (currentTipIndex + 1) % pauseMenuTips.Count;

            tipShown = true;
        }
    }
    #endregion
}
