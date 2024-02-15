using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class soundtrackManager : MonoBehaviour
{
    [Header("---- Audio Clips ----")]
    [SerializeField] List<AudioClip> menuSoundtracks = new List<AudioClip>();
    [SerializeField] List<AudioClip> calmSoundtracks = new List<AudioClip>();
    [SerializeField] List<AudioClip> combatSoundtracks = new List<AudioClip>();
    [SerializeField] List<AudioClip> ambushSoundtracks = new List<AudioClip>();

    [Header("---- Audio Sources ----")]
    [SerializeField] AudioSource menuAudioSource;
    [SerializeField] AudioSource calmAudioSource;
    [SerializeField] AudioSource combatAudioSource;
    [SerializeField] AudioSource activeAudioSource;

    public void PlayCalmMusic()
    {
        if (activeAudioSource == calmAudioSource)
            return;

        if (activeAudioSource != null)
            activeAudioSource.Stop();

        if (calmSoundtracks.Count > 0)
        {
            calmAudioSource.clip = calmSoundtracks[Random.Range(0, calmSoundtracks.Count)];
            calmAudioSource.Play();
        }

        activeAudioSource = calmAudioSource;
    }

    public void PlayCombatMusic()
    {
        if (activeAudioSource == combatAudioSource)
            return;

        if (activeAudioSource != null)
            activeAudioSource.Stop();

        if (combatSoundtracks.Count > 0)
        {
            combatAudioSource.clip = combatSoundtracks[Random.Range(0, combatSoundtracks.Count)];
            combatAudioSource.Play();
        }

        activeAudioSource = combatAudioSource;
    }

    public void PlayMenuMusic()
    {
        if (activeAudioSource == menuAudioSource)
            return;

        if (activeAudioSource != null)
            activeAudioSource.Stop();

        if (menuSoundtracks.Count > 0)
        {
            menuAudioSource.clip = menuSoundtracks[Random.Range(0, menuSoundtracks.Count)];
            menuAudioSource.Play();
        }

        activeAudioSource = menuAudioSource;
    }
}
