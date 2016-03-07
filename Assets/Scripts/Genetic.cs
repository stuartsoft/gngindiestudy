using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Genetic : MonoBehaviour {

    public float UnitHeight = 1;
    public GameObject UnitMarker;
    public GameObject edgeMarker;
    public List<Unit> units;
    public List<Edge> edges;

    void Start()
    {
        //set up data structures
        units = new List<Unit>();
        edges = new List<Edge>();

        //STEP 0
        Unit firstPoint = new Unit(new Vector3(0, UnitHeight, 0), UnitMarker);
        Unit secondPoint = new Unit(new Vector3(5*2, UnitHeight, 5*2), UnitMarker);
        units.Add(firstPoint);
        units.Add(secondPoint);
        Edge temp = new Edge(firstPoint, secondPoint);
        edges.Add(temp);
        firstPoint.mEdges.Add(temp);
        secondPoint.mEdges.Add(temp);

        for (int i = 0; i < edges.Count; ++i)
        {
            edges[i].mDrawnLine.GetComponent<LineRenderer>().SetPosition(0, edges[i].mPointA.mCenter - new Vector3(0.0f, 0.1f, 0.0f));
            edges[i].mDrawnLine.GetComponent<LineRenderer>().SetPosition(1, edges[i].mPointB.mCenter - new Vector3(0.0f, 0.1f, 0.0f));
        }
    }

}
