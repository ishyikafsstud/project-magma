using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gameManager : MonoBehaviour
{
    //this may not work till the GameManager is in the scene. in class we did it through the ui
    //if you need to test->i added a UI prefab with a GameManager to add to your scene
    public static gameManager instance;

    public GameObject player;
    
    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
        player = GameObject.FindGameObjectWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
