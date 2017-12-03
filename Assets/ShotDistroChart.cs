using UnityEngine;
using System;
using System.Collections;

public class ShotDistroChart : MonoBehaviour {

	// Default shot object.
	public GameObject dataPoint;
	// For comera focus.
	public GameObject target;
	// Average distance marker
	public GameObject averageDistanceMarker;
	float averageShotDistanceOnChart = 0f;
	// min/max/mean
	int maxY;
	int minY;
	int avgY;
	float transitionSpeed = .8f;

	// Use this for initialization
	void Start () {
		StartCoroutine (addDataPoints ());
		callToAndroid ();
	}

	void Update () {
		Vector3 averageShotVector = new Vector3(0,0, averageShotDistanceOnChart);
		averageDistanceMarker.transform.position = Vector3.Lerp (averageDistanceMarker.transform.position, averageShotVector, transitionSpeed * Time.deltaTime);	
	}

	IEnumerator addDataPoints(){
		int shotCount = 150;
		float longestShot = 0f;
		float shortestShot = 0f;
		float[] shotDistanceLog = new float[shotCount];
		// Since we are plotting shots in 2D space (on a plane), we dont need to keep calculating in loop below.
		float verticalPosition = 0f;
		// Plot the shots.
		for (int i = 0; i < shotCount; i++) {
			// x range (lateral position) is 9 thru -9
			// z range (distance) is 14 thru -14
			yield return new WaitForSeconds(0);
			float lateralPosition = UnityEngine.Random.Range (-9.0f, 9.0f);
			float distance = UnityEngine.Random.Range (-14.0f, 14.0f);
			shotDistanceLog [i] = distance;
			longestShot = distance > longestShot ? distance : longestShot;
			shortestShot = distance < shortestShot ? distance : shortestShot;
			addDataPoint(new Vector3(lateralPosition, verticalPosition, distance));

			// continually calcualate average (for animation).
			averageShotDistanceOnChart = GetMedian (shotDistanceLog);
		}
		print ("Longest shot =  " + longestShot);
		print ("Shortest shot = " + shortestShot);
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
		//print("callToAndroid called() : " + pluginClass.CallStatic<string>("callFromUnity"));
	}

	void addDataPoint(Vector3 location){
		Instantiate(dataPoint, location, Quaternion.identity);
	}
}
