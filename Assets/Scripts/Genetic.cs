using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Node
{
    public int ID;
    public Vector2 pos;
    public List<Node> connectedNodes;

    public Node(Vector2 position, int id)
    {
        connectedNodes = new List<Node>();
        pos = position;
        ID = id;
    }
}

public class Genetic : MonoBehaviour {

    public float UnitHeight = 1;
    public GameObject UnitMarker;
    public GameObject edgeMarker;
    public List<Unit> units;
    public List<Edge> edges;

    public Dictionary<int, Node> nodes;

    void Start()
    {
        //set up data structures
        units = new List<Unit>();
        edges = new List<Edge>();

        nodes = new Dictionary<int, Node>();

        string line;
        System.IO.StreamReader file = new System.IO.StreamReader("C:\\Users\\bowmanrs1\\IdeaProjects\\Waypoint\\output.txt");
        while((line = file.ReadLine()) != null)
        {
            string[] toks = line.Split(' ');
            if (toks[0] == "Node:")
            {
                int ID;
                int.TryParse(toks[1],out ID);
                float x;
                float.TryParse(toks[2], out x);
                float y;
                float.TryParse(toks[3], out y);
                Node n = new Node(new Vector2(x, y), ID);
                nodes.Add(ID, n);
                //Debug.Log("New node: " + ID);
            }
        }
        file.Close();

        System.IO.StreamReader file2 = new System.IO.StreamReader("C:\\Users\\bowmanrs1\\IdeaProjects\\Waypoint\\output.txt");
        while ((line = file2.ReadLine()) != null)
        {
            string[] toks = line.Split(' ');
            if (toks[0] == "Node:")
            {
                line = file2.ReadLine();
                string[] toks2 = line.Split(' ');
                int ID = int.Parse(toks[1]);
                if (toks2[0] == "Conn:")
                {
                    Node tempn;
                    nodes.TryGetValue(ID, out tempn);
                    for (int i = 1; i < toks2.Length; i++)
                    {
                        if (toks2[i] == "") break;
                        int connID;
                        int.TryParse(toks2[i], out connID);
                        Node connectedNode;
                        nodes.TryGetValue(connID, out connectedNode);
                        tempn.connectedNodes.Add(connectedNode);
                        //Debug.Log("ConnID: " + connID);
                    }
                }
            }
        }
        file.Close();

        //first create all node game objects
        foreach(KeyValuePair<int, Node> entry in nodes)
        {
            Vector2 setPos = new Vector2((float)GetComponent<BuildMaze>().getWidth() - (entry.Value.pos.x-0.5f), (float)GetComponent<BuildMaze>().getHeight() - (entry.Value.pos.y-0.5f));
            Unit u = addNode(entry.Key, entry.Value.pos);
        }

        //connect all node game objects
        foreach (KeyValuePair<int, Node> entry in nodes)
        {
            //find the root unit
            Unit rootUnit = units[0];
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
                Unit connUnit = units[1];
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

        //Unit u1 = addNode(0, new Vector2(0, 0));
        //Unit u2 = addNode(1, new Vector2(5, 5));
        //connectNodes(u1, u2);

        for (int i = 0; i < edges.Count; ++i)
        {
            edges[i].mDrawnLine.GetComponent<LineRenderer>().SetPosition(0, edges[i].mPointA.mCenter - new Vector3(0.0f, 0.1f, 0.0f));
            edges[i].mDrawnLine.GetComponent<LineRenderer>().SetPosition(1, edges[i].mPointB.mCenter - new Vector3(0.0f, 0.1f, 0.0f));
        }
    }

    public Unit addNode(int id, Vector2 pos)
    {
        Unit point = new Unit(new Vector3(pos.x*2, UnitHeight, pos.y*2 ),id, UnitMarker);
        units.Add(point);
        return point;
    }

    public void connectNodes(Unit u1, Unit u2)
    {
        Edge temp = new Edge(u1, u2);
        edges.Add(temp);
        u1.mEdges.Add(temp);
        u2.mEdges.Add(temp);

    }

}
