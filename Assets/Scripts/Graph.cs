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
            Node n = addNewNode(new Vector2(0,0));
            n.connectTo(nodes[0]);
        }

        removeNode(5);

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

}
