using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static gameManager;
using static weaponStats;

public abstract class saveSystem : MonoBehaviour
{
    /*
     Data to remember:

        * General settings
            * Master volume (int)
            * SFX volume (int)
            * Music volume (int)
            * UI volume (int)
            * Mouse sensitivity
            * Tilt enabled/disabled
            
        * Game progression
            * Data for every level
                * Is the level unlocked? (bool)
                * What weapons the player started with? (two ints as enum values)
                * Was the level completed? (bool)
                * Was the ambush defeated and the ambush reward received? (bool)
                * What weapons the player ended with? (two ints as enum values)
                * Level completion time (int or float) (beta task)
    */

    private static string masterVolumeKey = "MASTER_VOLUME";
    private static string sfxVolumeKey = "SFX_VOLUME";
    private static string musicVolumeKey = "MUSIC_VOLUME";
    private static string uiVolumeKey = "UI_VOLUME";
    private static string mouseSensitivityKey = "MOUSE_SENSITIVITY";
    private static string tiltEnabledKey = "TILT_ENABLED";
    private static string invertYKey = "INVERT_Y_ENABLED";

    private static string GetLevelPrefix(LevelIdEnum levelId)
    { return levelId.ToString().ToUpper(); }
    private static string levelUnlockedKeySuffix = "_UNLOCKED";
    private static string levelStartWeapon1KeySuffix = "_STARTWEAPON_1";
    private static string levelStartWeapon2KeySuffix = "_STARTWEAPON_2";
    private static string levelCompletedKeySuffix = "_COMPLETED";
    private static string levelAmbushDefeatedKeySuffix = "_AMBUSHDEFEATED";
    private static string levelEndWeapon1KeySuffix = "_ENDWEAPON_1";
    private static string levelEndWeapon2KeySuffix = "_ENDWEAPON_2";

    public delegate void EventHandler();
    public static event EventHandler SaveFileUpdated;

    public delegate void NumericSettingCallback(float value);
    public static event NumericSettingCallback MouseSensitivitySet;
    
    public delegate void BoolSettingCallback(bool value);
    public static event BoolSettingCallback TiltSet;
    public static event BoolSettingCallback InvertYSet;

    public static void SaveGeneralSettings(GeneralSettingsData generalSettingsData)
    {
        PlayerPrefs.SetFloat(masterVolumeKey, generalSettingsData.masterVolume);
        PlayerPrefs.SetFloat(sfxVolumeKey, generalSettingsData.sfxVolume);
        PlayerPrefs.SetFloat(musicVolumeKey, generalSettingsData.musicVolume);
        PlayerPrefs.SetFloat(uiVolumeKey, generalSettingsData.uiVolume);

        PlayerPrefs.SetInt(mouseSensitivityKey, generalSettingsData.mouseSensitivity);
        PlayerPrefs.SetInt(tiltEnabledKey, generalSettingsData.tiltEnabled == true ? 1 : 0);

        PlayerPrefs.Save();
        SaveFileUpdated?.Invoke();
    }

    public static void SaveVolume(string mixerGroup, float volume)
    {
        switch (mixerGroup)
        {
            case "master":
                PlayerPrefs.SetFloat(masterVolumeKey, volume);
                break;

            case "sfx":
                PlayerPrefs.SetFloat(sfxVolumeKey, volume);
                break;

            case "music":
                PlayerPrefs.SetFloat(musicVolumeKey, volume);
                break;

            case "ui":
                PlayerPrefs.SetFloat(uiVolumeKey, volume);
                break;

            default:
                break;
        }
        
        PlayerPrefs.Save();
        SaveFileUpdated?.Invoke();
    }

    public static void SaveMasterVolume(float volume)
    {
        PlayerPrefs.SetFloat(masterVolumeKey, volume);
        PlayerPrefs.Save();
    }

    public static void SaveSfxVolume(float volume)
    {
        PlayerPrefs.SetFloat(sfxVolumeKey, volume);
        PlayerPrefs.Save();
    }

    public static void SaveMusicVolume(float volume)
    {
        PlayerPrefs.SetFloat(musicVolumeKey, volume);
        PlayerPrefs.Save();
    }

    public static void SaveUiVolume(float volume)
    {
        PlayerPrefs.SetFloat(uiVolumeKey, volume);
        PlayerPrefs.Save();
    }

    public static void SaveSensitivity(int value)
    {
        PlayerPrefs.SetInt(mouseSensitivityKey, value);
        PlayerPrefs.Save();
        SaveFileUpdated?.Invoke();

        MouseSensitivitySet?.Invoke(value);
    }

    public static void SaveTilt(bool enabled)
    {
        PlayerPrefs.SetInt(tiltEnabledKey, enabled == true ? 1 : 0);
        PlayerPrefs.Save();
        SaveFileUpdated?.Invoke();

        TiltSet?.Invoke(enabled);
    }

    public static void SaveInvertY(bool enabled)
    {
        PlayerPrefs.SetInt(invertYKey, enabled == true ? 1 : 0);
        PlayerPrefs.Save();
        SaveFileUpdated?.Invoke();

        InvertYSet?.Invoke(enabled);
    }

    public static GeneralSettingsData LoadGeneralSettings()
    {
        GeneralSettingsData generalSettingsData = new GeneralSettingsData();

        generalSettingsData.masterVolume = PlayerPrefs.GetFloat(masterVolumeKey, 1);
        generalSettingsData.sfxVolume = PlayerPrefs.GetFloat(sfxVolumeKey, 1);
        generalSettingsData.musicVolume = PlayerPrefs.GetFloat(musicVolumeKey, 1);
        generalSettingsData.uiVolume = PlayerPrefs.GetFloat(uiVolumeKey, 1);

        generalSettingsData.mouseSensitivity = PlayerPrefs.GetInt(mouseSensitivityKey, 400);
        generalSettingsData.tiltEnabled = PlayerPrefs.GetInt(tiltEnabledKey, 1) == 1 ? true : false;
        generalSettingsData.invertY = PlayerPrefs.GetInt(invertYKey, 0) == 1 ? true : false;

        return generalSettingsData;
    }

    #region Level Progression

    /// <summary>
    /// Remember this level as completed, whether the ambush was defeated, and the weapons at the
    /// end of the level.
    /// <br>If it is not the last level, unlock the next level and update its starting
    /// weapons.</br>
    /// <para>IMPORTANT: This method is meant to be called only on level completion, and the code is written with
    /// this assumption in mind.</para>
    /// </summary>
    /// <param name="levelId">ID of the saved level.</param>
    /// <param name="gameManagerInst">gameManager instance, which will serve as the source of all
    /// needed info to save.</param>
    public static void SaveLevelData(LevelIdEnum levelId, gameManager gameManagerInst)
    {
        string levelPrefix = GetLevelPrefix(levelId);

        playerController player = gameManagerInst.playerScript;
        var playerWeapons = player.GetWeaponList();

        // Mark as Unlocked - yes (should already be true, but overwrite just in case
        PlayerPrefs.SetInt(levelPrefix + levelUnlockedKeySuffix, 1);
        // Mark as completed
        PlayerPrefs.SetInt(levelPrefix + levelCompletedKeySuffix, 1);
        // Remember end weapons
        PlayerPrefs.SetInt(levelPrefix + levelEndWeapon1KeySuffix, playerWeapons.Count >= 1 ? (int)playerWeapons[0].wandType : -1);
        PlayerPrefs.SetInt(levelPrefix + levelEndWeapon2KeySuffix, playerWeapons.Count >= 2 ? (int)playerWeapons[1].wandType : -1);
        // Ambush defeated
        PlayerPrefs.SetInt(levelPrefix + levelAmbushDefeatedKeySuffix, gameManagerInst.IsAmbushRewardPicked ? 1 : 0);


        // If it is not the last level, then unlock the next level + set the starting weapons for it
        if (levelId != LevelIdEnum.Level5)
        {
            LevelIdEnum nextLevelId = levelId + 1;
            string nextLevelPrefix = GetLevelPrefix(nextLevelId);

            // Mark as Unlocked
            PlayerPrefs.SetInt(nextLevelPrefix + levelUnlockedKeySuffix, 1);

            // Set start weapons
            PlayerPrefs.SetInt(nextLevelPrefix + levelStartWeapon1KeySuffix, playerWeapons.Count >= 1 ? (int)playerWeapons[0].wandType : -1);
            PlayerPrefs.SetInt(nextLevelPrefix + levelStartWeapon2KeySuffix, playerWeapons.Count >= 2 ? (int)playerWeapons[1].wandType : -1);
        }

        PlayerPrefs.Save();
        SaveFileUpdated?.Invoke();
    }

    public static LevelSaveData LoadLevelData(LevelIdEnum levelId)
    {
        string levelPrefix = GetLevelPrefix(levelId);
        LevelSaveData levelData = new LevelSaveData();
        levelData.startWeapons = new List<int>();
        levelData.endWeapons = new List<int>();

        levelData.levelId = levelId;
        levelData.isUnlocked = PlayerPrefs.GetInt(levelPrefix + levelUnlockedKeySuffix, 0) == 1 ? true : false;
        levelData.startWeapons.Add(PlayerPrefs.GetInt(levelPrefix + levelStartWeapon1KeySuffix, -1));
        levelData.startWeapons.Add(PlayerPrefs.GetInt(levelPrefix + levelStartWeapon2KeySuffix, -1));
        levelData.isCompleted = PlayerPrefs.GetInt(levelPrefix + levelCompletedKeySuffix, 0) == 1 ? true : false;
        levelData.endWeapons.Add(PlayerPrefs.GetInt(levelPrefix + levelEndWeapon1KeySuffix, -1));
        levelData.endWeapons.Add(PlayerPrefs.GetInt(levelPrefix + levelEndWeapon1KeySuffix, -1));
        levelData.isAmbushDefeated = PlayerPrefs.GetInt(levelPrefix + levelAmbushDefeatedKeySuffix, 0) == 1 ? true : false;

        return levelData;
    }

    public void ResetGeneralSettings()
    {
        PlayerPrefs.SetInt(masterVolumeKey, 100);
        PlayerPrefs.SetInt(sfxVolumeKey, 100);
        PlayerPrefs.SetInt(musicVolumeKey, 100);
        PlayerPrefs.SetInt(uiVolumeKey, 100);
        PlayerPrefs.SetInt(tiltEnabledKey, 1);

        PlayerPrefs.Save();
    }

    /// <summary>
    /// Resets level progression.
    /// <para>Currently, it just locks all levels past Level1.</para>
    /// </summary>
    public static void ResetLevelProgression()
    {
        // Lock all levels after Level1
        for (LevelIdEnum levelId = LevelIdEnum.Level2; levelId <= LevelIdEnum.LAST_LEVEL; levelId++)
        {
            PlayerPrefs.SetInt(GetLevelPrefix(levelId) + levelUnlockedKeySuffix, 0);
        }

        PlayerPrefs.Save();
        SaveFileUpdated?.Invoke();
    }

    /// <summary>
    /// Count ambushes defeated.
    /// <para>By default, counts the total number of ambushes defeated. Use the parameter to count
    /// defeated ambushes up to a specified level.</para>
    /// </summary>
    /// <param name="upToLevel">Up to which level to count the defeated ambushes.</param>
    /// <returns></returns>
    public static int CountAmbushesDefeated(LevelIdEnum upToLevel = LevelIdEnum.LAST_LEVEL + 1)
    {
        int ambushesDefeated = 0;

        for (LevelIdEnum levelId = LevelIdEnum.Level1; levelId < upToLevel; levelId++)
        {
            ambushesDefeated += PlayerPrefs.GetInt(GetLevelPrefix(levelId) + levelAmbushDefeatedKeySuffix, 0);
        }

        return ambushesDefeated;
    }

    public static bool IsLevelUnlocked(LevelIdEnum levelId)
    {
        return PlayerPrefs.GetInt(GetLevelPrefix(levelId) + levelUnlockedKeySuffix) == 1;
    }

    public static List<LevelIdEnum> GetUnlockedLevels()
    {
        List<LevelIdEnum> unlockedLevels = new List<LevelIdEnum>();

        // Get the unlocked levels
        for (LevelIdEnum levelId = LevelIdEnum.Level1; levelId < LevelIdEnum.LAST_LEVEL; levelId++)
        {
            bool levelUnlocked = PlayerPrefs.GetInt(GetLevelPrefix(levelId) + levelUnlockedKeySuffix, 0) == 1;

            if (levelUnlocked)
                unlockedLevels.Add(levelId);
            // If a level is locked, assume all the consequent levels are locked too and stop checking early
            else
                break;
        }

        return unlockedLevels;
    }
    #endregion
}

public struct LevelSaveData
{
    public LevelIdEnum levelId;

    public bool isUnlocked;
    public List<int> startWeapons;
    public bool isCompleted;
    public List<int> endWeapons;
    public bool isAmbushDefeated;
}

public struct GeneralSettingsData
{
    public float masterVolume;
    public float sfxVolume;
    public float musicVolume;
    public float uiVolume;

    public int mouseSensitivity;
    public bool tiltEnabled;
    public bool invertY;
}
