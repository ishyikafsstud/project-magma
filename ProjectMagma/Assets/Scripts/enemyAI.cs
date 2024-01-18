using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class enemyAI : MonoBehaviour, IDamage
{
    [SerializeField] Renderer model;
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Transform shootPos;

    [SerializeField] int HP;
    [Tooltip("For how long the enemy flashes red upon receiving damage.")]
    [SerializeField] float damageFlashLength;
    [SerializeField] int speed;

    [Header("Damage")]
    //[SerializeField] int shootDamage;
    [SerializeField] float shootRate;
    //[SerializeField] int shootDist;
    [SerializeField] GameObject bullet;

    [Header("Other")]
    [SerializeField] GameObject keyPrefab;

    bool isShooting;


    // Start is called before the first frame update
    void Start()
    {
        gameManager.instance.EnemyCount++;
    }

    // Update is called once per frame
    void Update()
    {
         ChasePlayer();
    }

    public void takeDamage(int amount)
    {
        HP -= amount;
        StartCoroutine(flashRed());
        
        if(HP <= 0)
        {
            die();
        }
    }

    private void die()
    {
        gameManager.instance.DecreaseEnemyCount();

        if (gameManager.instance.EnemyCount == 0)
        { 
            Instantiate(keyPrefab, transform.position, transform.rotation);
        }

        Destroy(gameObject);
    }

    void ChasePlayer()
    {
        transform.LookAt(gameManager.instance.player.transform.position);
        agent.SetDestination(gameManager.instance.player.transform.position);
        if(!isShooting)
        {
            StartCoroutine(Shoot());
        }
    }

    //this is going to change. this is for test feedback for the player.
    IEnumerator flashRed()
    {
        // Remember the old color
        Color oldColor = model.material.color;

        // Flash red for some time
        model.material.color = Color.red;
        yield return new WaitForSeconds(damageFlashLength);

        model.material.color = oldColor;
    }

    IEnumerator Shoot()
    {
        isShooting = true;

        Instantiate(bullet, shootPos.position, transform.rotation);

        yield return new WaitForSeconds(shootRate); // Unity Timer

        isShooting = false;
    }
}
