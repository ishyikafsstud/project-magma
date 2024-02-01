using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Makes the parent object always face the camera.
/// </summary>
public class faceCamera : MonoBehaviour
{
    void Update()
    {
        transform.rotation = Camera.main.transform.rotation;
    }
}
