using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class ShotDistroChart : MonoBehaviour {

	// Default shot object.
	public GameObject dataPoint;
	public static Stack<GameObject> dataPoints = new Stack<GameObject>();
	// For comera focus.
	public GameObject target;
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
	bool isEnabled =  true;

	// Genesis
	void Start () {
		//InvokeRepeating ("TimedDemo", .1f, 30f);	
		/*
		while(true)
		{
			String testJSONData = "{\n  \"numberOfRounds\": 1,\n  \"percentFairwayLeft\": 0,\n  \"percentFairwayRight\": 0,\n  \"percentFairwayHit\": 0,\n  \"minShotDistance\": 0,\n  \"maxShotDistance\": 113.56,\n  \"avgShotDistance\": 0,\n  \"minDispersionDistance\": 0,\n  \"maxDispersionDistance\": 0,\n  \"shotDispersionDetails\": [\n    {\n      \"shotId\": 1,\n      \"scorecardId\": 1122,\n      \"holeNumber\": 2,\n      \"shotTime\": \"2017-12-05\",\n      \"clubId\": 0,\n      \"dispersionDistance\": 21.3,\n      \"shotDistance\": 113.56,\n      \"fairwayShotOutcome\": \"LEFT\"\n    }\n  ]\n}";
			//Initialize (testJSONData);
			Initialize("");
			yield return new WaitForSeconds(8);
			yield return new WaitForEndOfFrame();
		}
		*/
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

	/*
	void TimedDemo(){
		Debug.Log ("TimedDemo");
		while (isEnabled) {
			String testJSONData = "{\n  \"numberOfRounds\": 1,\n  \"percentFairwayLeft\": 0,\n  \"percentFairwayRight\": 0,\n  \"percentFairwayHit\": 0,\n  \"minShotDistance\": 0,\n  \"maxShotDistance\": 113.56,\n  \"avgShotDistance\": 0,\n  \"minDispersionDistance\": 0,\n  \"maxDispersionDistance\": 0,\n  \"shotDispersionDetails\": [\n    {\n      \"shotId\": 1,\n      \"scorecardId\": 1122,\n      \"holeNumber\": 2,\n      \"shotTime\": \"2017-12-05\",\n      \"clubId\": 0,\n      \"dispersionDistance\": 21.3,\n      \"shotDistance\": 113.56,\n      \"fairwayShotOutcome\": \"LEFT\"\n    }\n  ]\n}";
			//Initialize (testJSONData);
			Initialize(testJSONData);
		}

		yield return new WaitForSeconds (5);
	} 
	*/


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

	IEnumerator AddDataPoints(ShotDispersionData dispersionData){
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

	// Called from hosting Platform (Android/Web/Etc). Method by name "methodName" will be called in Unity.
	public void externalCallDispatcher(String args){

		char[] splitOn = { '|' };
		String[] strArr = args.Split (splitOn);
		String methodName = strArr [0];
		// Use raw args if no delimiter was provided.
		if (methodName == null)
			methodName = args;
		
		if (methodName != null) {
			switch (methodName) {
			case "ToggleCamera":
				Debug.Log ("externalCallDispatcher() : calling ToggleCamera");
				Camera cam = Camera.main;
				CameraTouchControl script = cam.GetComponent<CameraTouchControl>();
				// Force ToggleCameraAngle
				script.ToggleCameraAngle (true);
				break;
			default :
				Debug.Log ("externalCallDispatcher() : I hear ya! methodName not found! = " + methodName + " args = " + args);
				break;
			}
		} else {
			Debug.Log ("externalCallDispatcher() : I hear ya! methodName is null! Stopping.  args = " + args);
		}
	}

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
				ShotDispersionData dispersionData = ShotDispersionData.CreateFromJSON (json);
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
