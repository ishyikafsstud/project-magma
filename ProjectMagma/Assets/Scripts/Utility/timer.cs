using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class timer : MonoBehaviour
{
    [SerializeField] List<GameObject> activatedObjects = new List<GameObject>();
    [SerializeField] bool realTime;

    float timeLeft;
    float lastSetTime;

    public void StartTimer(float time)
    {
        lastSetTime = time;
        ResetTimer();
    }

    public void ResetTimer()
    {
        timeLeft = lastSetTime;
        enabled = true;
    }

    void Update()
    {
        timeLeft -= !realTime ? Time.deltaTime : Time.unscaledDeltaTime;
        if (timeLeft < 0.0f)
            TimerEnded();
    }
    
    void TimerEnded()
    {
        // Activate all associated objects
        foreach (GameObject obj in activatedObjects)
            StartCoroutine(obj.GetComponent<IActivate>()?.Activate());

        enabled = false;
    }
}
