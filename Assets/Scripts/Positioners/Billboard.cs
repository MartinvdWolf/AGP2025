using UnityEngine;

public class Billboard : MonoBehaviour
{
    public Transform planet;
    public float surfaceOffset;

    private Transform camTransform;
    private Renderer planetRenderer;

    void Start()
    {
        planet = transform.parent.parent;
        camTransform = Camera.main.transform;
        planetRenderer = planet.GetComponent<Renderer>();
    }

    void LateUpdate()
    {
        Vector3 dir = (camTransform.position - planet.position).normalized;

        // Planet offset
        float radius = planetRenderer.bounds.extents.x;
        transform.position = planet.position + dir * (radius + surfaceOffset);

        // rotation fix
        Quaternion lookRot = Quaternion.LookRotation(dir, Vector3.up);
        Vector3 angles = lookRot.eulerAngles;

        // flip the billboard on the x axis
        transform.rotation = Quaternion.Euler(90f, angles.y, angles.z);
    }
}