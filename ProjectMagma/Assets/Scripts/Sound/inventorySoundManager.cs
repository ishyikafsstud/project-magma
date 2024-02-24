using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class inventorySoundManager : soundManagerBase
{
    [SerializeField] AudioMixerGroup mixerGroup;
    public AudioMixerGroup MixerGroup { get => mixerGroup; }

    [Header("----- Audio SFX -----")]
    [SerializeField] AudioSource weaponPickedSFX;
    [SerializeField] AudioSource weaponSwitchedSFX;
    [SerializeField] AudioClip weaponDroppedSFX;
    public AudioClip WeaponDroppedSFX { get => weaponDroppedSFX; }

    public void PlayWeaponPicked(float minPitch = 0.9f, float maxPitch = 1.1f)
    {
        PlaySound(weaponPickedSFX, minPitch, maxPitch);
    }

    public void PlayWeaponSwitched(float minPitch = 0.9f, float maxPitch = 1.1f)
    {
        PlaySound(weaponSwitchedSFX, minPitch, maxPitch);
    }
}
