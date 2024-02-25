using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace ExtensionMethods
{
    static class AudioSourceExtension
    {
        public static void PlayClipAtPoint(AudioClip clip, Vector3 position, float volume = 1.0f, AudioMixerGroup mixerGroup = null)
        {
            if (clip == null)
                return;

            GameObject gameObject = new GameObject("One Shot SFX");
            gameObject.transform.position = position;

            // Add an AudioSource component and set it up
            AudioSource audioSource = (AudioSource)gameObject.AddComponent(typeof(AudioSource));
            if (mixerGroup != null)
                audioSource.outputAudioMixerGroup = mixerGroup;
            audioSource.clip = clip;
            audioSource.spatialBlend = 1f;
            audioSource.volume = volume;
            audioSource.Play();

            Object.Destroy(gameObject, clip.length);
        }
    }
}
