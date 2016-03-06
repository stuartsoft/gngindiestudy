using UnityEngine;
using System.Collections;

public class BuildMaze : MonoBehaviour {

    public double getWidth() {
        return totalWidth;
    }

    public double getHeight()
    {
        return totalHeight;
    }

    public TextAsset mazeText;
    public GameObject floorTile;
    public GameObject wallTile;



    string mazeStr;
    int i;
    int j;
    int total;

    double totalWidth;
    double totalHeight;

    // Use this for initialization
    void Start () {
        
        mazeStr = mazeText.ToString();
        i = 0;
        j = 0;
        total = 0;
        totalWidth = 0.0;
        totalHeight = 0.0;
        double workingWidth = 0.0;
        double workingHeight = 0.0;

        GameObject floors = new GameObject("floors");
        floors.transform.parent = transform;
        GameObject walls = new GameObject("walls");
        walls.transform.parent = transform;

        while (total < mazeStr.Length)
        {
            if (mazeStr[total] == ' ')
            {
                GameObject newTile = Instantiate<GameObject>(floorTile);
                newTile.transform.position = new Vector3(i * newTile.transform.localScale.x, 0, j * newTile.transform.localScale.y);
                newTile.transform.Rotate(new Vector3(90, 0, 0));
                newTile.transform.parent = floors.transform;

                //Debug.Log("dis ting." + i.ToString() + ", " + j.ToString());

            }
            else if (mazeStr[total] == '#')
            {
                GameObject newWall = Instantiate<GameObject>(wallTile);
                newWall.transform.position = new Vector3(i * newWall.transform.localScale.x, 0, j * newWall.transform.localScale.z);
                newWall.transform.parent = walls.transform  ;
            }
            else if (mazeStr[total] == '\n') //it's a new line
            {
                ++j;
                workingHeight += floorTile.GetComponent<Renderer>().bounds.size.y; //assume that floor and wall have the same dimensions
                i = -1;
            }
            ++i;
            if (j == 0) { workingWidth += floorTile.GetComponent<Renderer>().bounds.size.x; } //assume that floor and wall have the same dimensions
            ++total;
        }

        totalWidth = workingWidth;
        totalHeight = workingHeight + floorTile.GetComponent<Renderer>().bounds.size.y; //last line has no newline, so this extra width needs to be added in

    }

    

	// Update is called once per frame
	void Update () {
        
    }
}
