using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class mainMenuScript : MonoBehaviour
{
    SoundtrackManager soundtrackManager;

    [Header("---- UI elements ----")]
    [SerializeField] Button[] levelSelectionButtons;
    [SerializeField] Button continueButton;

    List<gameManager.LevelIdEnum> unlockedLevels = new List<gameManager.LevelIdEnum>();


    private void Awake()
    {
        soundtrackManager = GameObject.FindGameObjectWithTag("SoundtrackManager").GetComponent<SoundtrackManager>();
    }

    void Start()
    {
        soundtrackManager.PlayMenuMusic();

        unlockedLevels = saveSystem.GetUnlockedLevels();

        for (int i = 0; i < levelSelectionButtons.Length; i++)
        {
            levelSelectionButtons[i].interactable = unlockedLevels.Contains((gameManager.LevelIdEnum)i + 1);
        }
        // If the player has some level progress (i.e., at least completed level one)
        if (unlockedLevels.Count > 1)
        {
            continueButton.interactable = true;
        }
        // If the player has not finished a single level
        else
        {
            continueButton.interactable = false;
        }
    }
}
