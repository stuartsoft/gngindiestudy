﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Generation {

    List<Graph> offspring;
    List<Graph> predecessors;
    List<Vector2> samplePointStart;
    List<Vector2> samplePointEnd;
    public static int numEntitiesPerGeneration = 10;//Constant for the number of graphs to build in each generation
    public static int numAStarPathChecks = 750;//number of random start and end pairs to generate and check during the evaluation phase
    public static int nodeGrowthRate = 25;

    int finalGeneration;
    int generationIndex;
    int alpha;
    float beta;
    bool hasBeenEvaluated;

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

        //do any setup prior to running the full breeding sequence
        samplePointStart = new List<Vector2>();
        samplePointEnd = new List<Vector2>();
        for (int i = 0; i < numAStarPathChecks; i++)
        {
            samplePointStart.Add(randPosInMaze());
            samplePointEnd.Add(randPosInMaze());
        }

        hasBeenEvaluated = false;
    }

    public List<Graph> getPredecessors()
    {
        return predecessors;
    }

    public List<Graph> getDecendents()
    {
        if (!hasBeenEvaluated)
            eval();
        crossover();
        onComplete();
        return offspring;//all grown up!
        //return new Generation(generationIndex + 1, entities, alpha, beta, floors, walls);
    }

    public void eval()//this is the only public function of the genetic algorithm, so that evals can be presented to the user
    {
        hasBeenEvaluated = true;
        foreach (Graph graph in predecessors)
        {
            graph.generateAStarSatisfaction(samplePointStart, samplePointEnd);
        }
    }

    void crossover()
    {
        int graphPairsToSelect = 2;//how many pairs of parents

        //sort graphs by score
        //selection sort
        int maxIndex = 0;
        float maxVal = 99999;
        
        //selection sort of scored predecessors
        for (int i = 0; i < predecessors.Count; i++)
        {
            maxVal = 0;
            maxIndex = i;
            for (int j = i; j < predecessors.Count; j++)
            {
                if (predecessors[j].getCompositeScore() > maxVal)
                {
                    maxIndex = j;
                    maxVal = predecessors[j].getCompositeScore();
                }
            }
            if (maxIndex != i)
            {
                //swap lowest element into position
                Graph swapthisout = new Graph(predecessors[i]);
                Graph minElement = new Graph(predecessors[maxIndex]);
                predecessors.RemoveAt(i);
                predecessors.Insert(i, minElement);
                predecessors.RemoveAt(maxIndex);
                predecessors.Insert(maxIndex, swapthisout);
            }
        }

        foreach(Graph g in predecessors)
        {
            g.connectAllNodes();//reconnect after sort
        }

        //build new graphs until we have a new generation
        offspring = new List<Graph>();
        int numNodesFromParents;//number of nodes to recieve from both parents total

        while (offspring.Count < Generation.numEntitiesPerGeneration)//do this until we have enough graphs for the next generation
        {
            for (int i = 0;i< graphPairsToSelect*2; i+=2)//loop through the number of desired pairs of graphs, starting with the best scoring
            {
                int newMaxID = predecessors[i].getMaxID();
                if (predecessors[i + 1].getMaxID() > newMaxID)
                    newMaxID = predecessors[i + 1].getMaxID();
                Graph offspringGraph = new Graph(newMaxID);

                Graph parentA = new Graph(predecessors[i]);//deep copy parents
                Graph parentB = new Graph(predecessors[i+1]);
                numNodesFromParents = parentA.nodes.Count;

                /*
                //remove nodes that have no adj nodes, we don't want these to be passed down
                foreach(KeyValuePair<int, Graph.Node> entry in predecessors[i].nodes)
                {
                    if (entry.Value.connectedNodes.Count == 0)//look up from predecessors since deep copy does not clone the connected node list
                    {
                        //remove the node from parentA
                        parentA.removeNode(entry.Value.ID);
                        //Debug.Log("Removed single node " + entry.Value.ID);
                    }
                }
                foreach (KeyValuePair<int, Graph.Node> entry in predecessors[i+1].nodes)
                {
                    if (entry.Value.connectedNodes.Count == 0)//look up from predecessors since deep copy does not clone the connected node list
                    {
                        //remove the node from parentA
                        parentB.removeNode(entry.Value.ID);
                        //Debug.Log("Removed single node " + entry.Value.ID);
                    }
                }
                */

                for (int j = 0; j < numNodesFromParents/2; j++)//fill this new graph with nodes from parentA and parentB
                {
                    //randomly select node from first graph
                    int rndIndexA = Random.Range(0, parentA.nodes.Count);
                    Graph.Node nodeA = new Graph.Node(Vector2.zero, 0, false);
                    int index = 0;
                    //retrieve the node at the random index of the dictionary
                    foreach(KeyValuePair<int, Graph.Node> entry in parentA.nodes)
                    {
                        if (index == rndIndexA)
                        {
                            nodeA = entry.Value;
                            break;
                        }
                        index++;
                    }

                    int rndIndexB = Random.Range(0, parentB.nodes.Count);
                    Graph.Node nodeB = new Graph.Node(Vector2.zero, 0, false);
                    index = 0;
                    //retrieve the node at the random index of the dictionary
                    foreach (KeyValuePair<int, Graph.Node> entry in parentB.nodes)
                    {
                        if (index == rndIndexB)
                        {
                            nodeB = entry.Value;
                            break;
                        }
                        index++;
                    }

                    //add the randomly selected nodes to our new graph
                    offspringGraph.addNewNode(new Vector2(nodeA.pos.x, nodeA.pos.y), offspringGraph.getMaxID()+1, true);
                    offspringGraph.addNewNode(new Vector2(nodeB.pos.x, nodeB.pos.y), offspringGraph.getMaxID()+1, true);

                    //remove these nodes from the graph pool in the parents now that they've been included here
                    parentA.removeNode(nodeA.ID);
                    parentB.removeNode(nodeB.ID);

                    //check that parentA & parentB both have enough nodes left to give, otherwise we can fill in the gaps with new randomly palced nodes
                    if (parentA.nodes.Count == 0 || parentB.nodes.Count == 0)
                        break;
                }

                while(offspringGraph.nodes.Count < (numNodesFromParents + nodeGrowthRate))//if we don't have enough nodes after breeding, fill in with some new randomly placed nodes
                {
                    //Debug.Log("Adding new random node");
                    int rndTile = Random.Range(0, floors.Count);
                    //get the 2d bounding area for any floor tile
                    MeshCollider mc = floors[rndTile].GetComponent<MeshCollider>();
                    Vector2 xyWorldSpaceBoundsBottomLeft = new Vector2(mc.bounds.center.x - mc.bounds.size.x / 2, mc.bounds.center.z - mc.bounds.size.z / 2);
                    Vector2 rndPosInTile = new Vector2(Random.Range(0, mc.bounds.size.x), Random.Range(0, mc.bounds.size.z));
                    Vector2 rndWorldPos = xyWorldSpaceBoundsBottomLeft + rndPosInTile;
                    Graph.Node n = offspringGraph.addNewNode(rndWorldPos);
                }
                
                offspringGraph.connectAllNodes();
                offspring.Add(offspringGraph);
                if (offspring.Count == Generation.numEntitiesPerGeneration)
                    break;//we're done, we have enough graphs!
            }
            //for loop through our top graph pairs and make some babies!
        }        

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

    public int getGenNum()
    {
        return generationIndex;
    }
    
}
