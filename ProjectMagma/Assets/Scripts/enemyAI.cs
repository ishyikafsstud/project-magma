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
   
    // Start is called before the first frame update
    void Start()
    {
        gameManager.instance.enemyCount++;
        
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
            gameManager.instance.DecreaseEnemyCount();
            Destroy(gameObject);
        }
    }

    void ChasePlayer()
    {
        
        transform.LookAt(gameManager.instance.player.transform.position);
        agent.SetDestination(gameManager.instance.player.transform.position);

    }

    void CheckIfLastEnemy()
    {
        if(gameManager.instance.enemyCount <= 1)
        {
            gameManager.instance.LastEnemyDefeated();
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
