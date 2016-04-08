using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Generation {
    List<Graph> entities;
    List<Graph> predecessors;
    List<Vector2> samplePointStart;
    List<Vector2> samplePointEnd;
    public static int numEntitiesPerGeneration = 5;
    int finalGeneration;
    int generationIndex;
    int alpha;
    float beta;

    List<GameObject> floors;
    List<GameObject> walls;

    public Generation(int genNum, List<Graph> predecessors, int alpha, float beta, List<GameObject> floors, List<GameObject> walls)
    {
        generationIndex = genNum;
        this.predecessors = predecessors;
        this.alpha = alpha;
        this.beta = beta;
        this.floors = floors;
        this.walls = walls;
        if (predecessors == null || predecessors.Count == 0)
        {
            throw new System.Exception("must provide predecessors");
        }
    }

    public List<Graph> getPredecessors()
    {
        return predecessors;
    }

    public void getDecendents()
    {
        onSetup();
        //return new Generation(generationIndex + 1, entities, alpha, beta, floors, walls);
    }

    void onSetup()
    {
        samplePointStart = new List<Vector2>();
        samplePointEnd = new List<Vector2>();
        for (int i = 0; i < 1000; i++)
        {
            samplePointStart.Add(randPosInMaze());
            samplePointEnd.Add(randPosInMaze());
        }
        //do any setup prior to running the full breeding sequence
        eval();
    }

    void eval()
    {
        foreach (Graph graph in predecessors)
        {
            graph.generateAStarSatisfaction(samplePointStart, samplePointEnd);
            Debug.Log("AStar Satisfaction: " + graph.getAStarPathSuccess());
        }
    //!!!
    }

    void crossover()
    {
    ///!!!
    }

    void selection()
    {

    }

    void mutate()
    {

    }

    void onComplete()
    {
    }

    Vector2 randPosInMaze()
    {
        int rndTile = Random.Range(0, floors.Count);
        //get the 2d bounding area for any floor tile
        MeshCollider mc = floors[rndTile].GetComponent<MeshCollider>();
        Vector2 xyWorldSpaceBoundsBottomLeft = new Vector2(mc.bounds.center.x - mc.bounds.size.x / 2, mc.bounds.center.z - mc.bounds.size.z / 2);
        Vector2 rndPosInTile = new Vector2(Random.Range(0, mc.bounds.size.x), Random.Range(0, mc.bounds.size.z));
        Vector2 rndWorldPos = xyWorldSpaceBoundsBottomLeft + rndPosInTile;
        return rndWorldPos;
    }
    
}
