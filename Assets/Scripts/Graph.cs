using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Graph
{
    public Dictionary<int, Node> nodes;

    public Graph(int initialNodes, List<GameObject> floors, List<GameObject> walls)
    {
        nodes = new Dictionary<int, Node>();
        for (int i = 0; i < initialNodes; i++)
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

    void connectAllNodes()
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

    public int maxID
    {
        get
        {
            int temp = 0;
            foreach (KeyValuePair<int, Node> entry in nodes)
            {
                if (entry.Value.ID > temp) temp = entry.Value.ID;
            }
            return temp;
        }
    }

    public class Node
    {
        public int ID;
        public Vector2 pos;
        public List<Node> connectedNodes;
        public float g, h;//public values for temporary use during searching and heuristic analysis
        public float f 
        {
            get {return g + h; }
            private set { }
        }

        public Node(Vector2 position, int id)
        {
            connectedNodes = new List<Node>();
            pos = position;
            ID = id;
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
        int newID = getFirstUnusedID();
        Node tempNode = new Node(pos, newID);
        nodes.Add(newID, tempNode);
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

    public List<Node> AStar(int startingNodeKey, int endingNodeKey, List<Node> pathHistory)
    {
        //if this is the very first iteration
        if (pathHistory == null) {
            pathHistory = new List<Node>();
            pathHistory.Add(nodes[startingNodeKey]);
        }

        Node currentNode = pathHistory[pathHistory.Count - 1];

        List<Node> adjNodes = pathHistory[pathHistory.Count - 1].connectedNodes;
        //remove adjacent nodes that have already been evaluated by this path
        for (int i = 0; i < adjNodes.Count; i++)
        {
            for (int j = 0; j < pathHistory.Count; j++)
            {
                if (adjNodes[i].ID == pathHistory[j].ID)
                {
                    adjNodes.RemoveAt(i);
                    i--;//go back because we just removed an index
                    break;
                }
            }
        }

        if (adjNodes.Count == 0  && pathHistory[pathHistory.Count-1].ID != endingNodeKey)
        {
            return new List<Node>();//no more adj node paths to explore from point.
            //Return empty list because this path doesn't work
        }

        //score adjacent nodes
        foreach (Node n in adjNodes)
        {
            n.g = currentNode.g + Vector2.Distance(currentNode.pos, n.pos);
            n.h = Vector2.Distance(n.pos, nodes[endingNodeKey].pos);
        }
        //sort adj nodes

        int firstUnsortedIndex = 0;
        for (int i = 0; i < adjNodes.Count; i++)
        {
            float minVal = 99999;
            int minIndex = 0;
            for (int j = firstUnsortedIndex; j < adjNodes.Count; j++)
            {
                if (adjNodes[j].f < minVal)
                {
                    minVal = adjNodes[j].f;
                    minIndex = j;
                }
            }
            //swap the lowest value item to the first unsorted position
            Node tempNode = adjNodes[firstUnsortedIndex];
            adjNodes[firstUnsortedIndex] = adjNodes[minIndex];
            adjNodes[minIndex] = tempNode;

            firstUnsortedIndex++;
        }

        //actually begin evaluating adjacent positions
        foreach(Node n in adjNodes)
        {
            List<Node> nextPathHistory = pathHistory;
            nextPathHistory.Add(n);

            //We have reached the goal, end recursion
            if (nextPathHistory[nextPathHistory.Count - 1].ID == endingNodeKey)
                return pathHistory;
            //make the recursive call
            List<Node> result = AStar(startingNodeKey, endingNodeKey, nextPathHistory);
            if (result != null && result.Count > 0)
                return result;//we have a valid result, pass it up the recursive chain
        }

        return new List<Node>();//no solution found
    }

}
