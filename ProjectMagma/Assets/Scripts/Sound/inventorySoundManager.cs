using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class inventorySoundManager : soundManagerBase
{
    [Header("----- Audio SFX -----")]
    [SerializeField] AudioSource weaponPickedSFX;
    [SerializeField] AudioSource weaponSwitchedSFX;

    public void PlayWeaponPicked(float minPitch = 0.9f, float maxPitch = 1.1f)
    {
        PlaySound(weaponPickedSFX, minPitch, maxPitch);
    }

    public void PlayWeaponSwitched(float minPitch = 0.9f, float maxPitch = 1.1f)
    {
        PlaySound(weaponSwitchedSFX, minPitch, maxPitch);
    }
}
