using ExtensionMethods;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class inventorySoundManager : soundManagerBase
{
    [Header("----- Audio SFX -----")]
    [SerializeField] AudioSource weaponPickedSFX;
    [SerializeField] AudioSource weaponSwitchedSFX;
    [SerializeField] AudioSource weaponDroppedSFX;

    public void PlayWeaponPicked(float minPitch = 0.9f, float maxPitch = 1.1f)
    {
        PlaySound(weaponPickedSFX, minPitch, maxPitch);
    }

    public void PlayWeaponSwitched(float minPitch = 0.9f, float maxPitch = 1.1f)
    {
        PlaySound(weaponSwitchedSFX, minPitch, maxPitch);
    }

    public void PlayWeaponDroppedSpatial(Vector3 pos)
    {
        AudioSourceExtension.PlayClipAtPoint(weaponDroppedSFX.clip, pos,
            1.0f, weaponDroppedSFX.outputAudioMixerGroup);
    }
}
