using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class GeneratePlanet : MonoBehaviour
{

    public int subdivision;
    public float radius;

    public void GenerateMesh()
    {
        GetComponent<MeshFilter>().mesh = OctahedronCreator.Create(subdivision, radius);
    }

    private void OnEnable()
    {
        GenerateMesh();
    }

}
