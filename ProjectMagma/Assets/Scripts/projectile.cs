using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class projectile : MonoBehaviour
{
    [SerializeField] Rigidbody rb;

    // Reference to the WeaponType enum in weaponStats.cs so we can check if its a projectile
    // based weapon. If yes, we can create the appropriate hit effect.
    [SerializeField] weaponStats.WeaponTypes weaponType;
    [Tooltip("Add hit effect for projectile based weapons here.")]
    [SerializeField] GameObject projectileHitEffect;

    [SerializeField] int damageAmount;
    [SerializeField] int speed;
    /// <summary>
    /// After what time the projectile automatically gets destroyed if encountered no obstacle.
    /// </summary>
    [SerializeField] int destroyTime;
    [SerializeField] Types type;

    enum Types
    {
        Basic,
        Ice,
        Fire,
    }

    public int DamageValue
    {
        get => damageAmount;
        set => damageAmount = value;
    }

    // Start is called before the first frame update
    void Start()
    {
        rb.velocity = transform.forward * speed;
        // bullet is destroyed after an amount of time
        Destroy(gameObject, destroyTime);
    }

    // look for collisions
    private void OnTriggerEnter(Collider other)
    {
        // ignores other triggers, to prevent self damage
        if (other.isTrigger)
        {
            return;
        }
        // if collided object has IDamage, it takes damage
        IDamage dmg = other.GetComponent<IDamage>();

        if (dmg != null)
        {
            dmg.takeDamage(damageAmount);

            if (type == Types.Ice)
            {
                StartCoroutine(dmg.ApplyFreeze(1));
            }
        }

        if (weaponType == weaponStats.WeaponTypes.Projectile && projectileHitEffect != null)
        {
            CreateProjectileHitEffect(transform.position);
        }

        // bullet is destroyed after colliding
        Destroy(gameObject);
    }

    // Creates Hit effect for projectile based weapons
    private void CreateProjectileHitEffect(Vector3 position)
    {
        Instantiate(projectileHitEffect, position, Quaternion.identity);
    }
}