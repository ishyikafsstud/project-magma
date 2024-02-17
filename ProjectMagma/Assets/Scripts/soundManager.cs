using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class soundManager : MonoBehaviour
{
    [Header("----- Player Audio SFX -----")]
    [SerializeField] public AudioSource hitSFX;
    [SerializeField] public AudioSource jumpSFX;
    [SerializeField] public AudioSource deathSFX;

    [Header("----- Weapon Audio SFX -----")]
    [SerializeField] public AudioSource meleeSFX;
    [SerializeField] public AudioSource fireCastSFX;
    [SerializeField] public AudioSource fireHitSFX;
    [SerializeField] public AudioSource iceCastSFX;
    [SerializeField] public AudioSource iceHitSFX;
    [SerializeField] public AudioSource electricCastSFX;
    [SerializeField] public AudioSource electricHitSFX;
    [SerializeField] public AudioSource rockCastSFX;
    [SerializeField] public AudioSource rockHitSFX;
    [SerializeField] public AudioSource poisonCastSFX;
    [SerializeField] public AudioSource poisonHitSFX;

    [Header("----- Enemy Audio SFX -----")]
    [SerializeField] public AudioSource spawnerSFX;
    [SerializeField] public AudioSource skeletonSwordSFX;
    [SerializeField] public AudioSource spiderlingSFX;
    [SerializeField] public AudioSource spiderSFX;
    [SerializeField] public AudioSource slimeSFX;


}
