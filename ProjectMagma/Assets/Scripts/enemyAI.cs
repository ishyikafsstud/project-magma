using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class enemyAI : MonoBehaviour, IDamage
{
    [SerializeField] Renderer model;
    [SerializeField] NavMeshAgent agent;
    

    
    [SerializeField] int HP;
    [Tooltip("For how long the enemy flashes red upon receiving damage.")]
    [SerializeField] float damageFlashLength;
    [SerializeField] int speed;
    [SerializeField] float AttackRadius;
    [SerializeField] float chaseTime;

    public bool canSeePlayer;
    public RaycastHit hitData;

    Ray enemyEyes;

    // Start is called before the first frame update
    void Start()
    {
        gameManager.instance.enemyCount++;
        enemyEyes = new Ray(transform.position, transform.forward);
    }

    // Update is called once per frame
    void Update()
    {
        Lurk();
        Debug.DrawRay(enemyEyes.origin, enemyEyes.direction * AttackRadius * 2, Color.green); 
    }

    public void takeDamage(int amount)
    {
        HP -= amount;
        StartCoroutine(flashRed());
        
        if(HP <= 0)
        {
            gameManager.instance.DecreaseEnemyCount();
            Destroy(gameObject);
        }
    }

    void Lurk()
    {
        if (Physics.Raycast(enemyEyes, out hitData, AttackRadius * 2) && hitData.collider.CompareTag("Player"))
        {
            canSeePlayer = true;
            ChasePlayer(); 
        }
        else
        {
            canSeePlayer= false;
        }
        
    }

    void ChasePlayer()
    {
        enemyEyes = new Ray(transform.position, transform.forward);
        StartCoroutine(ChaseTime());
        canSeePlayer = true;
        transform.LookAt(gameManager.instance.player.transform.position);
        agent.SetDestination(gameManager.instance.player.transform.position);

    }

    IEnumerator ChaseTime()
    {
        yield return new WaitForSeconds(chaseTime);
        enemyEyes = new Ray(transform.position, transform.forward);
        if (!Physics.Raycast(enemyEyes, out hitData, AttackRadius * 2))
        {
            canSeePlayer = false;
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

    
}
