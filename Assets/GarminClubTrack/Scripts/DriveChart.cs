using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

public class DriveChart : MonoBehaviour, IGarmin3DChart
{
	public bool isFocused { get; set; }
	// Default shot object.
	public GameObject whiteDataPoint;
	public GameObject redDataPoint;
	public static Stack<GameObject> dataPoints = new Stack<GameObject> ();
	// chart markers for distances
	public GameObject maxDistanceMarker;
	public GameObject avgDistanceMarker;
	public GameObject minDistanceMarker;

	TextMesh averageText;
	TextMesh maxText;
	TextMesh minText;

	// Used to devise a ratio with which we plot datapoints based on real world distances.
	private static readonly float[] DistanceBounds = new float[]{ -14.0f, 14.0f };
	private static readonly float[] LateralBounds = new float[]{ -9.0f, 9.0f };
	private static float DistanceLength = Math.Abs (DistanceBounds [0]) + Math.Abs (DistanceBounds [1]);
	// For animating chart markers
	float bottomOfChartZPosition = -15f;
	float avgShotZOnChart;
	float maxShotZOnChart;
	float minShotZOnChart;

	float maxShotDistanceOnChart = 0f;
	float avgShotDistanceOnChart = 0f;
	float minShotDistanceOnChart = 0;

	Vector3 longestShotVector;
	Vector3 averageShotVector;
	Vector3 shortestShotVector;


	// min/max/mean
	int maxY;
	int minY;
	int avgY;
	float transitionSpeed = .8f;
	private static bool isTightRange = false;
	//Are all shots within 10 yards/meters?

	// Genesis
	void Start ()
	{
		maxText = (TextMesh)maxDistanceMarker.GetComponentInChildren (typeof(TextMesh))as TextMesh;
		averageText = (TextMesh)avgDistanceMarker.GetComponentInChildren (typeof(TextMesh))as TextMesh;
		minText = (TextMesh)minDistanceMarker.GetComponentInChildren (typeof(TextMesh))as TextMesh;
		maxDistanceMarker.SetActive (false);
		minDistanceMarker.SetActive (false);
		avgDistanceMarker.SetActive (false);
	}

	public void MockInitialize ()
	{
		// This must be called by external platform. Pass JSON.
		Initialize (getMockJSON2());
		isFocused = true;
	}

	// Looper - runs (n)times a second depending on framerate.
	void Update ()
	{
		if (isFocused == true) {
			if (avgDistanceMarker.activeSelf.Equals (true)) {
				avgDistanceMarker.transform.position = Vector3.Lerp (avgDistanceMarker.transform.position, averageShotVector, transitionSpeed * Time.deltaTime);
			}

			if (minDistanceMarker.activeSelf.Equals (true)) {
				minDistanceMarker.transform.position = Vector3.Lerp (minDistanceMarker.transform.position, shortestShotVector, transitionSpeed * Time.deltaTime);	
			}

			maxDistanceMarker.transform.position = Vector3.Lerp (maxDistanceMarker.transform.position, longestShotVector, transitionSpeed * Time.deltaTime);	
		}
	}

	IEnumerator AddDataPoints (String json)
	{
		var clubTrackDriveData = JSON.Parse (json);
		var shotData = clubTrackDriveData ["shotDispersionDetails"];
		var shotCount = shotData.Count;
		Debug.Log ("AddDataPoints shotData count  " + shotCount);

		// Get the min and max ranges for both length (distance) and width (lateral distance) in yards/meters
		float maxDistance = clubTrackDriveData ["maxShotDistance"];
		float minDistance = clubTrackDriveData ["minShotDistance"];
		float maxLateralDistance = clubTrackDriveData ["maxDispersionDistance"];
		float minLateralDistance = clubTrackDriveData ["minDispersionDistance"];

		// Get the min and max bounds for both length and width of 3D space coords.
		float zMinBounds = DistanceBounds [0];
		float zMaxBounds = DistanceBounds [1];
		float xMinBounds = LateralBounds [0];
		float xMaxBounds = LateralBounds [1];

		/**
		 * We need this negative offset in order to plot a range of real world coordinates
		 * onto an arbitrary 3D plane which has no relationship to the real world origin of the data.
		 * With the understanding the the center of a given plane sits at [0,0] (x and y respectively),
		 * a minimum range shot that is to the left of center should appear in the bottom left of the
		 * plane. This would place the shot into negative 2D coordinate space ex: [-9, -21]. In order to 
		 * plot this correctly we must offset the ranges from real-world to a 2D coordinate space [x,y].
		 * Here we are simply removing the sign from any possible values from the range, and will use those
		 * to offet the caluculations elsewhere.
		 */ 

		float negativeZOffset = Math.Abs (zMinBounds);
		float lateralOffset = Math.Abs (xMinBounds);

		// Log of Z Positions
		float[] shotDistanceZLog = new float[shotCount];
		// Log of distances
		float[] shotDistanceLog = new float[shotCount];

		// Since we are plotting shots in 2D space (on a plane), we dont need to keep calculating vertical dimens in loop below.
		float verticalPosition = 0f;
	
		/**
		 * As long as there are at least 2 shots, there is also a minDistance Marker and maxDistance Marker.
		 * Single shot - no markers.
		 */
		if (shotCount > 1) {
			minDistanceMarker.SetActive (true);
			maxDistanceMarker.SetActive (true);
		}

		// Unless there are at least 4 shots, do not show 75% marker (formerly Avg Marker).
		if (shotCount > 3) {
			avgDistanceMarker.SetActive (true);
		}
			
		// Plot the shots.
		for (int i = 0; i < shotCount; i++) {
			JSONNode shotDetail = shotData [i];
			if (shotDetail != null) {
				float distance = shotDetail ["shotDistance"];
				float ZPosition = 0f;
				if (shotCount > 1) {
					// Bottom(min) to Top(max)
					float offsetDistance = distance + negativeZOffset;
					float distanceRatio = offsetDistance / (maxDistance + negativeZOffset);

					shotDistanceLog [i] = distance; // Real world distance log

					// continually calculate these values(for animation).
					if (avgDistanceMarker.activeSelf.Equals (true)) {
						avgShotZOnChart = Get75th (shotDistanceZLog);
						avgShotDistanceOnChart = Get75th (shotDistanceLog);
						averageShotVector = new Vector3 (0, 0, avgShotZOnChart);
						averageText.text = Mathf.Round (avgShotDistanceOnChart).ToString ();
					}
			
					if (isTightRange.Equals (true)) {
						Debug.Log ("###  IS TIGHT RANGE! - all shots are within 10 y/m of each other.");
						distanceRatio = distance / (maxDistance);
						ZPosition = (distanceRatio * zMaxBounds) - negativeZOffset;
					
					} else {
						Debug.Log ("###  IS NOT TIGHT RANGE! - shots are not all grouped closely.");
						ZPosition = (distanceRatio * (zMaxBounds + negativeZOffset)) - negativeZOffset;
					}
					// Keep track so we can know averages
					shotDistanceZLog [i] = ZPosition; // Position in 2D space log
					maxShotZOnChart = maxShotZOnChart != null ? (ZPosition > maxShotZOnChart) ? ZPosition : maxShotZOnChart : ZPosition;

					if (maxShotDistanceOnChart < distance) {
						maxShotDistanceOnChart = distance;
					}	
						
					if (minDistanceMarker.activeSelf.Equals (true)) {

						if (i == 0) {
							minShotDistanceOnChart = distance;
						} else {
							if (minShotDistanceOnChart > distance) {
								minShotDistanceOnChart = distance;
							}
						}
						minText.text = Mathf.Round (minShotDistanceOnChart).ToString ();
						if (i == 0) {
							minShotZOnChart = ZPosition;
						} else {
							minShotZOnChart = ZPosition < minShotZOnChart ? ZPosition : minShotZOnChart;
						}
						Debug.Log ("MIN DISTANCE MARKER IS TOTALLY ACTIVE!!!!");
						shortestShotVector = new Vector3 (0, 0, minShotZOnChart);
					} else {
						Debug.Log ("MIN DISTANCE MARKER IS NOT ACTIVE!!!!");
					}
					// Special case
					if (shotCount > 2) {
						// Update shot vectors. Used to animate shot markers.
						longestShotVector = new Vector3 (0, 0, maxShotZOnChart);
					}
				}

				// Left(min) to Right(max)
				float lateralDistance = shotDetail ["dispersionDistance"];
				float XPosition = 0f;

				XPosition = lateralDistance / 6;
				float scaleByRatio = 1f; // default
				// Special Case
				if (shotCount == 2) {
					if (maxShotZOnChart == ZPosition) {
						// Max value goes to top
						ZPosition = Math.Abs (bottomOfChartZPosition);
						// Update shot vectors. Used to animate shot markers.
						longestShotVector = new Vector3 (0, 0, ZPosition);
					} else {
						// Min value goes to bottom
						ZPosition = bottomOfChartZPosition;
						shortestShotVector = new Vector3 (0, 0, ZPosition);
					}
				} else {
					// 3 or more shots. Normalize all ZPositons to scale into  available Z space.
					float longestShot = longestShotVector.z;
					float shortestShot = shortestShotVector.z;
					float shotRange = Math.Abs (longestShot) + Math.Abs (shortestShot);
					float distanceScaleRatio = shotRange / DistanceLength; // .50 would mean shot range is 50% the vertical length of chart
					//scaleByRatio -= distanceScaleRatio;
					Debug.Log ("longestShot= " + longestShot + "  shortestShot= " + shortestShot + "  shotRange= " + shotRange + "  distanceScaleRatio= " + distanceScaleRatio + "  scaleByRatio= " + scaleByRatio); 

					//ZPosition = bottomOfChartZPosition - ((scaleByRatio * ZPosition));
				}
					
				Vector3 dataPointPosition = new Vector3 (XPosition, verticalPosition, ZPosition);
				/*
				if (scaleByRatio != 1f) {
					// multiple by scalar
					dataPointPosition *= scaleByRatio;
					// reset these properties.
					dataPointPosition.x = XPosition;
					dataPointPosition.y = verticalPosition;
				}
				*/
				Debug.Log ("LateralDistance ??? -> " + lateralDistance);
				if (lateralDistance < 60f && lateralDistance > -60f) {
					// If within 60f each direction, we use white balls.
					dataPoints.Push (AddDataPoint (whiteDataPoint, dataPointPosition));
				} else {
					// Otherwise, too far right. Your balls are red. And stuck on edge. Sorry.
					if (XPosition > xMaxBounds) {
						dataPointPosition.x = xMaxBounds + 1f;
						dataPoints.Push (AddDataPoint (redDataPoint, dataPointPosition));
					} else if (XPosition < xMinBounds) {
						// too far left. Stick to edge.
						dataPointPosition.x = xMinBounds - 1f;
						dataPoints.Push (AddDataPoint (redDataPoint, dataPointPosition));
					}
				}
					
				// Update max text
				maxText.text = Mathf.Floor (maxShotDistanceOnChart).ToString ();
			} else {
				Debug.Log ("AddDataPoints ShotData is null! = " + shotData);
			}
			// Stall the loop for aesthetics as shots drop.
			yield return new WaitForSeconds (0);
		}
	}

	GameObject AddDataPoint (GameObject dataPoint, Vector3 location)
	{
		return Instantiate (dataPoint, location, Quaternion.identity);
	}

	static float GetMedian (float[] sourceNumbers)
	{
		//Framework 2.0 version of this method. there is an easier way in F4        
		if (sourceNumbers == null || sourceNumbers.Length == 0)
			throw new System.Exception ("Median of empty array not defined.");

		//make sure the list is sorted, but use a new array
		float[] sortedPNumbers = (float[])sourceNumbers.Clone ();
		Array.Sort (sortedPNumbers);

		//get the median
		int size = sortedPNumbers.Length;
		int mid = size / 2;
		float median = (size % 2 != 0) ? (float)sortedPNumbers [mid] : ((float)sortedPNumbers [mid] + (float)sortedPNumbers [mid - 1]) / 2;
		return median;
	}

	/**
	 * return 75th percentile of a collection of numbers
	 */
	static float Get75th (float[] sourceNumbers)
	{
		Debug.Log ("Get75TH ");
		int length = sourceNumbers.Length;
		//Framework 2.0 version of this method. there is an easier way in F4        
		if (sourceNumbers == null || length == 0)
			throw new System.Exception ("Get75th of empty array not defined.");

		List<float> floatingList = new List<float> (sourceNumbers);
		float upper = floatingList.Max ();

		if (length == 1) {
			isTightRange = false;
			return upper;
		} else {
			float dec = .75f;
			float lower = floatingList.Min ();
			// Keep track if we are working with a tightly grouped collection of shots. Scale as appropriate.
			Debug.Log ("upper-lower = " + (upper - lower));
			isTightRange = (upper - lower) <= 10;
			float diff = upper - lower;
			return lower + (dec * diff);
		}
	}

	void Cleanup ()
	{
		avgShotZOnChart = bottomOfChartZPosition;
		maxShotZOnChart = bottomOfChartZPosition;
		minShotZOnChart = bottomOfChartZPosition;

		longestShotVector = new Vector3 (0, 0, maxShotZOnChart);
		averageShotVector = new Vector3 (0, 0, avgShotZOnChart);
		shortestShotVector = new Vector3 (0, 0, minShotZOnChart);

		if (dataPoints != null) {
			foreach (GameObject data in dataPoints) {
				Destroy (data);
			}
		}
	}

	// Call should ultimately come from external platform (Mobile/Web). Json provided should drive scene.
	public void Initialize (String json)
	{
		Debug.Log ("Initialize() json = " + json);
		Cleanup ();

		if (json == null || json.Length == 0) {
			Debug.Log ("Exception calling AddDataPoints : ");
		} else {
			try {
				Debug.Log ("Initialize() json is not null. Casting to JSON obj...");
				StartCoroutine (AddDataPoints (json));
			} catch (Exception e) {
				Debug.Log ("Exception parsing JSON : " + e);
			}
		}
	}

	String getMockJSON(){
		return "{\\r\\n\\\"numberOfRounds\\\": 1,\\r\\n\\\"percentFairwayLeft\\\": 0,\\r\\n\\\"percentFairwayRight\\\": 0,\\r\\n\\\"percentFairwayHit\\\": 50,\\r\\n\\\"minShotDistance\\\": 252.388,\\r\\n\\\"maxShotDistance\\\": 510.522,\\r\\n\\\"avgShotDistance\\\": 490,\\r\\n\\\"minDispersionDistance\\\": 1.35,\\r\\n\\\"maxDispersionDistance\\\": 11.57,\\r\\n\\\"longestShot\\\": {\\r\\n\\\"holeShot\\\": {\\r\\n\\\"holeNumber\\\": 5,\\r\\n\\\"holeImageUrl\\\": \\\"http:\\/\\/birdseye.garmin.com\\/birdseye\\/golf\\/006-d2471-23\\/gd6000\\/gid006295\\/006-d2471-23\\/0\\/hole5.jpg?garmindlm=1550774416_20bc44aeef11c5680116624ccf61bcce\\\",\\r\\n\\\"shots\\\": [\\r\\n{\\r\\n\\\"id\\\": 82520,\\r\\n\\\"scorecardId\\\": 17967,\\r\\n\\\"playerProfileId\\\": 1649886,\\r\\n\\\"shotTime\\\": 1518095350000,\\r\\n\\\"shotTimeZoneOffset\\\": 0,\\r\\n\\\"clubId\\\": 20823287,\\r\\n\\\"holeNumber\\\": 5,\\r\\n\\\"autoShotType\\\": \\\"USED\\\",\\r\\n\\\"startLoc\\\": {\\r\\n\\\"x\\\": 398,\\r\\n\\\"y\\\": 678\\r\\n},\\r\\n\\\"endLoc\\\": {\\r\\n\\\"x\\\": 361,\\r\\n\\\"y\\\": 120\\r\\n},\\r\\n\\\"meters\\\": 510.522\\r\\n}\\r\\n]\\r\\n},\\r\\n\\\"courseSnapshotId\\\": 497,\\r\\n\\\"courseName\\\": \\\"Heritage Park Golf Course\\\"\\r\\n},\\r\\n\\\"shotDispersionDetails\\\": [\\r\\n{\\r\\n\\\"shotId\\\": 82508,\\r\\n\\\"scorecardId\\\": 17967,\\r\\n\\\"holeNumber\\\": 1,\\r\\n\\\"shotTime\\\": \\\"2018-02-08T12:00:00.000Z\\\",\\r\\n\\\"clubId\\\": 20823302,\\r\\n\\\"dispersionDistance\\\": 4.87,\\r\\n\\\"shotDistance\\\": 252.388,\\r\\n\\\"fairwayShotOutcome\\\": \\\"NO_FAIRWAY\\\"\\r\\n},\\r\\n{\\r\\n\\\"shotId\\\": 82520,\\r\\n\\\"scorecardId\\\": 17967,\\r\\n\\\"holeNumber\\\": 5,\\r\\n\\\"shotTime\\\": \\\"2018-02-08T13:09:10.000Z\\\",\\r\\n\\\"clubId\\\": 20823287,\\r\\n\\\"dispersionDistance\\\": -4.57,\\r\\n\\\"shotDistance\\\": 510.522,\\r\\n\\\"fairwayShotOutcome\\\": \\\"HIT\\\"\\r\\n},\\r\\n{\\r\\n\\\"shotId\\\": 82547,\\r\\n\\\"scorecardId\\\": 17967,\\r\\n\\\"holeNumber\\\": 13,\\r\\n\\\"shotTime\\\": \\\"2018-02-08T14:59:50.000Z\\\",\\r\\n\\\"clubId\\\": 20823294,\\r\\n\\\"dispersionDistance\\\": -1.35,\\r\\n\\\"shotDistance\\\": 308.105,\\r\\n\\\"fairwayShotOutcome\\\": \\\"NO_FAIRWAY\\\"\\r\\n},\\r\\n{\\r\\n\\\"shotId\\\": 82554,\\r\\n\\\"scorecardId\\\": 17967,\\r\\n\\\"holeNumber\\\": 15,\\r\\n\\\"shotTime\\\": \\\"2018-02-08T15:27:30.000Z\\\",\\r\\n\\\"clubId\\\": 20823287,\\r\\n\\\"dispersionDistance\\\": -11.57,\\r\\n\\\"shotDistance\\\": 428.434,\\r\\n\\\"fairwayShotOutcome\\\": \\\"HIT\\\"\\r\\n}\\r\\n]\\r\\n}";
	}

	String getMockJSON2 ()
	{
		return "{\n \"numberOfRounds\": 1,\n    \"percentFairwayLeft\": 22.73,\n    \"percentFairwayRight\": 18.18,\n    \"percentFairwayHit\": 59.09,\n    \"minShotDistance\": 175.407,\n    \"maxShotDistance\": 198.445,\n    \"avgShotDistance\": 0,\n    \"minDispersionDistance\": -2.58,\n    \"maxDispersionDistance\": 18.7,\n    \"longestShot\": {\n        \"holeShot\": {\n            \"holeNumber\": 5,\n            \"holeImageUrl\": \"http://birdseye.garmin.com/birdseye/golf/006-d1399-24/gd24500/gid024587/006-d1399-24/0/hole5.jpg?garmindlm=1550009738_084a1ffe5a753390e487a8aa04274cf0\",\n            \"pinPosition\": {\n                \"x\": 413,\n                \"y\": 69\n            },\n            \"shots\": [\n                {\n                    \"id\": 64610,\n                    \"scorecardId\": 17126,\n                    \"playerProfileId\": 1643207,\n                    \"shotTime\": 1516106998000,\n                    \"shotTimeZoneOffset\": 0,\n                    \"clubId\": 0,\n                    \"holeNumber\": 5,\n                    \"autoShotType\": \"USED\",\n                    \"startLoc\": {\n                        \"x\": 360,\n                        \"y\": 725\n                    },\n                    \"endLoc\": {\n                        \"x\": 413,\n                        \"y\": 69\n                    },\n                    \"meters\": 525.804\n                }\n            ]\n        },\n        \"courseSnapshotId\": 1554,\n        \"courseName\": \"El Rompido Golf Club ~ Norte\"\n    },\n    \"shotDispersionDetails\": [\n        " +
		"{\n " +
		"\"shotId\": 64506,\n" +
		"\"scorecardId\": 17124,\n " +
		"\"holeNumber\": 2,\n " +
		"\"shotTime\": \"2018-01-16T15:58:53.000Z\",\n" +
		"\"clubId\": 0,\n" +

		"\"dispersionDistance\": -2.58,\n" +
		"\"shotDistance\": 1.9,\n" +

		"\"fairwayShotOutcome\": \"HIT\"\n" +
		"}," +
		"{\n " +
		"\"shotId\": 64506,\n" +
		"\"scorecardId\": 17124,\n " +
		"\"holeNumber\": 2,\n " +
		"\"shotTime\": \"2018-01-16T15:58:53.000Z\",\n" +
		"\"clubId\": 0,\n" +

		"\"dispersionDistance\": -160.58,\n" +
		"\"shotDistance\": 189.893,\n" +

		"\"fairwayShotOutcome\": \"LEFT\"\n" +
		"}," +

		"{\n \"shotId\": 64517,\n" +
		"\"scorecardId\": 17124,\n" +
		"\"holeNumber\": 6,\n" +
		"\"shotTime\": \"2018-01-16T16:50:04.000Z\",\n" +
		"\"clubId\": 0,\n" +

		"\"dispersionDistance\": -45.81,\n" +
		"\"shotDistance\": 123,\n" +

		"\"fairwayShotOutcome\": \"HIT\"\n" +
		"}," +

		"{\n \"shotId\": 64517,\n" +
		"\"scorecardId\": 17124,\n" +
		"\"holeNumber\": 6,\n" +
		"\"shotTime\": \"2018-01-16T16:50:04.000Z\",\n" +
		"\"clubId\": 0,\n" +

		"\"dispersionDistance\": 5.81,\n" +
		"\"shotDistance\": 194.457,\n" +

		"\"fairwayShotOutcome\": \"HIT\"\n" +
		"}," +
		"{\n \"shotId\": 65182,\n" +
		"\"scorecardId\": 17147,\n" +
		"\"holeNumber\": 1,\n" +
		"\"shotTime\": \"2018-01-16T20:50:25.000Z\",\n" +
		"\"clubId\": 56868,\n" +

		"\"dispersionDistance\": -18.7,\n" +
		"\"shotDistance\": 46.445,\n" +

		"\"fairwayShotOutcome\": \"HIT\"\n" +
		"}" + "]\n}";
	}

	String getMockJSONMin ()
	{
		return "{\n \"numberOfRounds\": 1,\n    \"percentFairwayLeft\": 22.73,\n    \"percentFairwayRight\": 18.18,\n    \"percentFairwayHit\": 59.09,\n    \"minShotDistance\": 175.407,\n    \"maxShotDistance\": 198.445,\n    \"avgShotDistance\": 0,\n    \"minDispersionDistance\": -2.58,\n    \"maxDispersionDistance\": 18.7,\n    \"longestShot\": {\n        \"holeShot\": {\n            \"holeNumber\": 5,\n            \"holeImageUrl\": \"http://birdseye.garmin.com/birdseye/golf/006-d1399-24/gd24500/gid024587/006-d1399-24/0/hole5.jpg?garmindlm=1550009738_084a1ffe5a753390e487a8aa04274cf0\",\n            \"pinPosition\": {\n                \"x\": 413,\n                \"y\": 69\n            },\n            \"shots\": [\n                {\n                    \"id\": 64610,\n                    \"scorecardId\": 17126,\n                    \"playerProfileId\": 1643207,\n                    \"shotTime\": 1516106998000,\n                    \"shotTimeZoneOffset\": 0,\n                    \"clubId\": 0,\n                    \"holeNumber\": 5,\n                    \"autoShotType\": \"USED\",\n                    \"startLoc\": {\n                        \"x\": 360,\n                        \"y\": 725\n                    },\n                    \"endLoc\": {\n                        \"x\": 413,\n                        \"y\": 69\n                    },\n                    \"meters\": 525.804\n                }\n            ]\n        },\n        \"courseSnapshotId\": 1554,\n        \"courseName\": \"El Rompido Golf Club ~ Norte\"\n    },\n    \"shotDispersionDetails\": [\n        " +
		"{\n " +
		"\"shotId\": 64506,\n" +
		"\"scorecardId\": 17124,\n " +
		"\"holeNumber\": 2,\n " +
		"\"shotTime\": \"2018-01-16T15:58:53.000Z\",\n" +
		"\"clubId\": 0,\n" +

		"\"dispersionDistance\": 59.9,\n" +
		"\"shotDistance\": 185.893,\n" +

			"\"fairwayShotOutcome\": \"NO_FAIRWAY\"\n" +
		"}," +
		"{\n " +
		"\"shotId\": 64506,\n" +
		"\"scorecardId\": 17124,\n " +
		"\"holeNumber\": 2,\n " +
		"\"shotTime\": \"2018-01-16T15:58:53.000Z\",\n" +
		"\"clubId\": 0,\n" +

		"\"dispersionDistance\": -62.58,\n" +
		"\"shotDistance\": 179.893,\n" +

		"\"fairwayShotOutcome\": \"LEFT\"\n" +
		"}" +

		"]}";
	}
	/** Shot template
		 * 
		 * 
		 "{\n " +
		"\"shotId\": 64506,\n" +
		"\"scorecardId\": 17124,\n " +
		"\"holeNumber\": 2,\n " +
		"\"shotTime\": \"2018-01-16T15:58:53.000Z\",\n" +
		"\"clubId\": 0,\n" +

		"\"dispersionDistance\": -62.58,\n" +
		"\"shotDistance\": 189.893,\n" +

		"\"fairwayShotOutcome\": \"LEFT\"\n" +
		"}" +
		
		 "{\n " +
			"\"shotId\": 64506,\n" +
			"\"scorecardId\": 17124,\n " +
			"\"holeNumber\": 2,\n " +
			"\"shotTime\": \"2018-01-16T15:58:53.000Z\",\n" +
			"\"clubId\": 0,\n" +

			"\"dispersionDistance\": 22.58,\n" +
			"\"shotDistance\": 56.893,\n" +

			"\"fairwayShotOutcome\": \"RIGHT\"\n" +
			"}"+

		**/
}
