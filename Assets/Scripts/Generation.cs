using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Generation {

    List<Graph> entities;
    List<Graph> predecessors;
    List<Vector2> samplePointStart;
    List<Vector2> samplePointEnd;
    public static int numEntitiesPerGeneration = 5;//Constant for the number of graphs to build in each generation
    public static int numAStarPathChecks = 1000;//number of random start and end pairs to generate and check during the evaluation phase

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
        eval();
        crossover();
        //return new Generation(generationIndex + 1, entities, alpha, beta, floors, walls);
    }

    void onSetup()
    {
        samplePointStart = new List<Vector2>();
        samplePointEnd = new List<Vector2>();
        for (int i = 0; i < numAStarPathChecks; i++)
        {
            samplePointStart.Add(randPosInMaze());
            samplePointEnd.Add(randPosInMaze());
        }
        //do any setup prior to running the full breeding sequence
    }

    void eval()
    {
        foreach (Graph graph in predecessors)
        {
            graph.generateAStarSatisfaction(samplePointStart, samplePointEnd);
        }
    }

    void crossover()
    {
        int graphPairsToSelect = 1;

        entities = new List<Graph>();

        //sort graphs by score
        //selection sort
        int minIndex = 0;
        float minVal = 99999;

        for (int i = 0; i < predecessors.Count; i++)
        {
            Debug.Log(predecessors[i].getCompositeScore());
        }

        Debug.Log("------------------");

        for (int i = 0; i < predecessors.Count; i++)
        {
            minVal = 99999;
            minIndex = i;
            for (int j = i; j < predecessors.Count; j++)
            {
                if (predecessors[j].getCompositeScore() < minVal)
                {
                    minIndex = j;
                    minVal = predecessors[j].getCompositeScore();
                }
            }
            if (minIndex != i)
            {
                //swap lowest element into position
                Graph swapthisout = new Graph(predecessors[i]);
                Graph minElement = new Graph(predecessors[minIndex]);
                predecessors.RemoveAt(i);
                predecessors.Insert(i, minElement);
                predecessors.RemoveAt(minIndex);
                predecessors.Insert(minIndex, swapthisout);
            }
        }

        //to do FIX BUG CAUSING LAST GRAPH TO SOMETIMES BE OUT OF THE CORRECT ORDER

        for (int i = 0; i < predecessors.Count; i++)
        {
            Debug.Log(predecessors[i].getCompositeScore());
        }
        
        //build new graphs until we have a new generation
        //while (entities.Count < numEntitiesPerGeneration)
        //{
            //for loop through our top graph pairs and make some babies!
        //}        

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
