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

    /// <summary>
    /// Describes the underlying implementation of the weapon.
    /// </summary>
    public WeaponTypes weaponType;
    /// <summary>
    /// The specific kind of wand it is.
    /// </summary>
    public WandType wandType;
    [Tooltip("Prefab of the projectile shot by this weapon. Ignore for raycast-based weapons.")]
    public GameObject projectilePrefab;

    public GameObject model;
    public ParticleSystem hitEffect;
    public AudioClip shootSound;
    [Range(0, 1)] public float shootSoundVolume;

    public enum WeaponTypes
    {
        Raycast,
        Projectile,
    }

    public enum WandType
    {
        Electric,
        Rock,
        Ice,
        Fire,
        Poison,
        Undefined,
    }
}
