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
    private static string tiltEnabledKey = "TILT_ENABLED";

    private static string GetLevelPrefix(LevelIdEnum levelId)
    { return levelId.ToString().ToUpper(); }
    private static string levelUnlockedKeySuffix = "_UNLOCKED";
    private static string levelStartWeapon1KeySuffix = "_STARTWEAPON_1";
    private static string levelStartWeapon2KeySuffix = "_STARTWEAPON_2";
    private static string levelCompletedKeySuffix = "_COMPLETED";
    private static string levelAmbushDefeatedKeySuffix = "_AMBUSHDEFEATED";
    private static string levelEndWeapon1KeySuffix = "_ENDWEAPON_1";
    private static string levelEndWeapon2KeySuffix = "_ENDWEAPON_2";

    public static void LoadGeneralSettings()
    {

    }

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
        PlayerPrefs.SetInt(levelPrefix + levelEndWeapon1KeySuffix, (int)playerWeapons[0].wandType);
        PlayerPrefs.SetInt(levelPrefix + levelEndWeapon2KeySuffix, (int)playerWeapons[1].wandType);
        // Ambush defeated
        // TODO: implement ambush defeated fact saving


        // If it is not the last level, then unlock the next level + set the starting weapons for it
        if (levelId != LevelIdEnum.Level5)
        {
            LevelIdEnum nextLevelId = levelId + 1;
            string nextLevelPrefix = GetLevelPrefix(nextLevelId);

            // Mark as Unlocked
            PlayerPrefs.SetInt(nextLevelPrefix + levelUnlockedKeySuffix, 1);

            // Set start weapons
            PlayerPrefs.SetInt(nextLevelPrefix + levelStartWeapon1KeySuffix, (int)playerWeapons[0].wandType);
            PlayerPrefs.SetInt(nextLevelPrefix + levelStartWeapon2KeySuffix, (int)playerWeapons[1].wandType);
        }
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

    public void SetSettingsToDefault()
    {
        PlayerPrefs.SetInt(masterVolumeKey, 100);
        PlayerPrefs.SetInt(sfxVolumeKey, 100);
        PlayerPrefs.SetInt(musicVolumeKey, 100);
        PlayerPrefs.SetInt(uiVolumeKey, 100);
        PlayerPrefs.SetInt(tiltEnabledKey, 1);
    }

    public static void EraseLevelProgression()
    {
        // Remember general settings

        // Clear the entire PlayerPrefs for ease

        // Reassign general settings
    }
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
