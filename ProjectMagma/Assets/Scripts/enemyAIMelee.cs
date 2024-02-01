using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemyAIMelee : enemyAI
{
    [Header("Melee")]
    [SerializeField] float particleDuration;
    [SerializeField] GameObject hitParticlesPrefab;


    protected override void Attack()
    {
        base.Attack();
    }

    protected override void AttackAnimationEvent()
    {
        Vector3 distanceToPlayerFromAttackOrigin = (gameManager.instance.player.transform.position - attackOrigin.transform.position);

        RaycastHit hit;
        int layerMask = (1 << 0) | (1 << 3);
        if (Physics.Raycast(attackOrigin.transform.position, distanceToPlayerFromAttackOrigin.normalized, out hit, attackRange, layerMask))
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
    }


    protected override void AttackAnimationEnd()
    {
        base.AttackAnimationEnd();
    }

    void SpawnHitParticles(Vector3 position)
    {
        GameObject hitParticles = Instantiate(hitParticlesPrefab, position, Quaternion.identity);

        Destroy(hitParticles, particleDuration);
    }
}
