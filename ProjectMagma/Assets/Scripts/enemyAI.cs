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
        
    }

    // Update is called once per frame
    void Update()
    {
        //this will not work till the GameManager is in the scene. in class we did it within the UI 
        agent.SetDestination(gameManager.instance.player.transform.position);
        
        //float distanceToPlayer = Vector3.Distance(transform.position, gameManager.instance.player.transform.position);
    }

    public void takeDamage(int amount)
    {
        HP -= amount;
        StartCoroutine(flashRed());
        
        if(HP <= 0)
        {
            Destroy(gameObject);
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
