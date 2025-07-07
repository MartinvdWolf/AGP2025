using UnityEngine;

public class FollowPlanet : MonoBehaviour
{
    public Transform target;
    public float offsetDistance;  // radius + surfaceOffset

    Transform cam;

    void Start() => cam = Camera.main.transform;

    void LateUpdate()
    {
        Vector3 dir = (cam.position - target.position).normalized;
        transform.position = target.position + dir * offsetDistance;
        transform.rotation = Quaternion.identity;
    }
}
