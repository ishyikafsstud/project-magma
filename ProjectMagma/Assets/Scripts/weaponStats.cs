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

    public WeaponTypes weaponType;
    [Tooltip("Prefab of the projectile shot by this weapon. Ignore for raycast-based weapons.")]
    public GameObject projectilePrefab;
    public int projectileDamage;

    public GameObject model;
    public ParticleSystem hitEffect;
    public AudioClip shootSound;
    [Range(0, 1)] public float shootSoundVolume;

    public enum WeaponTypes
    {
        Raycast,
        Projectile,
    }
}
