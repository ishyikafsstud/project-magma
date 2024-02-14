using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rotator : MonoBehaviour
{
    [SerializeField] float rotationSpeed;
    [SerializeField] float bounceHeight;
    [SerializeField] float bounceFrequency;

    private Vector3 originalPosition;

    // Start is called before the first frame update
    void Start()
    {
        originalPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
        ApplyBounce();
    }
    void ApplyBounce()
    {
        float yOffset = Mathf.Sin(Time.time * bounceFrequency) * bounceHeight;
        transform.position = originalPosition + new Vector3(0f, yOffset, 0f);
    }
}
