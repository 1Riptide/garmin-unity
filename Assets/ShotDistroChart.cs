using UnityEngine;
using System;
using System.Collections;

public class ShotDistroChart : MonoBehaviour {

	// Default shot object.
	public GameObject dataPoint;
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

	// min/max/mean
	int maxY;
	int minY;
	int avgY;
	float transitionSpeed = .8f;

	// Genesis
	void Start () {

		averageShotZOnChart = bottomOfChartZPosition;
		longestShotZOnChart = bottomOfChartZPosition;
		shortestShotZOnChart = bottomOfChartZPosition;
		StartCoroutine (addDataPoints ());
		// This is intended as a test to call a platform level dialog on either Android or Web.
		ShowPlatformDialog("Hello from Unity!");
	}

	// Looper - runs (n)times a second depending on framerate.
	void Update () {
		//Animate chart markers
		Vector3 longestShotVector = new Vector3(0,0, longestShotZOnChart);
		Vector3 averageShotVector = new Vector3(0,0, averageShotZOnChart);
		Vector3 shortestShotVector = new Vector3(0,0, shortestShotZOnChart);
		averageDistanceMarker.transform.position = Vector3.Lerp (averageDistanceMarker.transform.position, averageShotVector, transitionSpeed * Time.deltaTime);	
		minDistanceMarker.transform.position = Vector3.Lerp (minDistanceMarker.transform.position, shortestShotVector, transitionSpeed * Time.deltaTime);	
		maxDistanceMarker.transform.position = Vector3.Lerp (maxDistanceMarker.transform.position, longestShotVector, transitionSpeed * Time.deltaTime);	

	}

	IEnumerator addDataPoints(){
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
			addDataPoint(new Vector3(lateralPosition, verticalPosition, distance));
			// Stall the loop for aesthetics as shots drop.
			yield return new WaitForSeconds(0);
		}
	}

	void addDataPoint(Vector3 location){
		Instantiate(dataPoint, location, Quaternion.identity);
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
}
