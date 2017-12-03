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
		callToAndroid ();
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
		int shotCount = 150;
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

	static void callToAndroid(){
		AndroidJavaClass pluginClass = new AndroidJavaClass("com.garmin.android.apps.golf.ui.fragments.shottrack.ShotTrackFragment");
	}

	void addDataPoint(Vector3 location){
		Instantiate(dataPoint, location, Quaternion.identity);
	}
}
