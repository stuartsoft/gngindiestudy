using System.Collections;
using System.Collections.Generic;

public class Generation {
    List<Graph> entities;
    List<Graph> predecessors;
    int numEntitiesPerGeneration;
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
    }

    void onSetup()
    {

    }

    void eval()
    {

    }

    void selection()
    {

    }

    void crossover()
    {

    }

    void mutate()
    {

    }

    void onComplete()
    {

    }
}
