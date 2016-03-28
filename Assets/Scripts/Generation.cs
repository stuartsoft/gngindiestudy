using System.Collections;
using System.Collections.Generic;

public class Generation {
    List<Graph> entities;
    List<Graph> predecessors;
    public static int numEntitiesPerGeneration = 10;
    int finalGeneration;
    int generationIndex;
    int alpha;
    float beta;

    public Generation(int genNum, List<Graph> predecessors, int alpha, float beta)
    {
        generationIndex = genNum;
        this.predecessors = predecessors;
        this.alpha = alpha;
        this.beta = beta;
        if (predecessors == null || predecessors.Count == 0)
        {
            throw new System.Exception("must provide predecessors");
        }
    }

    public List<Graph> getPredecessors()
    {
        return predecessors;
    }

    public Generation getDecendents()
    {
        onSetup();
        return new Generation(generationIndex + 1, entities, alpha, beta);
    }

    void onSetup()
    {
        //do any setup prior to running the full breeding sequence
        eval();
    }

    void eval()
    {

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
    
}
