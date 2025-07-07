using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GeneratePlanet))]
public class GeneratePlanetEditorButton : Editor
{ 
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GeneratePlanet planet = (GeneratePlanet)target;
        if (GUILayout.Button("Regenerate Mesh"))
        {
            planet.GenerateMesh();
        }
    }
}
