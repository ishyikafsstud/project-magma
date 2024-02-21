using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class entitySoundManager : soundManagerBase
{
    [Header("----- Audio SFX -----")]
    [SerializeField] AudioSource hurtSFX;
    [SerializeField] AudioSource jumpSFX;
    [SerializeField] AudioSource deathSFX;
    [SerializeField] AudioSource barkSFX;
    [SerializeField] AudioSource attackStartSFX;
    [SerializeField] AudioSource attackMiddleSFX;

    public void PlayHurt(float minPitch = 0.9f, float maxPitch = 1.1f)
    {
        PlaySound(hurtSFX, minPitch, maxPitch);
    }
    public void PlayJump(float minPitch = 0.9f, float maxPitch = 1.1f)
    {
        PlaySound(jumpSFX, minPitch, maxPitch);
    }
    public void PlayDeath(float minPitch = 0.9f, float maxPitch = 1.1f)
    {
        PlaySound(deathSFX, minPitch, maxPitch);
    }
    public void PlayAttackStart(float minPitch = 0.9f, float maxPitch = 1.1f)
    {
        PlaySound(attackStartSFX, minPitch, maxPitch);
    }
    public void PlayAttackMiddle(float minPitch = 0.9f, float maxPitch = 1.1f)
    {
        PlaySound(attackMiddleSFX, minPitch, maxPitch);
    }

    public void PlayBark(float minPitch = 0.9f, float maxPitch = 1.1f)
    {
        PlaySound(barkSFX, minPitch, maxPitch);
    }
}
