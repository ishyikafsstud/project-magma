using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraController : MonoBehaviour
{
    [SerializeField] int sensitivity;
    [SerializeField] int lockVertMin;
    [SerializeField] int lockVertMax;
    [SerializeField] bool invertY;

    float xRot;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        saveSystem.MouseSensitivitySet += SetMouseSensitivity;
    }

    public void SetMouseSensitivity(float value)
    {
        sensitivity = (int)value;
    }

    // Update is called once per frame
    void Update()
    {
        float mouseY = Input.GetAxis("Mouse Y") * Time.deltaTime * sensitivity;
        float mouseX = Input.GetAxis("Mouse X") * Time.deltaTime * sensitivity;

        //Debug.Log("Mouse X: " + mouseX);
        //Debug.Log("Mouse Y: " + mouseY);

        if (invertY)
        {
            xRot += mouseY;
        }
        else
        {
            xRot -= mouseY;
        }
        // clamp xRot to the x axis

        xRot = Mathf.Clamp(xRot, lockVertMin, lockVertMax);
        // rotate on x axis using the camera rotation
        transform.localRotation = Quaternion.Euler(xRot, 0, 0);

        // rotate on y axis using the player rotation
        // The parent of the camera is camera handle, so get the parent of the parent to access player
        transform.parent.parent.Rotate(Vector3.up * mouseX);
    }
}