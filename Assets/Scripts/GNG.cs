using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Edge
{
    public Edge(Unit pointA, Unit pointB)
    {
        mAge = 0;
        mPointA = pointA;
        mPointB = pointB;
        mDrawnLine = new GameObject();
        mDrawnLine.AddComponent<LineRenderer>();
        mDrawnLine.GetComponent<LineRenderer>().SetWidth(0.2f, 0.2f);
        mDrawnLine.GetComponent<LineRenderer>().material = new Material(Shader.Find("Transparent/Diffuse"));
        mDrawnLine.GetComponent<LineRenderer>().SetColors(Color.yellow, Color.yellow);
    }

    public Unit mPointA, mPointB; //index of the point from the point ArrayList
    public int mAge; //track the age of the edge
    public GameObject mDrawnLine; //gameobject which generates the drawing of the line
}

public class Unit
{
    public Unit(float x, float y, float z, GameObject UnitMarker)
    {
        mCenter = new Vector3(x, y, z);
        mDrawnUnit = Object.Instantiate<GameObject>(UnitMarker); //THIS NEEDS TO BE CHANGED LATER
        mDrawnUnit.transform.position = mCenter;
        mEdges = new List<Edge>();
        mError = 0.0f; 
    }
    public Unit(Vector3 center, GameObject UnitMarker)
    {
        mCenter = center;
        mDrawnUnit = Object.Instantiate<GameObject>(UnitMarker); //THIS NEEDS TO BE CHANGED LATER
        mDrawnUnit.transform.position = mCenter;
        mEdges = new List<Edge>();
        mError = 0.0f;
    }
    public Unit(Vector3 center, int id, GameObject UnitMarker)
    {
        mCenter = center;
        mDrawnUnit = Object.Instantiate<GameObject>(UnitMarker); //THIS NEEDS TO BE CHANGED LATER
        mDrawnUnit.name = id.ToString();
        mDrawnUnit.transform.position = mCenter;
        mEdges = new List<Edge>();
        mError = 0.0f;
        ID = id;
    }
    public List<Edge> mEdges;
    public float mError;
    public Vector3 mCenter;
    public int ID;
    public GameObject mDrawnUnit; //gameobject which generates the drawing of the line
}

//public class Net
//{


//};

public class GNG : MonoBehaviour
{

    public uint maxAge = 5;
    public float UnitHeight = 1;
    public GameObject UnitMarker;
    public GameObject edgeMarker;
    public float epsilonB = 0.5f;
    public float epsilonN = 0.25f;
    public int lambda = 4;
    public float alpha = 0.3f;
    public float D = 0.8f;
    public int numRunsPerFrame = 1;
	public float targetErrorChange = 5.0f; //When the average slope is greater than this, stop the simulation
	public static int pastErrorsToKeep = 20;
	public float edgeRemovalThreshold = 0.4f;

    Vector3 getRandomPoint()
    {
        int numFloors = transform.FindChild("floors").childCount;
        int randIndex = Random.Range(0, numFloors);
        GameObject targetFloor = transform.FindChild("floors").GetChild(randIndex).gameObject; //we hes fleur
        float randX = Random.Range(targetFloor.transform.position.x - targetFloor.GetComponent<Renderer>().bounds.size.x / 2, targetFloor.transform.position.x + targetFloor.GetComponent<Renderer>().bounds.size.x / 2);
        float randZ = Random.Range(targetFloor.transform.position.z - targetFloor.GetComponent<Renderer>().bounds.size.z / 2, targetFloor.transform.position.z + targetFloor.GetComponent<Renderer>().bounds.size.z / 2);
        return new Vector3(randX, UnitHeight, randZ);
    }

	int getUnitIndex(Unit u)
	{
		for (int i = 0; i < units.Count; ++i )
		{
			if(units[i] == u)
			{
				return i;
			}
		}
		return -1; //this should never happen
	}

	bool isInList(int a, List<int> list)
	{
		for(int i=0; i<list.Count; ++i)
		{
			if(list[i] == a)
			{
				return true;
			}
		}
		return false;
	}

	//returns the number of disjoint regions in the graph
	int getNumRegions()
	{
		int numRegions = 0;

		List<int> unitIndices = new List<int>();
		for(int i=0; i<units.Count; ++i)
		{
			unitIndices.Add(i);
		}
		while(unitIndices.Count > 0)
		{
			List<int> thisRegion = new List<int>();
			List<int> removedUnits = new List<int>();
			thisRegion.Add(unitIndices[0]);
			unitIndices.RemoveAt(0);
			while(thisRegion.Count > 0)
			{
				for (int i = 0; i < units[thisRegion[0]].mEdges.Count; ++i) //add the indices of all units connected to the first in the region frontier to the region frontier
				{
					if( units[thisRegion[0]].mEdges[i].mPointA == units[thisRegion[0]] &&
						!isInList(getUnitIndex(units[thisRegion[0]].mEdges[i].mPointB), removedUnits))
					{
						thisRegion.Add(getUnitIndex(units[thisRegion[0]].mEdges[i].mPointB));
					}
					else if( units[thisRegion[0]].mEdges[i].mPointB == units[thisRegion[0]] &&
							 !isInList(getUnitIndex(units[thisRegion[0]].mEdges[i].mPointA), removedUnits))
					{
						thisRegion.Add(getUnitIndex(units[thisRegion[0]].mEdges[i].mPointA));
					}
				}
				removedUnits.Add(thisRegion[0]);
				thisRegion.RemoveAt(0);
			}
			for (int i = 0; i < unitIndices.Count; ++i ) //remove all the indices in removedUnits from the unitIndices list
			{
				if(isInList(unitIndices[i], removedUnits))
				{
					unitIndices.RemoveAt(i);
					--i;
				}
			}
			++numRegions;
		}
		return numRegions;
	}

	//edges more than 'percentAllowed' obstructed get removed
	//returns the number of edges removed
	int removeEdges(float percentAllowed)
	{
		int numBadEdges = 0;
		for(int i=0; i<edges.Count; ++i)
		{
			Ray r1 = new Ray(edges[i].mPointA.mCenter, edges[i].mPointB.mCenter - edges[i].mPointA.mCenter);
			Ray r2 = new Ray(edges[i].mPointB.mCenter, edges[i].mPointA.mCenter - edges[i].mPointB.mCenter);
			float distance = Vector3.Distance(edges[i].mPointA.mCenter, edges[i].mPointB.mCenter);
			float emptyDist = 0.0f;
			float newDist1 = 0.0f;
			float newDist2 = 0.0f;
			RaycastHit h1 = new RaycastHit();
			if (Physics.Raycast(r1, out h1, Mathf.Infinity) && h1.transform.gameObject.CompareTag("wall"))
			{
				newDist1 += Vector3.Distance(h1.point, edges[i].mPointA.mCenter);
				emptyDist += newDist1;
			}
			RaycastHit h2 = new RaycastHit();
			if (Physics.Raycast(r2, out h2, Mathf.Infinity) && h2.transform.gameObject.CompareTag("wall"))
			{
				newDist2 += Vector3.Distance(h2.point, edges[i].mPointB.mCenter);
				emptyDist += newDist2;
			}

			if(newDist1 > distance && newDist2 > distance)
			{
				continue;
			}
			else //determine the percent and
			{
				float percentObstructed = 1 - emptyDist/distance;
				Debug.Log(percentObstructed.ToString());
				if(percentObstructed > percentAllowed) //then remove that edge
				{
					for(int j=0; j<edges[i].mPointA.mEdges.Count; ++j)
					{
						if (edges[i].mPointA.mEdges[j].mPointA == edges[i].mPointA || edges[i].mPointA.mEdges[j].mPointB == edges[i].mPointA)
						{
							edges[i].mPointA.mEdges.RemoveAt(j);
							--j;
						}
					}
					for (int j = 0; j < edges[i].mPointB.mEdges.Count; ++j)
					{
						if (edges[i].mPointB.mEdges[j].mPointA == edges[i].mPointB || edges[i].mPointB.mEdges[j].mPointB == edges[i].mPointB)
						{
							edges[i].mPointB.mEdges.RemoveAt(j);
							--j;
						}
					}
					Destroy(edges[i].mDrawnLine);
					edges.RemoveAt(i);
					++numBadEdges;
					--i;
				}
			}

		}
		//now, delete orphaned units
		//for (int i = 0; i < units.Count; ++i)
		//{
		//	if(units[i].mEdges.Count == 0)
		//	{
		//		Destroy(units[i].mDrawnUnit);
		//		units.RemoveAt(i);
		//		--i;
		//	}
		//}
		return numBadEdges;
	}

    public List<Unit> units;
    public List<Edge> edges;
    int numSignals;
	int loopIterations;
	public bool completed;
	float[] pastErrors = new float[pastErrorsToKeep];
	bool edgesCleared;

	//double totalError()
	//{
	//	double err = 0;
	//	for (int i = 0; i < units.Count; ++i )
	//	{
	//		err += units[i].mError;
	//	}
	//	return err;
	//}

	List<Unit> getUnits()
	{
		return units;
	}

	List<Edge> getEdges()
	{
		return edges;
	}

    // Use this for initialization
    void Start ()
    {
        //set up data structures
        units = new List<Unit>();
        edges = new List<Edge>();
        numSignals = 0;
		loopIterations = 0;
		completed = false;
		edgesCleared = false;
		
		for(int i = 0; i < pastErrorsToKeep; ++i) {
			pastErrors[i] = 0f;
		}
        
        //STEP 0
        Unit firstPoint = new Unit(Random.Range(0, (float)GetComponent<BuildMaze>().getWidth()), UnitHeight, Random.Range(0, (float)GetComponent<BuildMaze>().getHeight()), UnitMarker);
        Unit secondPoint = new Unit(Random.Range(0, (float)GetComponent<BuildMaze>().getWidth()), UnitHeight, Random.Range(0, (float)GetComponent<BuildMaze>().getHeight()), UnitMarker);
        units.Add(firstPoint);
        units.Add(secondPoint);

    }

    // Update is called once per frame
    void Update ()
    {
		if(completed == false)
		{
			for (int a=0; a<numRunsPerFrame; ++a)
			{

				//STEP 1
				Vector3 signal = getRandomPoint();
				++numSignals;
				//GameObject drawnSignal = Object.Instantiate<GameObject>(UnitMarker);
				//drawnSignal.GetComponent<MeshRenderer>().material.color = Color.yellow;
				//drawnSignal.transform.position = signal;

				//STEP 2
				float minDist1 = 1000000; //these just need to be bigger. than other things.
				float minDist2 = 1000000;
				int minIndexUnitList1 = -1;
				int minIndexUnitList2 = -1;
				for (int i = 0; i < units.Count; ++i)
				{
					float thisDist = Vector3.Distance(signal, units[i].mCenter);
					if (thisDist < minDist2)
					{
						if (thisDist < minDist1)
						{
							minDist2 = minDist1;
							minIndexUnitList2 = minIndexUnitList1;
							minDist1 = thisDist;
							minIndexUnitList1 = i;
						}
						else
						{
							minDist2 = thisDist;
							minIndexUnitList2 = i;
						}
					}
				}
				Unit s1 = units[minIndexUnitList1];
				Unit s2 = units[minIndexUnitList2];

				//STEP 3
				for (int i = 0; i < s1.mEdges.Count; ++i)
				{
					++s1.mEdges[i].mAge;
				}

				//STEP 4
				s1.mError += Mathf.Pow(minDist1, 2);

				//STEP 5
				//Debug.Log(units[minIndexUnitList1].mCenter.ToString() + "  " + ((newUnit.mCenter - units[minIndexUnitList1].mCenter) * epsilonB).ToString());
				s1.mCenter += (signal - s1.mCenter) * epsilonB;
				s1.mDrawnUnit.transform.position = s1.mCenter;


				for (int i = 0; i < s1.mEdges.Count; ++i)
				{
					if (s1.mEdges[i].mPointA == s1)
					{
						s1.mEdges[i].mPointB.mCenter += (signal - s1.mEdges[i].mPointB.mCenter) * epsilonN;
						s1.mEdges[i].mPointB.mDrawnUnit.transform.position = s1.mEdges[i].mPointB.mCenter;
					}
					else
					{
						s1.mEdges[i].mPointA.mCenter += (signal - s1.mEdges[i].mPointA.mCenter) * epsilonN;
						s1.mEdges[i].mPointA.mDrawnUnit.transform.position = s1.mEdges[i].mPointA.mCenter;
					}
				}

				//STEP 6
				bool DeAged = false;
				for (int i = 0; i < s1.mEdges.Count && DeAged == false; ++i)
				{
					if ((s1.mEdges[i].mPointA == s1 && s1.mEdges[i].mPointB == s2) ||
						(s1.mEdges[i].mPointB == s1 && s1.mEdges[i].mPointA == s2))
					{
						s1.mEdges[i].mAge = 0;
						DeAged = true;
					}
				}
				if (DeAged == false)
				{
					Edge newEdge = new Edge(s1, s2);
					edges.Add(newEdge);
					s1.mEdges.Add(newEdge);
					s2.mEdges.Add(newEdge);
				}

			

				//STEP 7
					//removes edges
				for (int i = 0; i < edges.Count; ++i)
				{
					if (edges[i].mAge > maxAge)
					{
						Destroy(edges[i].mDrawnLine);
					
						for (int j=0; j<edges[i].mPointA.mEdges.Count; ++j)
						{
							if(edges[i].mPointA.mEdges[j] == edges[i])
							{
								edges[i].mPointA.mEdges.Remove(edges[i]);
								--j;
							}
						}
						for (int j = 0; j < edges[i].mPointB.mEdges.Count; ++j)
						{
							if (edges[i].mPointB.mEdges[j] == edges[i])
							{
								edges[i].mPointB.mEdges.Remove(edges[i]);
								--j;
							}
						}
						edges.Remove(edges[i]);
						--i;
					}
				}

					//remove units with no edges
				for (int i=0; i<units.Count; ++i)
				{
					if(units[i].mEdges.Count == 0)
					{
						//Debug.Log("DO WE GET IN HERE????");

						Destroy(units[i].mDrawnUnit);
						units.Remove(units[i]);
					}
				}

				//STEP 8

				if(numSignals % lambda == 0)
				{
						//find unit 'Q' with greatest error
					double maxErrorQ = 0;
					int maxErrorUnitIndexQ = 0;
					for (int i=0; i<units.Count; ++i)
					{
						if(units[i].mError > maxErrorQ)
						{
							maxErrorQ = units[i].mError;
							maxErrorUnitIndexQ = i;
						}
					}
					Unit Q = units[maxErrorUnitIndexQ];

						//insert a new unit 'R' half way between Q and its neighbor 'F' with the largest error
					double maxErrorF = 0;
					int maxErrorUnitIndexF = 0;
					bool isA = true;
					for (int i = 0; i < Q.mEdges.Count; ++i)
					{
						if (Q.mEdges[i].mPointA == Q && Q.mEdges[i].mPointB.mError > maxErrorF)
						{
							maxErrorF = Q.mEdges[i].mPointB.mError;
							maxErrorUnitIndexF = i;
							isA = false;
						}
						else if (Q.mEdges[i].mPointB == Q && Q.mEdges[i].mPointA.mError > maxErrorF)
						{
							maxErrorF = Q.mEdges[i].mPointA.mError;
							maxErrorUnitIndexF = i;
							isA = true;
						}
					}
					Unit F = (isA ? Q.mEdges[maxErrorUnitIndexF].mPointA : Q.mEdges[maxErrorUnitIndexF].mPointB);

					Unit R = new Unit(0.5f * (Q.mCenter + F.mCenter), UnitMarker);
					units.Add(R);

						//insert edges connecting R with Q and F, and remove the edge connecting Q and F
					Edge QR = new Edge(Q, R);
					Q.mEdges.Add(QR);
					R.mEdges.Add(QR);
					edges.Add(QR);
					Edge FR = new Edge(F, R);
					F.mEdges.Add(FR);
					R.mEdges.Add(FR);
					edges.Add(FR);
					for (int i = 0; i < Q.mEdges.Count; ++i)
					{
						if (Q.mEdges[i].mPointA == F || Q.mEdges[i].mPointB == F)
						{
							Edge toDestroy = Q.mEdges[i];
							Destroy(Q.mEdges[i].mDrawnLine);
							F.mEdges.Remove(Q.mEdges[i]);
							edges.Remove(Q.mEdges[i]);
							Q.mEdges.Remove(Q.mEdges[i]);
						}
					}

						//decrease the error of Q and F by scaling them by alpha
					Q.mError *= alpha;
					F.mError *= alpha;

						//initialize R with Q's new error
					R.mError = Q.mError;

					numSignals = 0;

					lambda += 5;//= (int)((double)lambda * 1.05);
				}

				//STEP 9
				for (int i=0; i<units.Count; ++i)
				{
					units[i].mError *= D;
				}

				//DRAW LINES AT THE VERY END
				for (int i = 0; i < edges.Count; ++i)
				{
					edges[i].mDrawnLine.GetComponent<LineRenderer>().SetPosition(0, edges[i].mPointA.mCenter - new Vector3(0.0f, 0.1f, 0.0f));
					edges[i].mDrawnLine.GetComponent<LineRenderer>().SetPosition(1, edges[i].mPointB.mCenter - new Vector3(0.0f, 0.1f, 0.0f));
				}

				//Check error of graph every X iterations
				if(loopIterations % 1500 == 0)
				{
					//Compute error value of graph					
					float avgSqrDist = 0.0f; //Current Error
					int numSamples = 1000;
					for(int c=0; c < numSamples; ++c)
					{
						Vector3 tempSig = getRandomPoint();
						float minTempDist = 1000000; //these just need to be bigger. than other things.
						for (int i = 0; i < units.Count; ++i)
						{
							float thisDist = Vector3.Distance(tempSig, units[i].mCenter);
							if (thisDist < minTempDist)
							{
								minTempDist = thisDist;
							}
						}
						avgSqrDist += Mathf.Pow(minTempDist, 2);
					}

					avgSqrDist /= (float)numSamples;
					PlotManager.Instance.PlotAdd("avgError", avgSqrDist);
					//Debug.Log("Avg. Error = " + avgSqrDist.ToString());

					//Determine current change in error
					float errorChangeTotal = 0f;
					int nonZeroErrors = 1;
					errorChangeTotal = avgSqrDist - pastErrors[0];
					
					//Move through the past error values and add to running sum change in error
					for(int e = 0; e < pastErrorsToKeep-1 && pastErrors[e+1] != 0f; ++e) {
						errorChangeTotal += pastErrors[e] - pastErrors[e+1];
						++nonZeroErrors;
					}
					
					//Average the error
					errorChangeTotal /= nonZeroErrors;
					//Debug.Log("Avg. Change in Error = " + errorChangeTotal.ToString());
					//Log error values
					for(int e = pastErrorsToKeep-1 ; e > 0;--e) {
						pastErrors[e] = pastErrors[e-1];
					}
					pastErrors[0] = avgSqrDist;
					
					//if the error change average is between 0+-targetErrorChange 
					if (-1*targetErrorChange < errorChangeTotal && errorChangeTotal < targetErrorChange)
					{
						completed = true;
					
					}
				}

				

				++loopIterations;


			}

		}

		//if(completed == true && edgesCleared == false)
		//{
		//	//int numEdgesRemoved = removeEdges(edgeRemovalThreshold);
		//	//Debug.Log(numEdgesRemoved.ToString() + " of " + edges.Count + " edges removed.");
		//	int numRegions = getNumRegions();
		//	Debug.Log("Mesh has " + numRegions.ToString() + " region" + ((numRegions == 1) ? "." : "s."));
		//	edgesCleared = true;
		//}
    }
}
