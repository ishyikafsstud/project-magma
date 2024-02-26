using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class disableOnTimer : MonoBehaviour
{
    public float timeToDisable = 5f; // Set the time after which the object should be disabled

    private float timer;

    void Start()
    {
        timer = timeToDisable;
    }

    void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            DisableObject();
        }
    }

    void DisableObject()
    {
        gameObject.SetActive(false);
    }
}
