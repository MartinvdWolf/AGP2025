using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEditor;
using UnityEngine;

[ExecuteAlways]
public class SpawnMeshtroids : MonoBehaviour
{
    public GameObject[] asteroidPrefabs;

    public int asteroidCount = 200;

    private float beltRadius;
    private float beltWidth;

    public bool randomRotation = true;

    public string beltParentName = "Meshtroids";

    private Transform beltParent;

    void Awake()
    {
        if (Application.isPlaying)
        {
            GenerateBelt();
        }
    }

    public void GenerateBelt()
    {
        GenerateMeshtroids();
        RecalculateBounds();
        AssignLODs();
    }


    public void GenerateMeshtroids()
    {
        beltParent = transform.Find(beltParentName);
        float planetRadius = beltParent.transform.parent.parent.GetComponent<Renderer>().bounds.extents.magnitude;

        beltRadius = planetRadius * 0.7f;
        beltWidth = planetRadius * 0.2f;

        float bandHalfWidth = beltWidth;
        float bandHeight = beltWidth * 0.5f;

        for (int i = 0; i < asteroidCount; i++)
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            //randomize distance
            float radiusOffset = Random.Range(-bandHalfWidth, bandHalfWidth);
            float radius = beltRadius + radiusOffset;
            float height = Random.Range(-bandHeight, bandHeight);

            //change to x,y,z pos
            float x = Mathf.Cos(angle) * radius;
            float y = height;
            float z = Mathf.Sin(angle) * radius;

            UnityEngine.Vector3 pos = transform.position + new UnityEngine.Vector3(x, y, z);
            UnityEngine.Quaternion rot;

            if (randomRotation)
            {
                rot = Random.rotation;
            }
            else
            {
                rot = UnityEngine.Quaternion.identity;
            }

            GameObject prefab = asteroidPrefabs[Random.Range(0, asteroidPrefabs.Length)];
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, beltParent);
            instance.transform.position = pos;
            instance.transform.rotation = rot;
        }
    }

    private void RecalculateBounds()
    {
        var lodGroup = GetComponent<LODGroup>();
        lodGroup.RecalculateBounds();
    }

    public void AssignLODs()
    {
        var lodGroup = GetComponent<LODGroup>();
        Renderer[] renderers = beltParent.GetComponentsInChildren<Renderer>();
        LOD[] lods = lodGroup.GetLODs();

        lods[0].renderers = renderers;
        lodGroup.SetLODs(lods);
    }
}
