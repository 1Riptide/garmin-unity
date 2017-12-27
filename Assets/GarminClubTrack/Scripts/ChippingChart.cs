using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChippingChart : MonoBehaviour, IGarmin3DChart {

	// Default shot object.
	public GameObject chippingDataPoint;
	public static Stack<GameObject> dataPoints = new Stack<GameObject>();
	public bool isFocused {get; set;}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void Initialize(String json){

	}
}
