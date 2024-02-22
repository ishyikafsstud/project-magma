using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class disableOnActivate : MonoBehaviour, IActivate
{
    [Tooltip("Delay in seconds before destroying.")]
    [SerializeField] float delay = 0.0f;

    public IEnumerator Activate()
    {
        if (delay > 0.0f)
            yield return new WaitForSeconds(delay);

        gameObject.SetActive(false);
    }
}
