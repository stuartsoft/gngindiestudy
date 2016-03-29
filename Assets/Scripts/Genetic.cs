using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class Genetic : MonoBehaviour {

    public float UnitHeight = 1;
    public GameObject UnitMarker;
    public GameObject edgeMarker;
    public List<GraphUnit> units;
    public List<GraphEdge> edges;

    GameObject graphGameObject;
    GameObject nodeGameObjects;

    public BuildMaze mazeBuilder;
    public Camera camera;

    Generation gen;
    Graph displayedGraph;

    Graph.Node nodeSearchA;
    Graph.Node nodeSearchB;

    void Start()
    {
        units = new List<GraphUnit>();
        edges = new List<GraphEdge>();

        nodeSearchA = null;
        nodeSearchB = null;

        graphGameObject = new GameObject("Graph");

        mazeBuilder.BuildTheMaze();

        List<Graph> initialGeneration = new List<Graph>();

        for (int i = 0; i < Generation.numEntitiesPerGeneration; i++)
        {
            Graph tempgraph = new Graph(500, mazeBuilder.floorlst, mazeBuilder.walllst);
            initialGeneration.Add(tempgraph);
        }

        displayedGraph = initialGeneration[initialGeneration.Count - 1];

        displayGraph(displayedGraph);

        gen = new Generation(1, initialGeneration, 0, 0, mazeBuilder.floorlst, mazeBuilder.walllst);
        //gen.getDecendents();//start the process!

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            gen.getDecendents();//start the process!
        }

        if (Input.GetMouseButtonDown(0))
        {
            Ray mouseRay = camera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(mouseRay, out hit, int.MaxValue))
            {
                GameObject tempGO = hit.transform.gameObject;
                if (tempGO.name.Contains("Node"))
                {
                    int id = int.Parse(tempGO.name.Split(' ')[1]);
                    if (nodeSearchA == null)
                    {
                        //clear all colors
                        foreach(KeyValuePair<int, Graph.Node> entry in displayedGraph.nodes)
                        {
                            GameObject.Find("Node: " + entry.Value.ID).GetComponent<Renderer>().material.color = Color.red;
                        }

                        nodeSearchA = displayedGraph.nodes[id];
                        tempGO.GetComponent<Renderer>().material.color = Color.green;
                    }
                    else
                    {
                        nodeSearchB = displayedGraph.nodes[id];
                        TestAStar();
                    }
                }
            }
        }
    }

    public void TestAStar()
    {
        int idA = nodeSearchA.ID;
        int idB = nodeSearchB.ID;
        List<Graph.Node> results = displayedGraph.AStar(idA, idB);
        string resultsStr = "";
        for(int i = 0; i < results.Count; i++)
        {
            GameObject.Find("Node: " + results[i].ID).GetComponent<Renderer>().material.color = Color.green;
            resultsStr += results[i].ID + " to ";
        }
        Debug.Log(resultsStr);
        nodeSearchA = null;
        nodeSearchB = null;
    }

    GraphUnit addNode(int id, Vector2 pos)
    {
        GraphUnit point = new GraphUnit(new Vector3(pos.x, UnitHeight, pos.y ),id, UnitMarker, nodeGameObjects);
        units.Add(point);
        return point;
    }

    void connectNodes(GraphUnit u1, GraphUnit u2)
    {
        GraphEdge temp = new GraphEdge(u1, u2);
        edges.Add(temp);
        u1.mEdges.Add(temp);
        u2.mEdges.Add(temp);
    }

    void displayGraph(Graph g)
    {
        //set up data structures
        units.Clear();
        edges.Clear();

        //clear game objects
        Destroy(nodeGameObjects);
        nodeGameObjects = new GameObject("Nodes");
        nodeGameObjects.transform.parent = graphGameObject.transform;

        //first create all node game objects
        foreach (KeyValuePair<int, Graph.Node> entry in g.nodes)
        {
            Vector2 setPos = new Vector2((entry.Value.pos.x - 0.5f), (entry.Value.pos.y - 0.5f));
            GraphUnit u = addNode(entry.Key, entry.Value.pos);
        }

        //connect all node game objects
        foreach (KeyValuePair<int, Graph.Node> entry in g.nodes)
        {
            //find the root unit
            GraphUnit rootUnit = units[0];
            for (int i = 0; i < units.Count; i++)
            {
                if (units[i].ID == entry.Value.ID)
                {
                    rootUnit = units[i];
                    break;
                }
            }

            for (int i = 0; i < entry.Value.connectedNodes.Count; i++)
            {
                GraphUnit connUnit = units[1];
                for (int j = 0; j < units.Count; j++)
                {
                    if (units[j].ID == entry.Value.connectedNodes[i].ID)
                    {
                        connUnit = units[j];
                        break;
                    }
                }
                connectNodes(rootUnit, connUnit);
            }
        }

        for (int i = 0; i < edges.Count; ++i)
        {
            edges[i].mDrawnLine.GetComponent<LineRenderer>().SetPosition(0, edges[i].mPointA.mCenter - new Vector3(0.0f, 0.1f, 0.0f));
            edges[i].mDrawnLine.GetComponent<LineRenderer>().SetPosition(1, edges[i].mPointB.mCenter - new Vector3(0.0f, 0.1f, 0.0f));
        }

    }

    public class GraphEdge
    {
        public GraphEdge(GraphUnit pointA, GraphUnit pointB)
        {
            mAge = 0;
            mPointA = pointA;
            mPointB = pointB;
            mDrawnLine = new GameObject();
            mDrawnLine.AddComponent<LineRenderer>();
            mDrawnLine.GetComponent<LineRenderer>().SetWidth(0.2f, 0.2f);
            mDrawnLine.GetComponent<LineRenderer>().material = new Material(Shader.Find("Transparent/Diffuse"));
            mDrawnLine.GetComponent<LineRenderer>().SetColors(Color.yellow, Color.yellow);
            mDrawnLine.transform.parent = pointA.mDrawnUnit.transform;
        }

        public GraphUnit mPointA, mPointB; //index of the point from the point ArrayList
        public int mAge; //track the age of the edge
        public GameObject mDrawnLine; //gameobject which generates the drawing of the line
    }

    public class GraphUnit
    {
        public GraphUnit(Vector3 center, int id, GameObject UnitMarker, GameObject Parent)
        {
            mCenter = center;
            mDrawnUnit = Object.Instantiate<GameObject>(UnitMarker); //THIS NEEDS TO BE CHANGED LATER
            mDrawnUnit.name = "Node: " + id.ToString();
            mDrawnUnit.transform.position = mCenter;
            mEdges = new List<GraphEdge>();
            mError = 0.0f;
            ID = id;
            mDrawnUnit.transform.parent = Parent.transform;
        }
        public List<GraphEdge> mEdges;
        public float mError;
        public Vector3 mCenter;
        public int ID;
        public GameObject mDrawnUnit; //gameobject which generates the drawing of the line
    }

}
