﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Node : ScriptableObject
{
    public Vector3 vec;
    public Transform objTransform;
    public GameObject obj;
    public Color clr;
    public float LeastCost = Mathf.Infinity;// the least access time
    public Node MostAccessPOI = null;// the most accessible POI
    public int index;// index for calculating the shortest path easlier
    public string stop_id; // stop_id from raw data file Value[0]
    public List<string> visited = new List<string>();


    [SerializeField]
    private List<string> neighborNames;
    public List<string> NeighborNames
    {
        get
        {
            if (neighborNames == null)
            {
                neighborNames = new List<string>();
            }

            return neighborNames;
        }
    }


    [SerializeField]
    private List<Node> neighbors;
    public List<Node> Neighbors
    {
        get
        {
            if (neighbors == null)
            {
                neighbors = new List<Node>();
            }
        
            return neighbors;
        }
    }

    [SerializeField]
    private List<float> weights;
    public List<float> Weights
    {
        get
        {
            if (weights == null)
            {
                weights = new List<float>();
            }

            return weights;
        }
    }

    public static Node Create(string name, Vector3 pos)
    {
        Node node = CreateInstance<Node>();
        string path = string.Format("Assets/{0}.asset", name);
        AssetDatabase.CreateAsset(node, path);

        return node;
    }

    public static T Create<T>(string name, Vector3 pos)
    where T : Node
    {
        T node = CreateInstance<T>();
        node.name = name;
        node.vec = pos;
        return node;
    }
}
