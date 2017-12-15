using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

public class DriveChart : MonoBehaviour, IGarmin3DChart {

	enum ShotOutcomes {HIT, LEFT, RIGHT, NO_FAIRWAY};
	// Default shot object.
	public GameObject whiteDataPoint;
	public GameObject redDataPoint;
	public static Stack<GameObject> dataPoints = new Stack<GameObject>();
	// chart markers for distances
	public GameObject maxDistanceMarker;
	public GameObject avgDistanceMarker;
	public GameObject minDistanceMarker;

	TextMesh averageText;
	TextMesh maxText;
	TextMesh minText;

	// For animating chart markers
	float bottomOfChartZPosition = -15f;
	float avgShotZOnChart;
	float maxShotZOnChart;
	float minShotZOnChart;
	int maxShotDistanceOnChart = -1;
	int avgShotDistanceOnChart = -1;
	int minShotDistanceOnChart = -1;

	Vector3 longestShotVector;
	Vector3 averageShotVector;
	Vector3 shortestShotVector;
	// Used to devise a ratio with witch we plot datapoints based on real world distances.
	private static readonly float[] DistanceBounds = new float[]{-14.0f, 14.0f};
	private static readonly float[] LateralBounds = new float[]{-9.0f, 9.0f};

	// min/max/mean
	int maxY;
	int minY;
	int avgY;
	float transitionSpeed = .8f;

	private bool isInitialized = false;
	public bool isEnabled {get; set;}


	// Genesis
	void Start () {
		maxText = (TextMesh)maxDistanceMarker.GetComponentInChildren(typeof(TextMesh))as TextMesh;
		averageText = (TextMesh)avgDistanceMarker.GetComponentInChildren(typeof(TextMesh))as TextMesh;
		minText = (TextMesh)minDistanceMarker.GetComponentInChildren(typeof(TextMesh))as TextMesh;
		// This must be called by external platform. Pass JSON.
		Initialize(getMockJSON());
	}

	// Looper - runs (n)times a second depending on framerate.
	void Update () {
		 if(isInitialized) {
				avgDistanceMarker.transform.position = Vector3.Lerp (avgDistanceMarker.transform.position, averageShotVector, transitionSpeed * Time.deltaTime);	
				minDistanceMarker.transform.position = Vector3.Lerp (minDistanceMarker.transform.position, shortestShotVector, transitionSpeed * Time.deltaTime);	
				maxDistanceMarker.transform.position = Vector3.Lerp (maxDistanceMarker.transform.position, longestShotVector, transitionSpeed * Time.deltaTime);	
	
		} else {
			Debug.Log ("Update isEnabled = " + isEnabled + " isInitialized " + isInitialized);
			maxDistanceMarker.active = false;
			minDistanceMarker.active = false;
			avgDistanceMarker.active = false;
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
			avgShotZOnChart = GetMedian (shotDistanceLog);
			maxShotZOnChart = distance > maxShotZOnChart ? distance : maxShotZOnChart;
			if (i == 0) {
				minShotZOnChart = distance;
			} else {
				minShotZOnChart = distance < minShotZOnChart ? distance : minShotZOnChart;
			}
			// Update shot vectors. Used to animate shot markers.
			longestShotVector = new Vector3 (0, 0, maxShotZOnChart);
			averageShotVector = new Vector3 (0, 0, avgShotZOnChart);
			shortestShotVector = new Vector3 (0, 0, minShotZOnChart);

			dataPoints.Push(AddDataPoint(whiteDataPoint, new Vector3(lateralPosition, verticalPosition, distance)));
			// Stall the loop for aesthetics as shots drop.
			yield return new WaitForSeconds(0);
		}
	}

	IEnumerator AddDataPoints(String json){
		var clubTrackDriveData = JSON.Parse(json);
		var shotData = clubTrackDriveData["shotDispersionDetails"];
		var shotCount = shotData.Count;
		Debug.Log ("AddDataPoints shotData count  " + shotCount);

		// Get the min and max ranges for both length (distance) and width (lateral distance) in yards/meters
		float maxDistance = clubTrackDriveData["maxShotDistance"];
		float minDistance = clubTrackDriveData["minShotDistance"];
		float maxLateralDistance = clubTrackDriveData["maxDispersionDistance"];
		float minLateralDistance = clubTrackDriveData["minDispersionDistance"];

		float distanceRange = maxDistance - minDistance;
		float lateralRange = maxLateralDistance - minLateralDistance;

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

		float negativeZOffset = Math.Abs(zMinBounds);
		float negativeXOffset = Math.Abs(xMinBounds);

		// Log of Z Positions
		float[] shotDistanceZLog = new float[shotCount];
		// Log of distances
		float[] shotDistanceLog = new float[shotCount];

		// Since we are plotting shots in 2D space (on a plane), we dont need to keep calculating in loop below.
		float verticalPosition = 0f;
	
		// Plot the shots.
		for (int i = 0; i < shotCount; i++) {
			
			JSONNode shotDetail = clubTrackDriveData ["shotDispersionDetails"][i];
			Debug.Log ("shotDetail = " + shotDetail.ToString() + " count = " + i);
			if (shotDetail != null) {
				float distance = shotDetail["shotDistance"];
				if (distance != null) {

					if (maxShotDistanceOnChart != -1) {
						if (maxShotDistanceOnChart < distance) {
							maxShotDistanceOnChart = (int)distance;
						}	
					} else {
						maxShotDistanceOnChart = (int)distance;
					}

					if (minShotDistanceOnChart != -1) {
						if (minShotDistanceOnChart > distance) {
							minShotDistanceOnChart = (int)distance;
						}
					} else {
						minShotDistanceOnChart = (int)distance;
					}


					// Bottom(min) to Top(max)
					float offsetDistance = distance + negativeZOffset;
					float distanceRatio = offsetDistance / (maxDistance + negativeZOffset);
					float ZPositon = (distanceRatio * (zMaxBounds + negativeZOffset)) - negativeZOffset;

					// Keep track so we can know averages
					shotDistanceZLog [i] = ZPositon;
					shotDistanceLog [i] = distance;
					// continually calculate these values(for animation).
					avgShotZOnChart = GetMedian (shotDistanceZLog);
					avgShotDistanceOnChart = (int) GetMedian (shotDistanceLog);

					maxShotZOnChart = maxShotZOnChart != null ? (ZPositon > maxShotZOnChart) ? ZPositon : maxShotZOnChart : ZPositon;

					if (i == 0) {
						minShotZOnChart = ZPositon;
					} else {
						minShotZOnChart = ZPositon < minShotZOnChart ? ZPositon : minShotZOnChart;
					}

					// Left(min) to Right(max)
					float lateralDistance = shotDetail ["dispersionDistance"];
					float offsetLateralDistance = lateralDistance + negativeXOffset;
					float lateralRatio = offsetLateralDistance / (maxLateralDistance + negativeXOffset);
					float XPosition = (lateralRatio * (xMaxBounds + negativeXOffset)) - negativeXOffset;

					// Update shot vectors. Used to animate shot markers.
					longestShotVector = new Vector3 (0, 0, maxShotZOnChart);
					averageShotVector = new Vector3 (0, 0, avgShotZOnChart);
					shortestShotVector = new Vector3 (0, 0, minShotZOnChart);
					/**
					 * Squeeze all shots into available lateral space. Shots that are out of range 
					 * will stick to the outside edge of the plane on either the right or left side.
					 */ 
					if (XPosition > xMaxBounds) {
						XPosition = xMaxBounds + 1f;
						dataPoints.Push (AddDataPoint (redDataPoint, new Vector3 (XPosition, verticalPosition, ZPositon)));
					} else if (XPosition < xMinBounds) {
						XPosition = xMinBounds - 1f;
						dataPoints.Push (AddDataPoint (redDataPoint, new Vector3 (XPosition, verticalPosition, ZPositon)));
					} else {
						dataPoints.Push (AddDataPoint (whiteDataPoint, new Vector3 (XPosition, verticalPosition, ZPositon)));
					}

					// Update markers
					maxText.text = maxShotDistanceOnChart.ToString();
					averageText.text = avgShotDistanceOnChart.ToString();
					minText.text = minShotDistanceOnChart.ToString();

				} else {
					Debug.Log ("distance is null. Skipping datapoint");
				}
			} else {
				Debug.Log ("AddDataPoints ShotData is null! = " + shotData);

			}
			// Stall the loop for aesthetics as shots drop.
			yield return new WaitForSeconds(0);
		}
	}

	GameObject AddDataPoint(GameObject dataPoint, Vector3 location){
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

	void Cleanup(){
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
	public void Initialize(String json){
		isInitialized = true;
		Debug.Log ("Initialize() json = " + json);
		Cleanup();
		var shotCount = 0;
		if (json == null || json.Length == 0) {
			// Get shot Count from JSON
			shotCount = 87;
			try{
				StartCoroutine (AddDataPoints ());
			}catch(Exception e){
				Debug.Log ("Exception calling AddDataPoints : " + e);
			}
		} else {
			try{
				Debug.Log ("Initialize() json is not null. Casting to JSON obj...");
				StartCoroutine(AddDataPoints(json));
			}catch(Exception e){
				Debug.Log ("Exception parsing JSON : " + e);
			}
		}
	}

	String getMockJSON() {
		return "{\n" + "  \"numberOfRounds\": 0,\n" + "  \"percentFairwayLeft\": 0,\n" + "  \"percentFairwayRight\": 0,\n" +
			"  \"percentFairwayHit\": 0,\n" + "  \"minShotDistance\": 30,\n" + "  \"maxShotDistance\": 263,\n" +
			"  \"avgShotDistance\": 0,\n" + "  \"minDispersionDistance\": 2,\n" + "  \"maxDispersionDistance\": 80,\n" +
			"  \"shotDispersionDetails\": [\n" + "    {\n" + "      \"shotId\": 0,\n" + "      \"scorecardId\": 0,\n" +
			"      \"holeNumber\": 2,\n" + "      \"shotTime\": \"2017-12-07\",\n" + "      \"clubId\": 53856,\n" +
			"      \"dispersionDistance\": 80,\n" + "      \"shotDistance\": 190,\n" + "      \"fairwayShotOutcome\": \"HIT\"\n" +
			"    },\n" + "    {\n" + "      \"shotId\": 1,\n" + "      \"scorecardId\": 0,\n" + "      \"holeNumber\": 3,\n" +
			"      \"shotTime\": \"2017-12-07\",\n" + "      \"clubId\": 53843,\n" + "      \"dispersionDistance\": 200,\n" +
			"      \"shotDistance\": 30,\n" + "      \"fairwayShotOutcome\": \"RIGHT\"\n" + "    },\n" + "    {\n" +
			"      \"shotId\": 2,\n" + "      \"scorecardId\": 0,\n" + "      \"holeNumber\": 4,\n" +
			"      \"shotTime\": \"2017-12-07\",\n" + "      \"clubId\": 53856,\n" + "      \"dispersionDistance\": 10,\n" +
			"      \"shotDistance\": 263,\n" + "      \"fairwayShotOutcome\": \"HIT\"\n" + "    },\n" + "    {\n" +
			"      \"shotId\": 3,\n" + "      \"scorecardId\": 0,\n" + "      \"holeNumber\": 5,\n" +
			"      \"shotTime\": \"2017-12-07\",\n" + "      \"clubId\": 53846,\n" + "      \"dispersionDistance\": 40,\n" +
			"      \"shotDistance\": 250,\n" + "      \"fairwayShotOutcome\": \"NO_FAIRWAY\"\n" + "    },\n" + "    {\n" +
			"      \"shotId\": 4,\n" + "      \"scorecardId\": 0,\n" + "      \"holeNumber\": 6,\n" +
			"      \"shotTime\": \"2017-12-07\",\n" + "      \"clubId\": 53844,\n" + "      \"dispersionDistance\": -80,\n" +
			"      \"shotDistance\": 172,\n" + "      \"fairwayShotOutcome\": \"HIT\"\n" + "    },\n" + "    {\n" +
			"      \"shotId\": 5,\n" + "      \"scorecardId\": 0,\n" + "      \"holeNumber\": 7,\n" +
			"      \"shotTime\": \"2017-12-07\",\n" + "      \"clubId\": 53857,\n" + "      \"dispersionDistance\": 10,\n" +
			"      \"shotDistance\": 209,\n" + "      \"fairwayShotOutcome\": \"HIT\"\n" + "    },\n" + "    {\n" +
			"      \"shotId\": 6,\n" + "      \"scorecardId\": 0,\n" + "      \"holeNumber\": 10,\n" +
			"      \"shotTime\": \"2017-12-07\",\n" + "      \"clubId\": 53843,\n" + "      \"dispersionDistance\": 6,\n" +
			"      \"shotDistance\": 194,\n" + "      \"fairwayShotOutcome\": \"HIT\"\n" + "    },\n" + "    {\n" +
			"      \"shotId\": 7,\n" + "      \"scorecardId\": 0,\n" + "      \"holeNumber\": 11,\n" +
			"      \"shotTime\": \"2017-12-07\",\n" + "      \"clubId\": 53856,\n" + "      \"dispersionDistance\": 60,\n" +
			"      \"shotDistance\": 230,\n" + "      \"fairwayShotOutcome\": \"LEFT\"\n" + "    },\n" + "    {\n" +
			"      \"shotId\": 8,\n" + "      \"scorecardId\": 0,\n" + "      \"holeNumber\": 17,\n" +
			"      \"shotTime\": \"2017-12-07\",\n" + "      \"clubId\": 53857,\n" + "      \"dispersionDistance\": 4,\n" +
			"      \"shotDistance\": 231,\n" + "      \"fairwayShotOutcome\": \"HIT\"\n" + "    },\n" + "    {\n" +
			"      \"shotId\": 9,\n" + "      \"scorecardId\": 1,\n" + "      \"holeNumber\": 3,\n" +
			"      \"shotTime\": \"2017-12-08\",\n" + "      \"clubId\": 53856,\n" + "      \"dispersionDistance\": 60,\n" +
			"      \"shotDistance\": 73,\n" + "      \"fairwayShotOutcome\": \"NO_FAIRWAY\"\n" + "    },\n" + "    {\n" +
			"      \"shotId\": 10,\n" + "      \"scorecardId\": 1,\n" + "      \"holeNumber\": 4,\n" +
			"      \"shotTime\": \"2017-12-08\",\n" + "      \"clubId\": 53857,\n" + "      \"dispersionDistance\": 40,\n" +
			"      \"shotDistance\": 204,\n" + "      \"fairwayShotOutcome\": \"LEFT\"\n" + "    },\n" + "    {\n" +
			"      \"shotId\": 16,\n" + "      \"scorecardId\": 1,\n" + "      \"holeNumber\": 6,\n" +
			"      \"shotTime\": \"2017-12-08\",\n" + "      \"clubId\": 53843,\n" + "      \"dispersionDistance\": 20,\n" +
			"      \"shotDistance\": 243,\n" + "      \"fairwayShotOutcome\": \"RIGHT\"\n" + "    },\n" + "    {\n" +
			"      \"shotId\": 17,\n" + "      \"scorecardId\": 2,\n" + "      \"holeNumber\": 1,\n" +
			"      \"shotTime\": \"2017-12-09\",\n" + "      \"clubId\": 53856,\n" + "      \"dispersionDistance\": 50,\n" +
			"      \"shotDistance\": 174,\n" + "      \"fairwayShotOutcome\": \"NO_FAIRWAY\"\n" + "    },\n" + "    {\n" +
			"      \"shotId\": 17,\n" + "      \"scorecardId\": 2,\n" + "      \"holeNumber\": 2,\n" +
			"      \"shotTime\": \"2017-12-09\",\n" + "      \"clubId\": 53843,\n" + "      \"dispersionDistance\": 12,\n" +
			"      \"shotDistance\": 221,\n" + "      \"fairwayShotOutcome\": \"RIGHT\"\n" + "    },\n" + "    {\n" +
			"      \"shotId\": 18,\n" + "      \"scorecardId\": 2,\n" + "      \"holeNumber\": 4,\n" +
			"      \"shotTime\": \"2017-12-09\",\n" + "      \"clubId\": 53859,\n" + "      \"dispersionDistance\": -111,\n" +
			"      \"shotDistance\": 30,\n" + "      \"fairwayShotOutcome\": \"LEFT\"\n" + "    }\n" + "  ]\n" + "}";
	}
}	
