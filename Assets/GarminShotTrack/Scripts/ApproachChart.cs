using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApproachChart : MonoBehaviour, IGarmin3DChart {

	// Default shot object.
	public GameObject approachDataPoint;
	public static Stack<GameObject> dataPoints = new Stack<GameObject>();
	private bool isInitialized = false;
	public bool isEnabled {get; set;}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
