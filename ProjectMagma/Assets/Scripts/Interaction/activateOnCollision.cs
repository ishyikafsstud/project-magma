using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class activateOnCollision : MonoBehaviour
{
    [Tooltip("A list of game objects that will be activated by this trigger.")]
    [SerializeField] List<GameObject> activatedObjects = new List<GameObject>();

    [SerializeField] bool activateOnlyOnce = true;

    bool hasBeenActivated;

    public void OnTriggerEnter(Collider other)
    {
        // If can be activated only once and has not been activated OR can be activated many times
        if ((activateOnlyOnce && !hasBeenActivated) || !activateOnlyOnce)
        {
            hasBeenActivated = true;

            // Activate all associated objects
            foreach (GameObject obj in activatedObjects)
            {
                foreach (IActivate script in obj.GetComponents<IActivate>())
                {
                    StartCoroutine(script.Activate());
                }
            }
        }
    }
}
