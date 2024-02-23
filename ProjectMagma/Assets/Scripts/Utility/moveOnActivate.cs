using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class moveOnActivate : MonoBehaviour, IActivate
{
    [SerializeField] bool activateOnStart = false;

    [Tooltip("How much to move by in each direction.")]
    [SerializeField] Vector3 moveByDistance;
    [Tooltip("How long the animation takes.")]
    [SerializeField] float timeToReachTarget = 1.0f;
    [Tooltip("If activated again, should it reverse its movement?")]
    [SerializeField] bool reverseOnReactivate = false;

    bool isMoving;
    bool isAtOriginPosition;
    Vector3 startPosition;
    Vector3 endPosition;
    float progress;

    void Start()
    {
        enabled = false;

        startPosition = transform.position;
        progress = 0.0f;
        isAtOriginPosition = true;

        if (activateOnStart)
            StartCoroutine(Activate());
    }

    void Update()
    {
        progress += Time.deltaTime / timeToReachTarget;
        transform.position = Vector3.Lerp(startPosition, endPosition, progress);

        if (progress >= 1.0f)
            enabled = false;
    }

    public IEnumerator Activate()
    {
        startPosition = transform.position;
        if (!reverseOnReactivate || isAtOriginPosition)
        {
            endPosition = startPosition + moveByDistance;
            isAtOriginPosition = false;
        }
        else
        {
            endPosition = startPosition - moveByDistance;
            isAtOriginPosition = true;
        }
        progress = 0.0f;
        enabled = true;

        yield return null;
    }
}
