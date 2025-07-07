using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitalPositioning : MonoBehaviour
{
    public Transform sun;

    private float angle;
    private float speed;
    private float majorAxis;
    private float minorAxis;
    private float rotationSpeed;

    public void Initialize(Transform sunTransform, float speed, float major, float minor, float angleStart, float rotSpeed)
    {
        sun = sunTransform;
        this.speed = speed;
        majorAxis = major;
        minorAxis = minor;
        angle = angleStart;
        rotationSpeed = rotSpeed;
    }

    void Update()
    {
        angle += speed * Time.deltaTime * 0.1f;

        float x = majorAxis * Mathf.Cos(angle);
        float z = minorAxis * Mathf.Sin(angle);

        transform.position = new Vector3(sun.position.x + x, transform.position.y, sun.position.z + z);
        transform.Rotate(Vector3.up * Time.deltaTime * rotationSpeed);

    }
}