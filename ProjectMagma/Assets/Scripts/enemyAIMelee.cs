using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemyAIMelee : enemyAI
{
    [Header("Melee")]
    [SerializeField] float particleDuration;
    [SerializeField] GameObject hitParticlesPrefab;
     
    /// <summary>
    /// Attack the target in melee.
    /// </summary>
    /// <returns></returns>
    protected override IEnumerator Attack()
    {
        if (isAttacking)
        {
            yield break;
        }

        isAttacking = true;

        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, attackRange))
        {
            if (hit.collider.CompareTag("Player"))
            {
                IDamage playerDamageable = hit.collider.GetComponent<IDamage>();
                if (playerDamageable != null)
                {
                    playerDamageable.takeDamage(attackDamage);
                }
                SpawnHitParticles(hit.point);
            }
        }
        yield return new WaitForSeconds(attackRate);
        isAttacking = false;

        yield break;
    }

    void SpawnHitParticles(Vector3 position)
    {
        GameObject hitParticles = Instantiate(hitParticlesPrefab, position, Quaternion.identity);

        Destroy(hitParticles, particleDuration);
    }
}
