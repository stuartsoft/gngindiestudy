using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Generation {

    List<Graph> offspring;
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

        //do any setup prior to running the full breeding sequence
        samplePointStart = new List<Vector2>();
        samplePointEnd = new List<Vector2>();
        for (int i = 0; i < numAStarPathChecks; i++)
        {
            samplePointStart.Add(randPosInMaze());
            samplePointEnd.Add(randPosInMaze());
        }
    }

    public List<Graph> getPredecessors()
    {
        return predecessors;
    }

    public List<Graph> getDecendents()
    {
        eval();
        crossover();
        onComplete();
        return offspring;//all grown up!
        //return new Generation(generationIndex + 1, entities, alpha, beta, floors, walls);
    }

    public void eval()//this is the only public function of the genetic algorithm, so that evals can be presented to the user
    {
        foreach (Graph graph in predecessors)
        {
            graph.generateAStarSatisfaction(samplePointStart, samplePointEnd);
        }
    }

    void crossover()
    {
        int graphPairsToSelect = 1;

        //sort graphs by score
        //selection sort
        int minIndex = 0;
        float minVal = 99999;
        
        //selection sort of scored predecessors
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

        //build new graphs until we have a new generation
        offspring = new List<Graph>();
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

                for (int j = 0; j < Graph.numNodes; j++)//fill this new graph with nodes from parentA and parentB
                {
                    //randomly select node from first graph
                    int rndIndexA = Random.Range(0, parentA.nodes.Count);
                    Graph.Node nodeA = new Graph.Node(Vector2.zero, 0);
                    int index = 0;
                    //retrieve the node at the random index of the dictionary
                    foreach(KeyValuePair<int, Graph.Node> entry in parentA.nodes)
                    {
                        if (index == rndIndexA)
                        {
                            nodeA = entry.Value;
                            return;
                        }
                        index++;
                    }

                    int rndIndexB = Random.Range(0, parentB.nodes.Count);
                    Graph.Node nodeB = new Graph.Node(Vector2.zero, 0);
                    index = 0;
                    //retrieve the node at the random index of the dictionary
                    foreach (KeyValuePair<int, Graph.Node> entry in parentB.nodes)
                    {
                        if (index == rndIndexB)
                        {
                            nodeB = entry.Value;
                            return;
                        }
                        index++;
                    }

                    //add the randomly selected nodes to our new graph
                    offspringGraph.addNewNode(new Vector2(nodeA.pos.x, nodeA.pos.y), offspringGraph.getMaxID()+1);
                    offspringGraph.addNewNode(new Vector2(nodeB.pos.x, nodeB.pos.y), offspringGraph.getMaxID()+1);

                    //remove these nodes from the graph pool in the parents now that they've been included here
                    parentA.removeNode(nodeA.ID);
                    parentB.removeNode(nodeB.ID);
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
