using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuttingChart : MonoBehaviour, IGarmin3DChart {

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
