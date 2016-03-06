using UnityEngine;
using System.Collections;

public class Plotter : MonoBehaviour {

	// Use this for initialization
	void Start () {
		PlotManager.Instance.PlotCreate("avgError", 0.0f, 50.0f, new Color(1.0f, 0.0f, 0.0f), new Vector2(0, 0)); 
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
