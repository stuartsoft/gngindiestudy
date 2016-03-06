using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Pair<T, U>
{
	public Pair()
	{
	}

	public Pair(T first, U second)
	{
		this.First = first;
		this.Second = second;
	}

	public T First { get; set; }
	public U Second { get; set; }
};



public class AStarMaze : MonoBehaviour {

	public GameObject MazeWithGNG;
	public Camera currentCamera;
	public float maxSpeed = 3;
	public float minWayPointDist = 0.5f;
	public float acceleration = 10.0f;
	GNG gng;

	//private info
	enum AgentState {Waiting, Planning, Seeking};
	AgentState currentState;
	Vector3 target;

		//A* info

	class AStarUnit
	{
		public AStarUnit()
		{
		}

		public AStarUnit(AStarUnit prev, int uInd, float f, float h, float g)
		{
			mF = f;
			mH = h;
			mG = g;
			mUnitIndex = uInd;
			mPrevious = prev;
		}

		public float mF;
		public float mH;
		public float mG;
		public int mUnitIndex;
		public AStarUnit mPrevious;
	};

	//			f  			g	   unit index
	SortedList<float, AStarUnit> frontier;
	List<AStarUnit> evaluated;
	List<int> path;
	int nextWaypointIndex;

	//float heuristicValue(int locIndex, Vector3 end)
	//{
	//	Vector3 pos = gng.units[locIndex].mCenter;
	//	return Vector3.Distance(pos, end) + frontier.Keys[frontier.IndexOfValue(locIndex)];
	//}

	//bool reachedGoal()
	//{
	//	Debug.Log(Vector3.Distance(new Vector3(transform.position.x, 0.0f, transform.position.z), new Vector3(target.x, 0.0f, target.z)));

	//	return ;
	//}

	int indexOfNearestUnitTo(Vector3 target)
	{
		float minDist = Mathf.Infinity;
		int minIndex = 0;
		for (int i = 0; i < gng.units.Count; ++i)
		{
			float currentDist = Vector3.Distance(gng.units[i].mCenter, target);
			if(currentDist < minDist)
			{
				minIndex = i;
				minDist = currentDist;
			}
		}
		return minIndex;
	}

	int getOtherUnitIndex(Unit u, Edge e)
	{
		for(int i=0; i<gng.units.Count; ++i)
		{
			if(gng.units[i] == ((e.mPointA == u) ? e.mPointB : e.mPointA))
			{
				return i;
			}
		}
		return 0; //this should never happen
	}

	// Use this for initialization
	void Start () {
		currentState = AgentState.Waiting;
		gng = MazeWithGNG.GetComponent<GNG>();
		frontier = new SortedList<float, AStarUnit>();
		evaluated = new List<AStarUnit>();
		path = new List<int>();
	}
	
	// Update is called once per frame
	void Update () {

		

		//if there is a click, find where that click landed and store that. Change the state to planning
		if(Input.GetMouseButtonDown(0))
		{
			//assign to target
			Ray r = currentCamera.ScreenPointToRay(Input.mousePosition);
			RaycastHit h = new RaycastHit();
			if (Physics.Raycast(r, out h, Mathf.Infinity) && h.transform.gameObject.CompareTag("floor"))
			{
				target = h.point;
				currentState = AgentState.Planning;
			}
		}

		//if the current state is planning, do so. (THIS IS WHERE A* HAPPENS)
		//Upon the completion of planning, begin searching
		if(currentState == AgentState.Planning && gng.completed)
		{
			//clear everything being used
			frontier.Clear();
			evaluated.Clear();
			path.Clear();

			//get start point
			int start = indexOfNearestUnitTo(transform.position);
			//get end point
			int end = indexOfNearestUnitTo(target);

			//run A* between them, recording a list of indices which the agent will take to get there.
			bool succeeded = false;
			float firstH = Vector3.Distance(gng.units[start].mCenter, gng.units[end].mCenter);
			frontier.Add(firstH + 0.0f, new AStarUnit(null, start, firstH + 0.0f, firstH, 0.0f));
			while(frontier.Count > 0 && succeeded == false)
			{
				AStarUnit current = frontier[frontier.Keys[0]];

				if(current.mUnitIndex == end)
				{
					//WE DONE
					//reconstruct the path here!!!
					while(current.mPrevious != null)
					{
						path.Add(current.mUnitIndex);
						current = current.mPrevious;
					}
					path.Add(start);
					nextWaypointIndex = path.Count-1;
					succeeded = true;
					continue; //leave the loop.
				}

				//Debug.Log(frontier.Count);

				evaluated.Add(current);
				frontier.Remove(current.mF);
				//add 
				for (int i = 0; i < gng.units[current.mUnitIndex].mEdges.Count; ++i)
				{
					int newNeighborUnitIndex = getOtherUnitIndex(gng.units[current.mUnitIndex], gng.units[current.mUnitIndex].mEdges[i]);
					bool cont = false;
					for (int j = 0; j < evaluated.Count; ++j)
					{
						if (newNeighborUnitIndex == evaluated[j].mUnitIndex)
						{
							cont = true;
						}
					}
					if(cont) continue;
					float g = current.mG + Vector3.Distance(gng.units[current.mUnitIndex].mEdges[i].mPointA.mCenter, gng.units[current.mUnitIndex].mEdges[i].mPointB.mCenter);
					float h = Vector3.Distance(gng.units[current.mUnitIndex].mCenter, gng.units[end].mCenter);
					float f = h + g;
					AStarUnit newNeighbor = new AStarUnit(current, newNeighborUnitIndex, f, h, g);
					if(!frontier.ContainsValue(newNeighbor))
					{
						frontier.Add(f, newNeighbor);
					}
					else if (frontier[f].mG <= g)
					{
						continue;
					}
				}
			}

			if(succeeded)
			{
				Debug.Log("Path found of length " + path.Count.ToString());
				currentState = AgentState.Seeking;
			}
			else
			{
				Debug.Log("A* failed to find a path to the target.");
				currentState = AgentState.Waiting;
			}

			

		}

		//if the current state is seeking, do so.
		//when seeking is done, return to the waiting state
		if(currentState == AgentState.Seeking)
		{
			//work through the list of indices one by one until none remain
			//have a max speed.
			//if (GetComponent<Rigidbody>().velocity.magnitude > maxSpeed)
			//{
			//	GetComponent<Rigidbody>().velocity.Normalize();
			//	GetComponent<Rigidbody>().velocity *= maxSpeed;
			//}

			if (nextWaypointIndex != -1)
			{
				//Debug.Log(nextWaypointIndex);
				Vector3 f = gng.units[path[nextWaypointIndex]].mCenter - transform.position;
				f.y = 0.0f;
				GetComponent<Rigidbody>().AddForce(Vector3.Normalize(f) * acceleration);
				//Debug.Log(Vector3.Normalize(gng.units[path[nextWaypointIndex]].mCenter - transform.position) * 30);
			}
			else
			{
				Vector3 f = target - transform.position;
				f.y = 0.0f;
				GetComponent<Rigidbody>().AddForce((f).normalized * acceleration);
			}

			

			if (nextWaypointIndex != -1 && Vector3.Distance(new Vector3(gng.units[path[nextWaypointIndex]].mCenter.x, 0.0f, gng.units[path[nextWaypointIndex]].mCenter.z), new Vector3(transform.position.x, 0.0f, transform.position.z)) < minWayPointDist)
			{
				path.RemoveAt(path.Count - 1);
				if(path.Count > 0)
				{
					nextWaypointIndex = path.Count - 1;
				}
				else
				{
					nextWaypointIndex = -1;
				}
			}

			//Debug.Log(Vector3.Distance(new Vector3(transform.position.x, 0.0f, transform.position.z), new Vector3(target.x, 0.0f, target.z)));
			//if (Vector3.Distance(new Vector3(transform.position.x, 0.0f, transform.position.z), new Vector3(target.x, 0.0f, target.z)) < minWayPointDist)
			//{
			//	currentState = AgentState.Waiting;
			//	Debug.Log("DUMB");
			//}
		}
	}
}
