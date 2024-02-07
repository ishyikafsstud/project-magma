using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class projectile : MonoBehaviour
{
    [SerializeField] Rigidbody rb;


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

        if (dmg != null && !other.CompareTag("Enemy"))
        {
            dmg.takeDamage(damageAmount);
        }

        // bullet is destroyed after colliding
        Destroy(gameObject);
    }
}