using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class destroyOnActivate : MonoBehaviour, IActivate
{
    [SerializeField] bool activateOnStart = false;

    [Tooltip("Delay in seconds before destroying.")]
    [SerializeField] float delay = 0.0f;

    void Start()
    {
        if (activateOnStart)
            StartCoroutine(Activate());
    }

    public IEnumerator Activate()
    {
        if (delay > 0.0f)
            yield return new WaitForSeconds(delay);

        Destroy(gameObject);
    }
}
