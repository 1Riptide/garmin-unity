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
	private static readonly float[] distanceBounds = new float[]{ -14.0f, 14.0f };
	private static readonly float[] lateralInBoundsDistanceRange = new float[]{ -60f, 60f };
	private static readonly float[] lateralBounds = new float[]{ -9.0f, 9.0f };
	private static float availableDistanceBounds = Math.Abs (distanceBounds [0]) + Math.Abs (distanceBounds [1]);
	// For animating chart markers
	static float bottomOfChartZPosition = -15f;
	float avgShotZOnChart = distanceBounds [0];
	float maxShotZOnChart = distanceBounds [0];
	float minShotZOnChart = bottomOfChartZPosition;

	Vector3 longestShotVector;
	Vector3 averageShotVector;
	Vector3 shortestShotVector;

	// min/max/mean
	int maxY;
	int minY;
	int avgY;
	float transitionSpeed = .8f;
	//Are all shots within 10 yards/meters?
	private static bool isTightRange = false;

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
		Initialize (getMockJSON ());
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
		float zMinBounds = distanceBounds [0];
		float zMaxBounds = distanceBounds [1];
		float xMinBounds = lateralBounds [0];
		float xMaxBounds = lateralBounds [1];

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

		// Log of Z Positions
		float[] shotDistanceZLog = new float[shotCount];
		// Log of distances
		float[] shotDistanceLog = generateShotLog (shotData as JSONNode);
		// General Stats
		float longestShot = shotDistanceLog.Max ();
		float shortestShot = shotDistanceLog.Min ();
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
	
		float maxShotDistanceOnChart = 0f;
		float avgShotDistanceOnChart = 0f;
		float minShotDistanceOnChart = 0f;

		//avgShotZOnChart = Get75th (shotDistanceZLog);
		avgShotDistanceOnChart = Get75th (shotDistanceLog);

		// Plot the shots.
		for (int i = 0; i < shotCount; i++) {
			JSONNode shotDetail = shotData [i];
			if (shotDetail != null) {
				float distance = shotDetail ["shotDistance"]; // should be same as value in shotDistanceLog[i]
				float ZPosition = 0f;
				if (shotCount > 1) {
					// Bottom(min) to Top(max)
					float offsetDistance = distance + negativeZOffset;
					float distanceRatio = offsetDistance / (maxDistance + negativeZOffset);
				
			
					if (isTightRange.Equals (true)) {
						Debug.Log ("AddDataPoints: Tight Range! - all shots are within 10 y/m of each other.");
						distanceRatio = distance / (longestShot);
						ZPosition = (distanceRatio * zMaxBounds) - negativeZOffset;
					} else {
						/** 
						 * We need to scale the shots to fit the available vertical space.
						 * Given this shots distance, we need to calc what percentage of the total range this value represents.
						 * 
						 * A simple way to do this is to offset the range so that the lower limit is 0. If you have the 
						 * range [−14,14] you can add 14 (..or Math.Abs(-14)) to all the ZPositions and consider the 
						 * range to be [0,28]. 
						 * 
						 * To get the percentage of the Zposition of 5, you add 14 to it and find 
						 * the percentage to be 19/28⋅100% ≈ 67.86% 
						 * 
						 * For the inverse, you apply the percentage and apply the offset, so if you are given
						 * 67.86% on the range [−14,14] you find 67.86%⋅28=19. Then remember to subtract 14 to get back to 5.
						 */

						distanceRatio = (distance - shortestShot) / (longestShot - shortestShot);
						ZPosition = (distanceRatio * availableDistanceBounds) - distanceBounds [1];

						// Calculate Avg/75th marker
						if (avgDistanceMarker.activeSelf.Equals (true)) {
							distanceRatio = (avgShotDistanceOnChart + shortestShot) / (longestShot + shortestShot);
							float avgZPosition = (distanceRatio * availableDistanceBounds) - distanceBounds [1];
							averageShotVector = new Vector3 (0, 0, avgZPosition);
							averageText.text = Mathf.Round (avgShotDistanceOnChart).ToString ();
						}
					}
					// Keep track so we can know averages
					shotDistanceZLog [i] = ZPosition; // Position in 2D space log

					// Record for animation in Update()
					maxShotZOnChart = ZPosition > maxShotZOnChart ? ZPosition : maxShotZOnChart;
				
					Debug.Log ("AddDataPoints : minShotZOnChart = " + minShotZOnChart);
					Debug.Log ("AddDataPoints : maxShotZOnChart = " + maxShotZOnChart);

	
					if (minShotZOnChart == bottomOfChartZPosition) {
						minShotDistanceOnChart = distance;
						minShotZOnChart = distanceBounds [0];
						shortestShotVector = new Vector3 (0, 0, minShotZOnChart);
					} else {
						if (distance < minShotDistanceOnChart) {
							minShotDistanceOnChart = distance;
							minShotZOnChart = ZPosition;
							shortestShotVector = new Vector3 (0, 0, minShotZOnChart);
						}
					}
					if (maxShotDistanceOnChart < distance) {
						maxShotDistanceOnChart = distance;
						longestShotVector = new Vector3 (0, 0, maxShotZOnChart);
					}	
					// Update max text
					maxText.text = Mathf.Floor (maxShotDistanceOnChart).ToString ();
					// Update min text
					minText.text = Mathf.Round (minShotDistanceOnChart).ToString ();
				}

				// Left(min) to Right(max)
				float lateralDistance = shotDetail ["dispersionDistance"];
				float XPosition = 0f;
				XPosition = lateralDistance / 6;

				Debug.Log ("\tAddDataPoint : distance = " + distance + " lateralDistance = " + lateralDistance);
		
				// Here is where your shots goes in 3D Space.
				Vector3 dataPointPosition = new Vector3 (XPosition, verticalPosition, ZPosition);
				// Create shot and place where it goes.
				if (lateralDistance <= lateralInBoundsDistanceRange [1] && lateralDistance >= lateralInBoundsDistanceRange [0]) {
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
		if (length == 1) {
			isTightRange = false;
			return floatingList.Max ();
		} else {
			isTightRange = (floatingList.Max () - floatingList.Min ()) <= 10;
			// Keep track if we are working with a tightly grouped collection of shots. Scale as appropriate.
			var pos = 75f * (length + 1) / 100f;
			double fractionalPart = pos - Math.Floor (pos); 
			double absPos = Math.Truncate (pos);
			if (pos < 1) {
				return floatingList.Min ();
			} else if (pos >= length) {
				return floatingList.Max ();
			} else {
				// You better sort that S first!
				floatingList.Sort ();
				float lower = floatingList [(int)absPos - 1];
				float upper = floatingList [(int)absPos];
				float diff = upper - lower;
				return lower + ((float)fractionalPart * diff);
			}
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

	float[] generateShotLog (JSONNode shotData)
	{
		var shotCount = shotData.Count;
		// Log of distances
		float[] shotDistanceLog = new float[shotCount];
		for (int i = 0; i < shotCount; i++) {
			JSONNode shotDetail = shotData [i];
			if (shotDetail != null) {
				shotDistanceLog [i] = shotDetail ["shotDistance"];
			} else {
				shotDistanceLog [i] = 0f;
			}
		}
		return shotDistanceLog;
	}

	String getMockJSON ()
	{
		return "{\\\"numberOfRounds\\\":5,\\\"percentFairwayLeft\\\":18.18,\\\"percentFairwayRight\\\":18.18,\\\"percentFairwayHit\\\":63.64,\\\"minShotDistance\\\":106.244,\\\"maxShotDistance\\\":242.637,\\\"avgShotDistance\\\":223.16,\\\"minDispersionDistance\\\":0.35,\\\"maxDispersionDistance\\\":35.98,\\\"longestShot\\\":{\\\"holeShot\\\":{\\\"holeNumber\\\":16,\\\"holeImageUrl\\\":\\\"http://birdseye.garmin.com/birdseye/golf/006-d2471-23/gd15500/gid015537/006-d2471-23/0/hole16.jpg?garmindlm=1552168253_aa74a1cdbd4d3190b3d785d53122b2b0\\\",\\\"shots\\\":[{\\\"id\\\":74477,\\\"scorecardId\\\":17757,\\\"playerProfileId\\\":1649661,\\\"shotTime\\\":1518385982000,\\\"shotTimeZoneOffset\\\":0,\\\"clubId\\\":20823183,\\\"holeNumber\\\":16,\\\"autoShotType\\\":\\\"USED\\\",\\\"startLoc\\\":{\\\"x\\\":-636,\\\"y\\\":-394},\\\"endLoc\\\":{\\\"x\\\":359,\\\"y\\\":82},\\\"meters\\\":501.827}]},\\\"courseSnapshotId\\\":1597,\\\"courseName\\\":\\\"Aliante Golf Club\\\"},\\\"shotDispersionDetails\\\":[{\\\"shotId\\\":74243,\\\"scorecardId\\\":17754,\\\"holeNumber\\\":1,\\\"shotTime\\\":\\\"2018-01-16T20:50:25.000Z\\\",\\\"clubId\\\":20823167,\\\"dispersionDistance\\\":18.7,\\\"shotDistance\\\":198.445,\\\"fairwayShotOutcome\\\":\\\"HIT\\\"},{\\\"shotId\\\":74246,\\\"scorecardId\\\":17754,\\\"holeNumber\\\":2,\\\"shotTime\\\":\\\"2018-01-16T21:04:33.000Z\\\",\\\"clubId\\\":20823167,\\\"dispersionDistance\\\":15.61,\\\"shotDistance\\\":192.333,\\\"fairwayShotOutcome\\\":\\\"RIGHT\\\"},{\\\"shotId\\\":74252,\\\"scorecardId\\\":17754,\\\"holeNumber\\\":5,\\\"shotTime\\\":\\\"2018-01-16T21:43:16.000Z\\\",\\\"clubId\\\":20823167,\\\"dispersionDistance\\\":8.1,\\\"shotDistance\\\":181.919,\\\"fairwayShotOutcome\\\":\\\"HIT\\\"},{\\\"shotId\\\":74256,\\\"scorecardId\\\":17754,\\\"holeNumber\\\":6,\\\"shotTime\\\":\\\"2018-01-16T21:54:57.000Z\\\",\\\"clubId\\\":20823167,\\\"dispersionDistance\\\":11.36,\\\"shotDistance\\\":182.234,\\\"fairwayShotOutcome\\\":\\\"HIT\\\"},{\\\"shotId\\\":74259,\\\"scorecardId\\\":17754,\\\"holeNumber\\\":7,\\\"shotTime\\\":\\\"2018-01-16T22:08:34.000Z\\\",\\\"clubId\\\":20823167,\\\"dispersionDistance\\\":-0.75,\\\"shotDistance\\\":180.769,\\\"fairwayShotOutcome\\\":\\\"HIT\\\"},{\\\"shotId\\\":74265,\\\"scorecardId\\\":17754,\\\"holeNumber\\\":9,\\\"shotTime\\\":\\\"2018-01-16T22:31:29.000Z\\\",\\\"clubId\\\":20823167,\\\"dispersionDistance\\\":17.69,\\\"shotDistance\\\":170.513,\\\"fairwayShotOutcome\\\":\\\"RIGHT\\\"},{\\\"shotId\\\":74268,\\\"scorecardId\\\":17755,\\\"holeNumber\\\":1,\\\"shotTime\\\":\\\"2018-01-31T16:29:03.000Z\\\",\\\"clubId\\\":20823167,\\\"dispersionDistance\\\":-7.63,\\\"shotDistance\\\":222.581,\\\"fairwayShotOutcome\\\":\\\"HIT\\\"},{\\\"shotId\\\":74272,\\\"scorecardId\\\":17755,\\\"holeNumber\\\":2,\\\"shotTime\\\":\\\"2018-01-31T16:30:05.000Z\\\",\\\"clubId\\\":20823167,\\\"dispersionDistance\\\":27.83,\\\"shotDistance\\\":219.418,\\\"fairwayShotOutcome\\\":\\\"RIGHT\\\"},{\\\"shotId\\\":74276,\\\"scorecardId\\\":17755,\\\"holeNumber\\\":3,\\\"shotTime\\\":\\\"2018-01-31T16:30:47.000Z\\\",\\\"clubId\\\":20823167,\\\"dispersionDistance\\\":-25.31,\\\"shotDistance\\\":234.963,\\\"fairwayShotOutcome\\\":\\\"LEFT\\\"},{\\\"shotId\\\":74280,\\\"scorecardId\\\":17755,\\\"holeNumber\\\":4,\\\"shotTime\\\":\\\"2018-01-31T16:31:34.000Z\\\",\\\"clubId\\\":20823167,\\\"dispersionDistance\\\":7.8,\\\"shotDistance\\\":230.884,\\\"fairwayShotOutcome\\\":\\\"HIT\\\"},{\\\"shotId\\\":74288,\\\"scorecardId\\\":17755,\\\"holeNumber\\\":6,\\\"shotTime\\\":\\\"2018-01-31T16:33:20.000Z\\\",\\\"clubId\\\":20823167,\\\"dispersionDistance\\\":31.22,\\\"shotDistance\\\":223.194,\\\"fairwayShotOutcome\\\":\\\"RIGHT\\\"},{\\\"shotId\\\":74292,\\\"scorecardId\\\":17755,\\\"holeNumber\\\":7,\\\"shotTime\\\":\\\"2018-01-31T16:34:28.000Z\\\",\\\"clubId\\\":20823167,\\\"dispersionDistance\\\":-15.02,\\\"shotDistance\\\":231.716,\\\"fairwayShotOutcome\\\":\\\"HIT\\\"},{\\\"shotId\\\":74299,\\\"scorecardId\\\":17755,\\\"holeNumber\\\":9,\\\"shotTime\\\":\\\"2018-01-31T16:36:04.000Z\\\",\\\"clubId\\\":20823167,\\\"dispersionDistance\\\":0.35,\\\"shotDistance\\\":242.637,\\\"fairwayShotOutcome\\\":\\\"HIT\\\"},{\\\"shotId\\\":74303,\\\"scorecardId\\\":17755,\\\"holeNumber\\\":10,\\\"shotTime\\\":\\\"2018-01-31T16:37:16.000Z\\\",\\\"clubId\\\":20823167,\\\"dispersionDistance\\\":3.07,\\\"shotDistance\\\":220.915,\\\"fairwayShotOutcome\\\":\\\"HIT\\\"},{\\\"shotId\\\":74307,\\\"scorecardId\\\":17755,\\\"holeNumber\\\":11,\\\"shotTime\\\":\\\"2018-01-31T16:37:59.000Z\\\",\\\"clubId\\\":20823167,\\\"dispersionDistance\\\":10.1,\\\"shotDistance\\\":226.4,\\\"fairwayShotOutcome\\\":\\\"HIT\\\"},{\\\"shotId\\\":74311,\\\"scorecardId\\\":17755,\\\"holeNumber\\\":12,\\\"shotTime\\\":\\\"2018-01-31T16:38:37.000Z\\\",\\\"clubId\\\":20823167,\\\"dispersionDistance\\\":-21.35,\\\"shotDistance\\\":220.549,\\\"fairwayShotOutcome\\\":\\\"LEFT\\\"},{\\\"shotId\\\":74316,\\\"scorecardId\\\":17755,\\\"holeNumber\\\":13,\\\"shotTime\\\":\\\"2018-01-31T16:40:27.000Z\\\",\\\"clubId\\\":20823167,\\\"dispersionDistance\\\":29.45,\\\"shotDistance\\\":220.803,\\\"fairwayShotOutcome\\\":\\\"RIGHT\\\"},{\\\"shotId\\\":74322,\\\"scorecardId\\\":17755,\\\"holeNumber\\\":15,\\\"shotTime\\\":\\\"2018-01-31T16:41:43.000Z\\\",\\\"clubId\\\":20823167,\\\"dispersionDistance\\\":35.98,\\\"shotDistance\\\":237.58,\\\"fairwayShotOutcome\\\":\\\"RIGHT\\\"},{\\\"shotId\\\":74331,\\\"scorecardId\\\":17755,\\\"holeNumber\\\":17,\\\"shotTime\\\":\\\"2018-01-31T16:43:48.000Z\\\",\\\"clubId\\\":20823167,\\\"dispersionDistance\\\":-1.31,\\\"shotDistance\\\":223.066,\\\"fairwayShotOutcome\\\":\\\"HIT\\\"},{\\\"shotId\\\":74334,\\\"scorecardId\\\":17755,\\\"holeNumber\\\":18,\\\"shotTime\\\":\\\"2018-01-31T16:44:22.000Z\\\",\\\"clubId\\\":20823167,\\\"dispersionDistance\\\":4.28,\\\"shotDistance\\\":212.086,\\\"fairwayShotOutcome\\\":\\\"HIT\\\"},{\\\"shotId\\\":74339,\\\"scorecardId\\\":17756,\\\"holeNumber\\\":1,\\\"shotTime\\\":\\\"2018-01-31T17:09:01.000Z\\\",\\\"clubId\\\":20823167,\\\"dispersionDistance\\\":12.21,\\\"shotDistance\\\":229.721,\\\"fairwayShotOutcome\\\":\\\"RIGHT\\\"},{\\\"shotId\\\":74346,\\\"scorecardId\\\":17756,\\\"holeNumber\\\":3,\\\"shotTime\\\":\\\"2018-01-31T17:10:26.000Z\\\",\\\"clubId\\\":20823167,\\\"dispersionDistance\\\":-3.59,\\\"shotDistance\\\":219.586,\\\"fairwayShotOutcome\\\":\\\"HIT\\\"},{\\\"shotId\\\":74352,\\\"scorecardId\\\":17756,\\\"holeNumber\\\":5,\\\"shotTime\\\":\\\"2018-01-31T17:11:39.000Z\\\",\\\"clubId\\\":20823167,\\\"dispersionDistance\\\":-22.68,\\\"shotDistance\\\":203.054,\\\"fairwayShotOutcome\\\":\\\"LEFT\\\"},{\\\"shotId\\\":74360,\\\"scorecardId\\\":17756,\\\"holeNumber\\\":7,\\\"shotTime\\\":\\\"2018-01-31T17:13:34.000Z\\\",\\\"clubId\\\":20823167,\\\"dispersionDistance\\\":5.27,\\\"shotDistance\\\":213.266,\\\"fairwayShotOutcome\\\":\\\"HIT\\\"},{\\\"shotId\\\":74364,\\\"scorecardId\\\":17756,\\\"holeNumber\\\":8,\\\"shotTime\\\":\\\"2018-01-31T17:14:39.000Z\\\",\\\"clubId\\\":20823167,\\\"dispersionDistance\\\":-18.83,\\\"shotDistance\\\":227.311,\\\"fairwayShotOutcome\\\":\\\"HIT\\\"},{\\\"shotId\\\":74368,\\\"scorecardId\\\":17756,\\\"holeNumber\\\":9,\\\"shotTime\\\":\\\"2018-01-31T17:15:24.000Z\\\",\\\"clubId\\\":20823167,\\\"dispersionDistance\\\":-4.49,\\\"shotDistance\\\":217.62,\\\"fairwayShotOutcome\\\":\\\"HIT\\\"},{\\\"shotId\\\":74381,\\\"scorecardId\\\":17756,\\\"holeNumber\\\":12,\\\"shotTime\\\":\\\"2018-01-31T17:18:00.000Z\\\",\\\"clubId\\\":20823167,\\\"dispersionDistance\\\":3.93,\\\"shotDistance\\\":217.872,\\\"fairwayShotOutcome\\\":\\\"HIT\\\"},{\\\"shotId\\\":74384,\\\"scorecardId\\\":17756,\\\"holeNumber\\\":13,\\\"shotTime\\\":\\\"2018-01-31T17:18:44.000Z\\\",\\\"clubId\\\":20823167,\\\"dispersionDistance\\\":-29.77,\\\"shotDistance\\\":225.773,\\\"fairwayShotOutcome\\\":\\\"LEFT\\\"},{\\\"shotId\\\":74388,\\\"scorecardId\\\":17756,\\\"holeNumber\\\":14,\\\"shotTime\\\":\\\"2018-01-31T17:19:27.000Z\\\",\\\"clubId\\\":20823167,\\\"dispersionDistance\\\":14.27,\\\"shotDistance\\\":218.266,\\\"fairwayShotOutcome\\\":\\\"RIGHT\\\"},{\\\"shotId\\\":74392,\\\"scorecardId\\\":17756,\\\"holeNumber\\\":15,\\\"shotTime\\\":\\\"2018-01-31T17:20:17.000Z\\\",\\\"clubId\\\":20823167,\\\"dispersionDistance\\\":6.82,\\\"shotDistance\\\":224.614,\\\"fairwayShotOutcome\\\":\\\"HIT\\\"},{\\\"shotId\\\":74399,\\\"scorecardId\\\":17756,\\\"holeNumber\\\":17,\\\"shotTime\\\":\\\"2018-01-31T17:22:04.000Z\\\",\\\"clubId\\\":20823167,\\\"dispersionDistance\\\":-1.74,\\\"shotDistance\\\":219.6,\\\"fairwayShotOutcome\\\":\\\"HIT\\\"},{\\\"shotId\\\":74403,\\\"scorecardId\\\":17756,\\\"holeNumber\\\":18,\\\"shotTime\\\":\\\"2018-01-31T17:22:45.000Z\\\",\\\"clubId\\\":20823167,\\\"dispersionDistance\\\":9.31,\\\"shotDistance\\\":216.304,\\\"fairwayShotOutcome\\\":\\\"HIT\\\"},{\\\"shotId\\\":74500,\\\"scorecardId\\\":17758,\\\"holeNumber\\\":5,\\\"shotTime\\\":\\\"2018-02-09T19:27:18.000Z\\\",\\\"clubId\\\":20823044,\\\"dispersionDistance\\\":12.54,\\\"shotDistance\\\":208.895,\\\"fairwayShotOutcome\\\":\\\"HIT\\\"},{\\\"shotId\\\":74504,\\\"scorecardId\\\":17758,\\\"holeNumber\\\":6,\\\"shotTime\\\":\\\"2018-02-09T19:41:05.000Z\\\",\\\"clubId\\\":20823044,\\\"dispersionDistance\\\":-4.62,\\\"shotDistance\\\":179.853,\\\"fairwayShotOutcome\\\":\\\"HIT\\\"},{\\\"shotId\\\":74521,\\\"scorecardId\\\":17758,\\\"holeNumber\\\":11,\\\"shotTime\\\":\\\"2018-02-09T20:57:36.000Z\\\",\\\"clubId\\\":20823044,\\\"dispersionDistance\\\":-24.35,\\\"shotDistance\\\":186.41,\\\"fairwayShotOutcome\\\":\\\"LEFT\\\"},{\\\"shotId\\\":74530,\\\"scorecardId\\\":17758,\\\"holeNumber\\\":13,\\\"shotTime\\\":\\\"2018-02-09T21:34:26.000Z\\\",\\\"clubId\\\":20823044,\\\"dispersionDistance\\\":13.77,\\\"shotDistance\\\":180.167,\\\"fairwayShotOutcome\\\":\\\"HIT\\\"},{\\\"shotId\\\":74539,\\\"scorecardId\\\":17758,\\\"holeNumber\\\":15,\\\"shotTime\\\":\\\"2018-02-09T21:59:17.000Z\\\",\\\"clubId\\\":0,\\\"dispersionDistance\\\":-7.21,\\\"shotDistance\\\":141.847,\\\"fairwayShotOutcome\\\":\\\"HIT\\\"},{\\\"shotId\\\":74544,\\\"scorecardId\\\":17758,\\\"holeNumber\\\":16,\\\"shotTime\\\":\\\"2018-02-09T22:16:11.000Z\\\",\\\"clubId\\\":20823044,\\\"dispersionDistance\\\":7.92,\\\"shotDistance\\\":193.514,\\\"fairwayShotOutcome\\\":\\\"HIT\\\"},{\\\"shotId\\\":74424,\\\"scorecardId\\\":17757,\\\"holeNumber\\\":5,\\\"shotTime\\\":\\\"2018-02-11T18:40:04.000Z\\\",\\\"clubId\\\":20823045,\\\"dispersionDistance\\\":-10.38,\\\"shotDistance\\\":165.731,\\\"fairwayShotOutcome\\\":\\\"HIT\\\"},{\\\"shotId\\\":74429,\\\"scorecardId\\\":17757,\\\"holeNumber\\\":7,\\\"shotTime\\\":\\\"2018-02-11T19:05:59.000Z\\\",\\\"clubId\\\":20823044,\\\"dispersionDistance\\\":-1.54,\\\"shotDistance\\\":205.483,\\\"fairwayShotOutcome\\\":\\\"HIT\\\"},{\\\"shotId\\\":74445,\\\"scorecardId\\\":17757,\\\"holeNumber\\\":11,\\\"shotTime\\\":\\\"2018-02-11T20:14:29.000Z\\\",\\\"clubId\\\":20823044,\\\"dispersionDistance\\\":-19.18,\\\"shotDistance\\\":106.244,\\\"fairwayShotOutcome\\\":\\\"LEFT\\\"},{\\\"shotId\\\":74449,\\\"scorecardId\\\":17757,\\\"holeNumber\\\":12,\\\"shotTime\\\":\\\"2018-02-11T20:32:39.000Z\\\",\\\"clubId\\\":20823044,\\\"dispersionDistance\\\":-32.46,\\\"shotDistance\\\":204.151,\\\"fairwayShotOutcome\\\":\\\"LEFT\\\"},{\\\"shotId\\\":74461,\\\"scorecardId\\\":17757,\\\"holeNumber\\\":14,\\\"shotTime\\\":\\\"2018-02-11T21:00:29.000Z\\\",\\\"clubId\\\":20823044,\\\"dispersionDistance\\\":9.99,\\\"shotDistance\\\":191.661,\\\"fairwayShotOutcome\\\":\\\"HIT\\\"},{\\\"shotId\\\":74478,\\\"scorecardId\\\":17757,\\\"holeNumber\\\":18,\\\"shotTime\\\":\\\"2018-02-11T21:57:05.000Z\\\",\\\"clubId\\\":20823044,\\\"dispersionDistance\\\":-32.11,\\\"shotDistance\\\":200.751,\\\"fairwayShotOutcome\\\":\\\"LEFT\\\"}]}";
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
		"\"shotDistance\": 150,\n" +

		"\"fairwayShotOutcome\": \"HIT\"\n" +
		"}," +
		"{\n " +
		"\"shotId\": 64506,\n" +
		"\"scorecardId\": 17124,\n " +
		"\"holeNumber\": 2,\n " +
		"\"shotTime\": \"2018-01-16T15:58:53.000Z\",\n" +
		"\"clubId\": 0,\n" +

		"\"dispersionDistance\": -160.58,\n" +
		"\"shotDistance\": 175,\n" +

		"\"fairwayShotOutcome\": \"LEFT\"\n" +
		"}," +

		"{\n \"shotId\": 64517,\n" +
		"\"scorecardId\": 17124,\n" +
		"\"holeNumber\": 6,\n" +
		"\"shotTime\": \"2018-01-16T16:50:04.000Z\",\n" +
		"\"clubId\": 0,\n" +

		"\"dispersionDistance\": -45.81,\n" +
		"\"shotDistance\": 200,\n" +

		"\"fairwayShotOutcome\": \"HIT\"\n" +
		"}," +

		"{\n \"shotId\": 64517,\n" +
		"\"scorecardId\": 17124,\n" +
		"\"holeNumber\": 6,\n" +
		"\"shotTime\": \"2018-01-16T16:50:04.000Z\",\n" +
		"\"clubId\": 0,\n" +

		"\"dispersionDistance\": 5.81,\n" +
		"\"shotDistance\": 225,\n" +

		"\"fairwayShotOutcome\": \"HIT\"\n" +
		"}," +
		"{" +
		"\"shotId\": 64506,\n" +
		"\"scorecardId\": 17124,\n " +
		"\"holeNumber\": 2,\n " +
		"\"shotTime\": \"2018-01-16T15:58:53.000Z\",\n" +
		"\"clubId\": 0,\n" +

		"\"dispersionDistance\": -2.58,\n" +
		"\"shotDistance\": 150,\n" +

		"\"fairwayShotOutcome\": \"HIT\"\n" +
		"}," +
		"{\n " +
		"\"shotId\": 64506,\n" +
		"\"scorecardId\": 17124,\n " +
		"\"holeNumber\": 2,\n " +
		"\"shotTime\": \"2018-01-16T15:58:53.000Z\",\n" +
		"\"clubId\": 0,\n" +

		"\"dispersionDistance\": -160.58,\n" +
		"\"shotDistance\": 175,\n" +

		"\"fairwayShotOutcome\": \"LEFT\"\n" +
		"}," +

		"{\n \"shotId\": 64517,\n" +
		"\"scorecardId\": 17124,\n" +
		"\"holeNumber\": 6,\n" +
		"\"shotTime\": \"2018-01-16T16:50:04.000Z\",\n" +
		"\"clubId\": 0,\n" +

		"\"dispersionDistance\": -45.81,\n" +
		"\"shotDistance\": 200,\n" +

		"\"fairwayShotOutcome\": \"HIT\"\n" +
		"}," +

		"{\n \"shotId\": 64517,\n" +
		"\"scorecardId\": 17124,\n" +
		"\"holeNumber\": 6,\n" +
		"\"shotTime\": \"2018-01-16T16:50:04.000Z\",\n" +
		"\"clubId\": 0,\n" +

		"\"dispersionDistance\": 5.81,\n" +
		"\"shotDistance\": 225,\n" +

		"\"fairwayShotOutcome\": \"HIT\"\n" +
		"},"
		+
		"{" +
		"\"shotId\": 64506,\n" +
		"\"scorecardId\": 17124,\n " +
		"\"holeNumber\": 2,\n " +
		"\"shotTime\": \"2018-01-16T15:58:53.000Z\",\n" +
		"\"clubId\": 0,\n" +

		"\"dispersionDistance\": -2.58,\n" +
		"\"shotDistance\": 150,\n" +

		"\"fairwayShotOutcome\": \"HIT\"\n" +
		"}," +
		"{\n " +
		"\"shotId\": 64506,\n" +
		"\"scorecardId\": 17124,\n " +
		"\"holeNumber\": 2,\n " +
		"\"shotTime\": \"2018-01-16T15:58:53.000Z\",\n" +
		"\"clubId\": 0,\n" +

		"\"dispersionDistance\": -160.58,\n" +
		"\"shotDistance\": 175,\n" +

		"\"fairwayShotOutcome\": \"LEFT\"\n" +
		"}," +

		"{\n \"shotId\": 64517,\n" +
		"\"scorecardId\": 17124,\n" +
		"\"holeNumber\": 6,\n" +
		"\"shotTime\": \"2018-01-16T16:50:04.000Z\",\n" +
		"\"clubId\": 0,\n" +

		"\"dispersionDistance\": -45.81,\n" +
		"\"shotDistance\": 200,\n" +

		"\"fairwayShotOutcome\": \"HIT\"\n" +
		"}," +

		"{\n \"shotId\": 64517,\n" +
		"\"scorecardId\": 17124,\n" +
		"\"holeNumber\": 6,\n" +
		"\"shotTime\": \"2018-01-16T16:50:04.000Z\",\n" +
		"\"clubId\": 0,\n" +

		"\"dispersionDistance\": 5.81,\n" +
		"\"shotDistance\": 225,\n" +

		"\"fairwayShotOutcome\": \"HIT\"\n" +
		"},"
		+
		"{" +
		"\"shotId\": 64506,\n" +
		"\"scorecardId\": 17124,\n " +
		"\"holeNumber\": 2,\n " +
		"\"shotTime\": \"2018-01-16T15:58:53.000Z\",\n" +
		"\"clubId\": 0,\n" +

		"\"dispersionDistance\": -2.58,\n" +
		"\"shotDistance\": 150,\n" +

		"\"fairwayShotOutcome\": \"HIT\"\n" +
		"}," +
		"{\n " +
		"\"shotId\": 64506,\n" +
		"\"scorecardId\": 17124,\n " +
		"\"holeNumber\": 2,\n " +
		"\"shotTime\": \"2018-01-16T15:58:53.000Z\",\n" +
		"\"clubId\": 0,\n" +

		"\"dispersionDistance\": -160.58,\n" +
		"\"shotDistance\": 175,\n" +

		"\"fairwayShotOutcome\": \"LEFT\"\n" +
		"}," +

		"{\n \"shotId\": 64517,\n" +
		"\"scorecardId\": 17124,\n" +
		"\"holeNumber\": 6,\n" +
		"\"shotTime\": \"2018-01-16T16:50:04.000Z\",\n" +
		"\"clubId\": 0,\n" +

		"\"dispersionDistance\": -45.81,\n" +
		"\"shotDistance\": 200,\n" +

		"\"fairwayShotOutcome\": \"HIT\"\n" +
		"}," +

		"{\n \"shotId\": 64517,\n" +
		"\"scorecardId\": 17124,\n" +
		"\"holeNumber\": 6,\n" +
		"\"shotTime\": \"2018-01-16T16:50:04.000Z\",\n" +
		"\"clubId\": 0,\n" +

		"\"dispersionDistance\": 5.81,\n" +
		"\"shotDistance\": 225,\n" +

		"\"fairwayShotOutcome\": \"HIT\"\n" +
		"},"
		+ "]\n}";
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

		"\"dispersionDistance\": 60.01,\n" +
		"\"shotDistance\": 185.893,\n" +

		"\"fairwayShotOutcome\": \"NO_FAIRWAY\"\n" +
		"}," +


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
