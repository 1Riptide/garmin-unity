using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class DriveChart : MonoBehaviour {

	// Default shot object.
	public GameObject dataPoint;
	public static Stack<GameObject> dataPoints = new Stack<GameObject>();
	// chart markers for distances
	public GameObject maxDistanceMarker;
	public GameObject averageDistanceMarker;
	public GameObject minDistanceMarker;

	// For animating chart markers
	float bottomOfChartZPosition = -15f;
	float averageShotZOnChart;
	float longestShotZOnChart;
	float shortestShotZOnChart;
	Vector3 longestShotVector;
	Vector3 averageShotVector;
	Vector3 shortestShotVector;

	// min/max/mean
	int maxY;
	int minY;
	int avgY;
	float transitionSpeed = .8f;

	// When GameObject is out of view, disable and stop all Looping behavior
	public bool isEnabled =  true;

	// Genesis
	void Start () {
		// This must be called by external platform. Pass JSON.
		Initialize("");
	}

	// Looper - runs (n)times a second depending on framerate.
	void Update () {
		if (isEnabled) {
			//Animate chart markers
			if (averageShotVector.z > bottomOfChartZPosition) {
				averageDistanceMarker.transform.position = Vector3.Lerp (averageDistanceMarker.transform.position, averageShotVector, transitionSpeed * Time.deltaTime);	
			}
			if (shortestShotVector.z > bottomOfChartZPosition) {
				minDistanceMarker.transform.position = Vector3.Lerp (minDistanceMarker.transform.position, shortestShotVector, transitionSpeed * Time.deltaTime);	
			}
			if (longestShotVector.z > bottomOfChartZPosition) {
				maxDistanceMarker.transform.position = Vector3.Lerp (maxDistanceMarker.transform.position, longestShotVector, transitionSpeed * Time.deltaTime);	
			}
		}
	}
		
	IEnumerator AddDataPoints(){
		Debug.Log ("AddDataPoints no args");
		int shotCount = 87;
		float[] shotDistanceLog = new float[shotCount];
		// Since we are plotting shots in 2D space (on a plane), we dont need to keep calculating in loop below.
		float verticalPosition = 0f;
		// Plot the shots.
		for (int i = 0; i < shotCount; i++) {
			float lateralPosition = UnityEngine.Random.Range (-9.0f, 9.0f);
			float distance = UnityEngine.Random.Range (-14.0f, 14.0f);
			shotDistanceLog [i] = distance;
			// continually calculate these values(for animation).
			averageShotZOnChart = GetMedian (shotDistanceLog);
			longestShotZOnChart = distance > longestShotZOnChart ? distance : longestShotZOnChart;
			if (i == 0) {
				shortestShotZOnChart = distance;
			} else {
				shortestShotZOnChart = distance < shortestShotZOnChart ? distance : shortestShotZOnChart;
			}
			// Update shot vectors. Used to animate shot markers.
			longestShotVector = new Vector3 (0, 0, longestShotZOnChart);
			averageShotVector = new Vector3 (0, 0, averageShotZOnChart);
			shortestShotVector = new Vector3 (0, 0, shortestShotZOnChart);

			dataPoints.Push(AddDataPoint(new Vector3(lateralPosition, verticalPosition, distance)));
			Debug.Log ("AddDataPoints no args");
			// Stall the loop for aesthetics as shots drop.
			yield return new WaitForSeconds(0);
		}
	}

	IEnumerator AddDataPoints(ClubTrackDriveDataDTO dispersionData){
		var shotCount = dispersionData.shotDispersionDetails.Length;
		Debug.Log ("AddDataPoints shotCount = " + shotCount);
		float[] shotDistanceLog = new float[shotCount];
		// Since we are plotting shots in 2D space (on a plane), we dont need to keep calculating in loop below.
		float verticalPosition = 0f;
		// Plot the shots.
		for (int i = 0; i < shotCount; i++) {
			float lateralPosition = UnityEngine.Random.Range (-9.0f, 9.0f);
			float distance = UnityEngine.Random.Range (-14.0f, 14.0f);
			shotDistanceLog [i] = distance;
			// continually calculate these values(for animation).
			averageShotZOnChart = GetMedian (shotDistanceLog);
			longestShotZOnChart = distance > longestShotZOnChart ? distance : longestShotZOnChart;
			if (i == 0) {
				shortestShotZOnChart = distance;
			} else {
				shortestShotZOnChart = distance < shortestShotZOnChart ? distance : shortestShotZOnChart;
			}
			// Update shot vectors. Used to animate shot markers.
			longestShotVector = new Vector3 (0, 0, longestShotZOnChart);
			averageShotVector = new Vector3 (0, 0, averageShotZOnChart);
			shortestShotVector = new Vector3 (0, 0, shortestShotZOnChart);
			dataPoints.Push(AddDataPoint(new Vector3(lateralPosition, verticalPosition, distance)));
			// Stall the loop for aesthetics as shots drop.
			yield return new WaitForSeconds(0);
		}
	}

	GameObject AddDataPoint(Vector3 location){
		return Instantiate(dataPoint, location, Quaternion.identity);
	}

	static float GetMedian(float[] sourceNumbers) {
		//Framework 2.0 version of this method. there is an easier way in F4        
		if (sourceNumbers == null || sourceNumbers.Length == 0)
			throw new System.Exception("Median of empty array not defined.");

		//make sure the list is sorted, but use a new array
		float[] sortedPNumbers = (float[])sourceNumbers.Clone();
		Array.Sort(sortedPNumbers);

		//get the median
		int size = sortedPNumbers.Length;
		int mid = size / 2;
		float median = (size % 2 != 0) ? (float)sortedPNumbers[mid] : ((float)sortedPNumbers[mid] + (float)sortedPNumbers[mid - 1]) / 2;
		return median;
	}

	#if UNITY_ANDROID
	private AndroidJavaObject javaObj = null;
	private AndroidJavaObject GetJavaObject() {
		if (javaObj == null) {
			javaObj = new AndroidJavaObject("com.garmin.android.apps.golf.ui.fragments.shottrack.ShotTrackFragment");
		}
		return javaObj;
	}

	private void ShowPlatformDialog(string message) {
		GetJavaObject().Call("unityToastTest", message);
		Debug.Log ("ShowPlatformDialog() : For Android - called.");
	}
	#else
	// Web Impl
	private void ShowPlatformDialog(string message) {
		//Show an alert on web platform?
		Debug.Log ("ShowPlatformDialog() : For Web - called.");
	}
	#endif

	void Cleanup(){
		averageShotZOnChart = bottomOfChartZPosition;
		longestShotZOnChart = bottomOfChartZPosition;
		shortestShotZOnChart = bottomOfChartZPosition;

		longestShotVector = new Vector3 (0, 0, longestShotZOnChart);
		averageShotVector = new Vector3 (0, 0, averageShotZOnChart);
		shortestShotVector = new Vector3 (0, 0, shortestShotZOnChart);

		if (dataPoints != null) {
			foreach (GameObject data in dataPoints) {
				Destroy (data);
			}
		}
	}

	// Call should ultimately come from external platform (Mobile/Web). Json provided should drive scene.
	public void Initialize(String json){
		Debug.Log ("Initialize() json = " + json);
		Cleanup();

		if (json == null || json.Length == 0) {
			// Get shot Count from JSON
			var shotCount = 87;
			try{
				StartCoroutine (AddDataPoints ());
			}catch(Exception e){
				Debug.Log ("Exception calling AddDataPoints : " + e);
			}
			// This is intended as a test to call a platform level dialog on either Android or Web.
			ShowPlatformDialog ("Hello from Unity!");
		} else {
			Debug.Log ("Initialize() json is not null. Casting to JSON obj...");
			try{
				ClubTrackDriveDataDTO dispersionData = ClubTrackDriveDataDTO.CreateFromJSON (json);
				Debug.Log ("Initialize() created " + dispersionData.shotDispersionDetails);
				if (dispersionData != null && dispersionData.shotDispersionDetails != null) {
					StartCoroutine (AddDataPoints (dispersionData));
				} else {
					Debug.Log ("json was null. stopping! " + json);
				}
			}catch(Exception e){
				Debug.Log ("Exception parsing JSON : " + e);
			}

		}
	}
}	
