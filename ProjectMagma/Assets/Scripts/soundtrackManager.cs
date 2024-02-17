using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundtrackManager : MonoBehaviour
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

    [Header("---- Other ----")]

    UnityEngine.Audio.AudioMixer audioMixer;


    private void Start()
    {
        audioMixer = menuAudioSource.outputAudioMixerGroup.audioMixer;
        ApplyVolumeSettings(saveSystem.LoadGeneralSettings());
    }

    public void ApplyVolumeSettings(GeneralSettingsData settings)
    {
        audioMixer.SetFloat("master", Mathf.Log10(settings.masterVolume) * 20);
        audioMixer.SetFloat("sfx", Mathf.Log10(settings.sfxVolume) * 20);
        audioMixer.SetFloat("music", Mathf.Log10(settings.musicVolume) * 20);
        audioMixer.SetFloat("ui", Mathf.Log10(settings.uiVolume) * 20);
    }

    public void PlayGameStateMusic(gameManager.GameStates state)
    {
        switch (state)
        {
            case gameManager.GameStates.Calm:
                PlayCalmMusic();
                break;
            case gameManager.GameStates.Combat:
                PlayCombatMusic();
                break;
            case gameManager.GameStates.Ambush:
                PlayCombatMusic();
                break;
            case gameManager.GameStates.AmbushDefeated:
                PlayCalmMusic();
                break;
        }

        activeAudioSource.UnPause();
    }

    /// <summary>
    /// Pause or unpause current music.
    /// </summary>
    /// <param name="pause">Pause (true) or unpause (false)</param>
    public void PauseMusic(bool pause = true)
    {
        if (pause)
            activeAudioSource.Pause();
        else
            activeAudioSource.UnPause();
    }

    void PlayMusic(AudioSource newSource, List<AudioClip> newMusic)
    {
        if (activeAudioSource == newSource)
            return;

        if (activeAudioSource != null)
            activeAudioSource.Stop();

        if (newMusic.Count > 0)
        {
            newSource.clip = newMusic[Random.Range(0, newMusic.Count)];
            newSource.Play();
        }

        activeAudioSource = newSource;
    }

    public void PlayCalmMusic()
    {
        PlayMusic(calmAudioSource, calmSoundtracks);
    }

    public void PlayCombatMusic()
    {
        PlayMusic(combatAudioSource, combatSoundtracks);
    }

    public void PlayMenuMusic()
    {
        PlayMusic(menuAudioSource, menuSoundtracks);
    }
}
