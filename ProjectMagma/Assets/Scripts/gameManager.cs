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

    [Header("---- Settings ----")]
    [SerializeField] bool spawnAmbushOnKeyPicked = true;
    [Tooltip("For how long the screen flashes red on player hurt.")]
    [SerializeField] float hurtFlashDuration = 0.1f;

    [Header("---- UI elements ----")]
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

    [SerializeField] Image playerHealthbar;
    [SerializeField] TextMeshProUGUI playerHealthbarText;
    [SerializeField] Image playerEnergybar;
    [SerializeField] TextMeshProUGUI playerEnergybarText;
    [SerializeField] Image playerEnergybarBG;
    [SerializeField] GameObject playerDamageScreenFlash;
    [SerializeField] GameObject healedIcons;
    [SerializeField] GameObject[] weaponSlots;
    [Tooltip("Parent group of all weapon-related things: energy bar, weapon inventory UI, reload HUD, etc.")]
    [SerializeField] GameObject weaponHUD;
    public ReloadHUD reloadHUD;

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

    public delegate void EventHandler();
    public static event EventHandler OnKeyPicked;
    public event EventHandler AmbushStarted;
    public static event EventHandler AmbushRewardDropped;
    public static event EventHandler AmbushRewardPickedEvent;

    bool wasAmbushTriggered;
    public bool WasAmbushTriggered { get => wasAmbushTriggered; }


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

        AmbushStarted += GameManager_AmbushStarted;
        playerScript.SpawnedEvent += OnPlayerSpawned;
        playerScript.Hurt += PlayerScript_Hurt;
        playerScript.Healed += PlayerScript_Healed;

        LoadGeneralSettings();

        if (levelId != LevelIdEnum.Other)
            LoadLevelStartData();
    }

    private void PlayerScript_Healed()
    {
        healedIcons.GetComponent<Animator>().SetTrigger("Healed");
        playerDamageScreenFlash.SetActive(false);
    }

    private void PlayerScript_Hurt()
    {
        StartCoroutine(FlashRedScreen());
    }

    IEnumerator FlashRedScreen()
    {
        playerDamageScreenFlash.SetActive(true); // Flash red

        yield return new WaitForSeconds(hurtFlashDuration);

        playerDamageScreenFlash.SetActive(false);
    }

    private void PlayerScript_WeaponSwitched(weaponStats weapon)
    {
        //weaponHUD.gameObject.SetActive(playerScript.GetWeaponList().Count > 0);

        // Ideally the weapon UI icon should be updated only once in a separate WeaponPicked event
        // method, but due to the lack of one, it is done here
        if (playerScript.SelectedWeaponStats != null)
        {
            // Update slot icons
            for (int i = 0; i < playerScript.GetWeaponList().Count; i++)
                weaponSlots[i].GetComponent<WeaponSlotScript>().FillSlot(playerScript.GetWeaponList()[i].Icon);

            // Reflect selection in UI
            for (int i = 0; i < weaponSlots.Length; i++)
            {
                weaponSlots[i].GetComponent<WeaponSlotScript>().Select(playerScript.SelectedWeapon == i);
            }
        }

    }

    void LoadGeneralSettings()
    {
        GeneralSettingsData generalSettingsData = saveSystem.LoadGeneralSettings();

        playerScript.EnableTilt(generalSettingsData.tiltEnabled);
        Camera.main.GetComponent<cameraController>()?.SetMouseSensitivity(generalSettingsData.mouseSensitivity);
        Camera.main.GetComponent<cameraController>()?.SetInvertY(generalSettingsData.invertY);
    }

    void LoadLevelStartData()
    {
        LevelSaveData levelData = saveSystem.LoadLevelData(levelId);

        weaponStats firstWeapon = levelData.startWeapons[0] != -1 ? helper.weaponList[(levelData.startWeapons[0])] : null;
        weaponStats secondWeapon = levelData.startWeapons[1] != -1 ? helper.weaponList[(levelData.startWeapons[1])] : null;
        int selectedWeapon = levelData.startWeaponSelected;

        playerScript.pickupWeapon(firstWeapon);
        playerScript.pickupWeapon(secondWeapon);
        playerScript.EquipWeaponFromSlot(selectedWeapon);
    }

    IEnumerator Start()
    {
        // Force update the weapon selection UI
        PlayerScript_WeaponSwitched(playerScript.SelectedWeaponStats);

        UpdateEnemyCountText();
        ShowHint("Good Luck!");

        EnterGameState(defaultGameState);

        if (playerSpawnPosition != null)
        {
            player.transform.position = playerSpawnPosition.transform.position;
            player.transform.rotation = playerSpawnPosition.transform.rotation;
        }

        if (spawnAmbushOnKeyPicked)
        {

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

        soundtrackManager.PlayGameStateMusic(curGameState);
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

        OnKeyPicked?.Invoke();

        if (spawnAmbushOnKeyPicked)
        {
            if (!wasAmbushTriggered)
            {
                AmbushStarted?.Invoke();
            }
        }
        else
        {
            ShowHint("No ambush!\nProceed to the portal");
        }
    }

    private void GameManager_AmbushStarted()
    {
        wasAmbushTriggered = true;
        EnterGameState(GameStates.Ambush);
        ShowHint("Ambush!\nYou Collected The Key\nEscape though the portal\n or kill them all");

        // Backward compatibility code
        enemyManager.instance.ambushSpawner?.gameObject.SetActive(true);
        enemyManager.instance.ambushSpawner?.StartAmbush();
    }

    /// <summary>
    /// Spawn the ambush reward and inform the player about it.
    /// </summary>
    /// <param name="pos">Spawn position.</param>
    public void SpawnAmbushReward(Vector3 pos)
    {
        IsAmbushRewardDropped = true;
        AmbushRewardDropped?.Invoke();

        EnterGameState(GameStates.AmbushDefeated);
        if (ambushRewardPrefab != null)
            Instantiate(ambushRewardPrefab, pos, Quaternion.identity);

        gameManager.instance.ShowHint("Enemy Dropped Ambush Reward");
    }

    /// <summary>
    /// Trigger ambush reward pickup events.
    /// </summary>
    /// <param name="isEarnedAmbushReward">Whether the reward was for an ambush. Should be false for secret items</param>
    public void ambushRewardPicked(bool isEarnedAmbushReward = true)
    {
        playerScript.ApplyAmbushDefeatPowerup(1);
     
        if (isEarnedAmbushReward)
        {
            IsAmbushRewardPicked = true;
            AmbushRewardPickedEvent?.Invoke();
        }
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
