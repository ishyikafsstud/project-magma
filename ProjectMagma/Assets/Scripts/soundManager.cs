using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class soundManager : MonoBehaviour
{
    [Header("----- Audio SFX -----")]
    [SerializeField] AudioSource hurtSFX;
    [SerializeField] AudioSource jumpSFX;
    [SerializeField] AudioSource deathSFX;
    [SerializeField] AudioSource attackStartSFX;
    [SerializeField] AudioSource attackMiddleSFX;
    [SerializeField] AudioSource altAttackSFX;

    public void PlayHurt()
    {
        if (hurtSFX != null && hurtSFX.clip != null)
            hurtSFX.Play();
    }
    public void PlayJump()
    {
        if (jumpSFX != null && jumpSFX.clip != null)
            jumpSFX.Play();
    }
    public void PlayDeath()
    {
        if (deathSFX != null && deathSFX.clip != null)
            deathSFX.Play();
    }
    public void PlayAttackStart()
    {
        if (attackStartSFX != null && attackStartSFX.clip != null)
            attackStartSFX.Play();
    }
    public void PlayAttackMiddle()
    {
        if (attackMiddleSFX != null && attackMiddleSFX.clip != null)
            attackMiddleSFX.Play();
    }
    public void PlayAltAttack()
    {
        if (altAttackSFX != null && altAttackSFX.clip != null)
            altAttackSFX.Play();
    }
}
