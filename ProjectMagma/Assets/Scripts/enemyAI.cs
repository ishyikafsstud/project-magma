using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class enemyAI : MonoBehaviour, IDamage
{
    [SerializeField] Renderer model;
    [SerializeField] NavMeshAgent agent;
    
    [SerializeField] int HP;
    [SerializeField] int speed;



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //this will not work till the GameManager is in the scene. in class we did it within the UI
        transform.position = Vector3.MoveTowards(transform.position, gameManager.instance.player.transform.position, speed * Time.deltaTime);
        agent.SetDestination(gameManager.instance.player.transform.position);
        
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

    IEnumerator flashRed()
    {
        model.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        model.material.color = Color.white;

    }
}
