using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using Quaternion = UnityEngine.Quaternion;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class PopulateScene : MonoBehaviour
{
    private GameObject newPlanet;
    public GameObject[] planets;
    public GameObject sun;

    public GameObject asteroidBeltPrefab;

    public float asteroidOffset = 5f;

    private Vector3 defaultSizeBilltroids = new Vector3(7, 1, 1.5f);

    readonly float billtroidsXScalar = 0.02f;
    readonly float billtroidsZScalar = 0.02f;

    readonly float partroidsXScalar = 0.75f;

    float sunRadius = 0f;
    float sunBuffer = 0f;

    //orbit variables
    public float flatteningA = 5f;
    public float flatteningB = 15f;
    public float majorAxisMin = 5f;
    public float majorAxisMax = 15f;
    public float speedMin = 0.05f;
    public float speedMax = 0.2f;

    [Range(0, 10)]
    public int planetCount;

    public Material mat;


    private void Awake()
    {
        planets = new GameObject[planetCount];
        Populate();

    }

    void Populate()
    {
        //Instantiate the sun
        if (!sun)
        {
            sun = CreatePlanet(99);
        }
        for (int i = 0; i < planets.Length; i++){
            planets[i] = CreatePlanet(i);
            if (planets[i].tag == "gas"){
                CreateAsteroids(planets[i]);
            }
        }
    }

    void Regenerate()
    {
            foreach (var planet in planets)
            {
                if (planet != null)
                {
                    Destroy(planet);
                }
           }

        planets = new GameObject[planetCount];

        Populate();
    }

    GameObject CreatePlanet(int id)
    {
        float minDist = 0f;
        float maxDist = 0f;
        Vector2 dir2D;
        float dist2D;

        newPlanet = new GameObject("Planet" + id);
        newPlanet.AddComponent<MeshFilter>();
        newPlanet.AddComponent<MeshRenderer>();
        newPlanet.AddComponent<GeneratePlanet>().enabled = false;

        if (id == 99)
        {
            //create sun
            newPlanet.GetComponent<GeneratePlanet>().subdivision = 6;
            newPlanet.GetComponent<GeneratePlanet>().radius = 600;
            newPlanet.GetComponent<GeneratePlanet>().enabled = true;

            newPlanet.GetComponent<MeshRenderer>().material = mat;
            newPlanet.transform.position = new Vector3(0, 0, 0);

            newPlanet.AddComponent<Light>();
            Light sunLight = newPlanet.GetComponent<Light>();
            sunLight.type = UnityEngine.LightType.Point;
            sunLight.color = new Color(0.7f, 0.6f, 0.5f);
            sunLight.intensity = 4;
            sunLight.range = 10000;
            newPlanet.GetComponent<Renderer>().material = GetComponent<PlanetTypeConfiguration>().SetType(newPlanet, true);

            sunRadius = newPlanet.GetComponent<Renderer>().bounds.extents.x;
            sunBuffer = 200f;

        }
        else
        {
            newPlanet.GetComponent<GeneratePlanet>().subdivision = 6;
            newPlanet.GetComponent<GeneratePlanet>().radius = Random.Range(100, 300);
            newPlanet.GetComponent<GeneratePlanet>().enabled = true;

            minDist = sunRadius + sunBuffer;
            maxDist = 10000f;

            dir2D = Random.insideUnitCircle.normalized;
            dist2D = Random.Range(minDist, maxDist);

            newPlanet.GetComponent<MeshRenderer>().material = mat;
            newPlanet.transform.position = sun.transform.position + new Vector3(dir2D.x * dist2D, 0f, dir2D.y * dist2D);

            newPlanet.GetComponent<Renderer>().material = GetComponent<PlanetTypeConfiguration>().SetType(newPlanet);

            // Attach orbit script
            OrbitalPositioning orbit = newPlanet.AddComponent<OrbitalPositioning>();
            float majorAxis = dist2D;
            // Level of 'flattening' of the orbit, keep between 0.9 and 2 for circular enough movement
            float flattening = Random.Range(flatteningA, flatteningB);
            float minorAxis = majorAxis * flattening;
            float angleStart = Random.Range(0f, 360f);
            float orbitSpeed = Random.Range(speedMin, speedMax);
            float rotSpeed = Random.Range(3f, 15f);

            orbit.Initialize(sun.transform, orbitSpeed, majorAxis, minorAxis, angleStart, rotSpeed);
        }
        return newPlanet;
    }


    void CreateAsteroids(GameObject planet)
    {
        GameObject belt = Instantiate(asteroidBeltPrefab, planet.transform.position, planet.transform.rotation, planet.transform);
        belt.name = planet.name + "_Asteroids";

        float planetRadius = planet.GetComponent<Renderer>().bounds.extents.magnitude;

        float billtroidsX = planetRadius * billtroidsXScalar;
        float billtroidsZ = planetRadius * billtroidsZScalar;
        float partroidsX = planetRadius * partroidsXScalar;

        var billtroids = belt.transform.Find("Billtroids");
        var partroids = belt.GetComponentInChildren<ParticleSystem>();

        Vector3 billtroidsScale = Vector3.Scale(defaultSizeBilltroids, new Vector3(billtroidsX, 1, billtroidsZ));
        billtroids.transform.localScale = billtroidsScale;

        var pShape = partroids.shape;
        pShape.radius = planetRadius * partroidsXScalar;

        partroids.transform.localScale = new Vector3(1f, 1f, billtroidsScale.z);

    }
}


