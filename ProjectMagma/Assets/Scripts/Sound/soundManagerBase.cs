using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class soundManagerBase : MonoBehaviour
{
    protected void PlaySound(AudioSource source, float minPitch = 0.9f, float maxPitch = 1.1f)
    {
        if (source == null || source.clip == null)
            return;

        source.pitch = Random.Range(minPitch, maxPitch);
        source.Play();
    }
}
