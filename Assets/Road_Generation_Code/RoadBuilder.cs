using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;

//Literally just copied from my last assignment

public class RoadBuilder : MonoBehaviour
{
    [Header("Road")]
    [SerializeField] int roadVerticies;
    [SerializeField] float roadWidth;
    [SerializeField] SplineContainer roadSpline;
    [SerializeField] MeshFilter roadMeshFilter;
    [SerializeField] MeshRenderer roadRenderer;

    private void Start()
    {
        GenerateRoad();
    }

    public SplineContainer GetRoadSpline()
    {
        return roadSpline;
    }

    private void GenerateRoad()
    {
        //set up the array of verticies for the road
        var verticies = new Vector3[roadVerticies * 2];

        //create array of uvs
        Vector2[] uv = new Vector2[verticies.Length];

        //set up the list of triangles defined by three integers each
        List<int> triangles = new List<int>();

        //place the verticies along the path of the road
        for (int i = 0; i < roadVerticies; i++)
        {
            float dist = ((float)i / roadVerticies);

            //position to place vertex and direction from road to place it
            Vector3 position;
            Vector3 vertexDirection;

            //Get the position
            position = roadSpline.EvaluatePosition(dist);

            //Get the line that is perpendicular to the tangent (aproximately perpendicular to the road)
            vertexDirection = Vector3.Cross(roadSpline.EvaluateTangent(dist), roadSpline.EvaluateUpVector(dist)).normalized;

            //Place the verticies
            verticies[2 * i] = position + vertexDirection * -roadWidth;
            verticies[2 * i + 1] = position + vertexDirection * roadWidth;
        }

        //builds the UVs of the road
        for (int i = 0; i < verticies.Length; i++)
        {
            uv[i] = new Vector2(verticies[i].x, verticies[i].z);
        }

        int t = 0;
        //Build triangles
        while (t < verticies.Length - 2)
        {
            //two trianges are needed for a quad
            //tri 1
            triangles.Add(t);
            triangles.Add(t + 1);
            triangles.Add(t + 2);

            //tri 2
            triangles.Add(t + 1);
            triangles.Add(t + 3);
            triangles.Add(t + 2);

            t += 2;
        }

        if (roadSpline.Spline.Closed)
        {
            triangles.Add(verticies.Length - 2);
            triangles.Add(verticies.Length - 1);
            triangles.Add(0);

            triangles.Add(verticies.Length - 1);
            triangles.Add(1);
            triangles.Add(0);
        }

        Mesh roadMesh = new Mesh
        {
            vertices = verticies,
            triangles = triangles.ToArray(),
            uv = uv
        };

        //Recalculate incase of mistakes
        roadMesh.RecalculateNormals();
        roadMesh.RecalculateBounds();
        roadMesh.RecalculateTangents();

        roadMeshFilter.mesh = roadMesh;

        roadRenderer.UpdateGIMaterials();
        //Add collision
        var collider = gameObject.AddComponent<MeshCollider>();

        collider.sharedMesh = roadMesh;
    }
}