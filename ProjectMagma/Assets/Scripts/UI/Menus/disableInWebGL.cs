using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A component with the sole purpose of disabling its game object in the WebGL build.
/// </summary>
public class disableInWebGL : MonoBehaviour
{
    void Start()
    {
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            gameObject.SetActive(false);
        }
    }
}
