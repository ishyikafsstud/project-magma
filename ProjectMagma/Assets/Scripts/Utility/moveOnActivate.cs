using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class moveOnActivate : MonoBehaviour, IActivate
{
    [Tooltip("How much to move by in each direction.")]
    [SerializeField] Vector3 moveByDistance;
    [Tooltip("How long the animation takes.")]
    [SerializeField] float timeToReachTarget = 1.0f;

    bool isMoving;
    Vector3 startPosition;
    Vector3 endPosition;
    float progress;

    void Start()
    {
        enabled = false;

        startPosition = transform.position;
        endPosition = startPosition + moveByDistance;
        progress = 0.0f;
    }

    void Update()
    {
        progress += Time.deltaTime/timeToReachTarget;
        transform.position = Vector3.Lerp(startPosition, endPosition, progress);

        if (progress >= 1.0f)
            enabled = false;
    }

    public IEnumerator Activate()
    {
        startPosition = transform.position;
        endPosition = startPosition + moveByDistance;
        progress = 0.0f;
        enabled = true;

        yield return null;
    }
}
