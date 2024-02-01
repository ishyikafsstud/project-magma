using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]

public class weaponStats : ScriptableObject
{
    public int shootDamage;
    public float shootRate;
    public int shootDist;
    public float energyCostPerShot;

    public GameObject model;
    public ParticleSystem hitEffect;
    public AudioClip shootSound;
    [Range(0, 1)] public float shootSoundVolume;
}
