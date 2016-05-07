/*
Stuart Bowman 2016

This class contains the graph nodes themselves, as well as helper functions for use by the generation class
to better facilitate evaluation and breeding. It also contains the class definition for the nodes themselves,
as well as the A* search implementation used by the genetic algorithm to check if a path can be traced between
two given points on the maze.

*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Graph
{
    public Dictionary<int, Node> nodes;
    float AStarPathSuccess = 0.0f;//fraction of samples that could be maped to nodes and completed with AStar
    float AStarAvgPathLength = 0.0f;//average path length of successful paths

    public float getAStarPathSuccess() { return AStarPathSuccess; }
    public float getAStarAvgPathLength() { return AStarAvgPathLength; }
    public float getCompositeScore() { return AStarPathSuccess; }//a weighted, composite score of all evaluated attributes of this graph

    public int newNodeStartingID = 0;

    public Graph(int startingID)
    {
        newNodeStartingID = startingID;
        nodes = new Dictionary<int, Node>();
    }

    public Graph(List<GameObject> floors, List<GameObject> walls, int initNumNodes = 20)
    {
        nodes = new Dictionary<int, Node>();
        for (int i = 0; i < initNumNodes; i++)
        {
            int rndTile = Random.Range(0, floors.Count);
            //get the 2d bounding area for any floor tile
            MeshCollider mc = floors[rndTile].GetComponent<MeshCollider>();
            Vector2 xyWorldSpaceBoundsBottomLeft = new Vector2(mc.bounds.center.x - mc.bounds.size.x/2, mc.bounds.center.z - mc.bounds.size.z/2);
            Vector2 rndPosInTile = new Vector2(Random.Range(0, mc.bounds.size.x), Random.Range(0, mc.bounds.size.z));
            Vector2 rndWorldPos = xyWorldSpaceBoundsBottomLeft + rndPosInTile;
            Node n = addNewNode(rndWorldPos);
        }

        connectAllNodes();
    }

    public Graph(Graph g)//deep copy constructor
    {
        nodes = new Dictionary<int, Node>();
        //deep copy        

        foreach (KeyValuePair<int, Node> entry in g.nodes)
        {
            //deep copy the node with the best score
            bool inherited = entry.Value.isNodeInheritedFromParent();
            Node currentNode = new Node(new Vector2(entry.Value.pos.x, entry.Value.pos.y), entry.Value.ID, inherited);
            //don't bother copying the A* and heuristic stuff for each node, it's just going to be re-crunched later
            nodes.Add(currentNode.ID, currentNode);
        }
        AStarPathSuccess = g.getAStarPathSuccess();
        AStarAvgPathLength = g.getAStarAvgPathLength();
    }

    public static float normDistRand(float mean, float stdDev)
    {
        float u1 = Random.Range(0, 1.0f);
        float u2 = Random.Range(0, 1.0f);
        float randStdNormal = Mathf.Sqrt(-2.0f * Mathf.Log(u1) * Mathf.Sin(2.0f * 3.14159f * u2));
        return mean + stdDev * randStdNormal;
    }

    public string getSummary()
    {
        string result = "";
        result += nodes.Count + "\tNodes\t(" + getNumOfNewlyAddedNodes() + " new)\n";
        result += getAStarPathSuccess() * 100 + "%\tA* Satisfaction\n";
        result += getAStarAvgPathLength() + "\tUnit avg. A* Path\n";
        return result;
    }

    int getNumOfNewlyAddedNodes()
    {
        int result = 0;
        foreach (KeyValuePair<int, Node> entry in nodes)
        {
            if (entry.Value.isNodeInheritedFromParent() == false)
                result++;
        }
        return result;
    }

    public void connectAllNodes()
    {
        foreach (KeyValuePair<int,Node> entry in nodes)
        {
            foreach (KeyValuePair<int, Node> entry2 in nodes)
            {
                if (entry.Value != entry2.Value)
                {
                    if (!Physics.Linecast(new Vector3(entry.Value.pos.x, 2, entry.Value.pos.y), new Vector3(entry2.Value.pos.x, 2, entry2.Value.pos.y)))
                    {
                        entry.Value.connectTo(entry2.Value);
                    }
                }
            }
        }
    }

    public int getFirstUnusedID()
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            if (!nodes.ContainsKey(i))
                return i;
        }
        return nodes.Count;
    }

    public int getMaxID()
    {
        int temp = 0;
        foreach (KeyValuePair<int, Node> entry in nodes)
        {
            if (entry.Value.ID > temp) temp = entry.Value.ID;
        }
        if (temp < newNodeStartingID)
            return newNodeStartingID;
        return temp;
    }

    public class Node
    {
        public int ID;
        public Vector2 pos;
        public List<Node> connectedNodes;
        bool inheritedFromParent;
        public Node Ancestor;//used in A*
        public float g, h;//public values for temporary use during searching and heuristic analysis
        public float f 
        {
            get {return g + h; }
            private set { }
        }

        public bool isNodeInheritedFromParent()
        {
            return inheritedFromParent;
        }

        public Node(Vector2 position, int id, bool inherited)
        {
            connectedNodes = new List<Node>();
            pos = position;
            ID = id;
            inheritedFromParent = inherited;
        }

        public void connectTo(Node node)
        {
            if (!connectedNodes.Contains(node))
            {
                connectedNodes.Add(node);
                if (!node.connectedNodes.Contains(this))
                {
                    node.connectedNodes.Add(this);
                }
            }
        }

        public void disconnectFrom(Node n)
        {
            for (int i = 0; i < connectedNodes.Count; i++)
            {
                if (connectedNodes[i] == n)
                    n.connectedNodes.Remove(this);
            }

            connectedNodes.Remove(n);
        }
    }

    public Node addNewNode(Vector2 pos)
    {
        int newID = this.getMaxID()+1;//get the new id from the maxID property
        Node tempNode = new Node(pos, newID, false);
        nodes.Add(newID, tempNode);
        return tempNode;
    }

    public Node addNewNode(Vector2 pos, int ID)
    {
        Node tempNode = new Node(pos, ID, false);
        nodes.Add(ID, tempNode);
        return tempNode;
    }

    public Node addNewNode(Vector2 pos, int ID, bool inherited)
    {
        Node tempNode = new Node(pos, ID, inherited);
        nodes.Add(ID, tempNode);
        return tempNode;
    }

    public void removeNode(int ID)
    {
        Node nodeToRemove = nodes[ID];
        foreach (Node n in nodeToRemove.connectedNodes)
        {
            //remove symmetrical connections
            n.connectedNodes.Remove(nodeToRemove);
        }
        nodes.Remove(ID);//delete the actual node
    }

    public void printAdjMatrix()
    {
        foreach (KeyValuePair<int, Node> entry in nodes)
        {
            string connNodes = "Node: " + entry.Value.ID + "\nConn: ";
            foreach (Node n2 in entry.Value.connectedNodes)
            {
                connNodes += n2.ID + ", ";
            }
            Debug.Log(connNodes);
        }
    }

    public List<Node> AStar(int startingNodeKey, int endingNodeKey)
    {
        List<Node> ClosedSet = new List<Node>();
        List<Node> OpenSet = new List<Node>();

        foreach(KeyValuePair<int, Node> entry in nodes)
        {
            entry.Value.g = 99999;//set all g values to infinity
            entry.Value.Ancestor = null;//set all node ancestors to null
        }
        nodes[startingNodeKey].g = 0;
        nodes[startingNodeKey].h = Vector2.Distance(nodes[startingNodeKey].pos, nodes[endingNodeKey].pos);

        OpenSet.Add(nodes[startingNodeKey]);

        while (OpenSet.Count > 0 )
        {
            float minscore = 99999;
            int minIndex = 0;
            for(int i = 0;i<OpenSet.Count;i++)
            {
                if (OpenSet[i].f < minscore)
                {
                    minscore = OpenSet[i].f;
                    minIndex = i;
                }
            }

            //deep copy the node with the best score
            Node currentNode = new Node(new Vector2(OpenSet[minIndex].pos.x, OpenSet[minIndex].pos.y), OpenSet[minIndex].ID, false);
            currentNode.g = OpenSet[minIndex].g;
            currentNode.h = OpenSet[minIndex].h;
            currentNode.Ancestor = OpenSet[minIndex].Ancestor;

            if (currentNode.ID == endingNodeKey)
            {
                //build the path list
                List<Node> fullPath = new List<Node>();
                Node temp = currentNode;
                while (temp != null)
                {
                    fullPath.Add(temp);
                    temp = temp.Ancestor;
                }
                return fullPath;
            }

            //remove this node from the open set
            OpenSet.RemoveAt(minIndex);
            ClosedSet.Add(currentNode);

            //go through the list of nodes that are connected to the current node
            foreach(Node n in nodes[currentNode.ID].connectedNodes)
            {
                bool isInClosedSet = false;
                //check if it's already in the closed set
                for (int i = 0; i < ClosedSet.Count; i++)
                {
                    if (ClosedSet[i].ID == n.ID)
                    {
                        isInClosedSet = true;
                        break;
                    }
                }
                if (isInClosedSet)
                    continue;

                float tenativeG = currentNode.g + Vector2.Distance(n.pos, currentNode.pos);
                bool isInOpenSet = false;
                for (int i = 0; i < OpenSet.Count; i++)
                {
                    if (OpenSet[i].ID == n.ID)
                        isInOpenSet = true;
                }
                if (!isInOpenSet)
                    OpenSet.Add(n);
                else if (tenativeG >= n.g)
                    continue;

                n.Ancestor = currentNode;
                n.g = tenativeG;
                n.h = Vector2.Distance(n.pos, nodes[endingNodeKey].pos);
            }

        }
        //didn't find a path
        return new List<Node>();

    }

    public void generateAStarSatisfaction(List<Vector2> startingPoint, List<Vector2> endingPoint)
    {
        int successfulPaths = 0;
        float avgPathLen = 0.0f;
        for (int i = 0; i < startingPoint.Count; i++)
        {
            Node startingNode = closestNodeToPoint(startingPoint[i]);
            if (startingNode == null)
                continue;//skip to next iteration if no starting node can be found
            Node endingNode = closestNodeToPoint(endingPoint[i]);
            if (endingNode == null)
                continue;//skip to next iteration if no ending node can be found

            List<Node> path = AStar(startingNode.ID, endingNode.ID);
            if (path.Count != 0)//if the path was successful
            {
                successfulPaths++;
                avgPathLen += path[path.Count - 1].g + 
                    Vector2.Distance(startingPoint[i], startingNode.pos) + Vector2.Distance(endingPoint[i], endingNode.pos);
            }
        }

        avgPathLen /= successfulPaths;

        //store results
        AStarAvgPathLength = avgPathLen;
        AStarPathSuccess = successfulPaths / (float)startingPoint.Count;

    }

    Node closestNodeToPoint(Vector2 point)
    {
        //find closest node to the given starting point
        List<Node> lineOfSightNodes = new List<Node>();

        foreach (KeyValuePair<int, Node> entry in nodes)
        {
            if (!Physics.Linecast(new Vector3(point.x, 2, point.y), new Vector3(entry.Value.pos.x, 2, entry.Value.pos.y)))
            {
                lineOfSightNodes.Add(entry.Value);
            }
        }

        float minDist = 999999;
        int minIndex = 0;

        if (lineOfSightNodes.Count == 0)
            return null;//no nodes are line of sight to this point

        for (int j = 0; j < lineOfSightNodes.Count; j++)
        {
            float dist = Vector2.Distance(point, lineOfSightNodes[j].pos);
            if (dist < minDist)
            {
                minDist = dist;
                minIndex = j;
            }
        }

        return lineOfSightNodes[minIndex];

    }

}
