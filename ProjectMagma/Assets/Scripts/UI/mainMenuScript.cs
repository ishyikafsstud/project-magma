using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mainMenuScript : MonoBehaviour
{
    SoundtrackManager soundtrackManager;

    private void Awake()
    {
        soundtrackManager = GameObject.FindGameObjectWithTag("SoundtrackManager").GetComponent<SoundtrackManager>();
    }

    void Start()
    {
        soundtrackManager.PlayMenuMusic();
    }
}
