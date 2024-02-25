using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class projectile : MonoBehaviour
{
    [Header("---- Projectile Settings ----")]
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

    [Header("---- Bomb/AOE Projectile Settings ---- ")]
    [SerializeField] GameObject explosionEffect;
    [Range(0.1f, 5.0f)][SerializeField] float explosionDelay;
    [SerializeField] int explosionRadius;
    [SerializeField] int explosionDamage;

    enum Types
    {
        Basic,
        Ice,
        Fire,
        Poison,
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

        switch (type)
        {
            case Types.Fire:
                Vector3 aoePos = transform.position;
                aoePos += Vector3.up * 0.1f; // Raise a little to prevent clipping

                // If hitting an enemy, use their feet position as the fire AOE origin
                if (other.GetComponent<enemyAI>() != null)
                    aoePos = other.transform.position;

                Instantiate(explosionEffect, aoePos, Quaternion.identity);
                break;

            case Types.Poison:
                StickToObject(other);
                StartCoroutine(ExplosionDelay());
                return;
        }

        if (weaponType == weaponStats.WeaponTypes.Projectile && projectileHitEffect != null)
        {
            CreateProjectileHitEffect(transform.position);
        }

        // bullet is destroyed after colliding
        Destroy(gameObject);
    }

    private void StickToObject(Collider other)
    {
        transform.parent = other.transform;
        Destroy(rb);
    }

    IEnumerator ExplosionDelay()
    {
        yield return new WaitForSeconds(explosionDelay);
        Explode();
    }

    void Explode()
    {
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider col in colliders)
        {
            if (col.CompareTag("Player"))
            {
                continue;
            }

            IDamage dmg = col.GetComponent<IDamage>();
            if (dmg != null)
            {
                dmg.takeDamage(explosionDamage);
            }
        }
        Destroy(gameObject);
    }

    // Creates Hit effect for projectile based weapons
    private void CreateProjectileHitEffect(Vector3 position)
    {
        Instantiate(projectileHitEffect, position, Quaternion.identity);
    }
}