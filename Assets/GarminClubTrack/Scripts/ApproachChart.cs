using System;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

public class ApproachChart : MonoBehaviour, IGarmin3DChart
{
	enum LieTypes {Unknown, Teebox, Rough, Bunker, Fairway, Green, Waste};
	// Default shot object.
	public GameObject whiteDataPoint;
	public GameObject redDataPoint;
	public GameObject chartGameObject;
	public static GameObject[] dataPoints;

	public bool isFocused { get; set; }

	// Properties mapped to JSON values
	float percentHitGreen10 = 0f;
	float percentHitGreen20 = 0f;
	float percentHitGreen20Plus = 0f;
	float percentMissedGreen = 0f;
	float percentShortOfGreen = 0f;
	float percentLongOfGreen = 0f;
	float percentLeftOfGreen = 0f;
	float percentRightOfGreen = 0f;

	// Used to devise a ratio with witch we plot datapoints based on real world distances.
	private static readonly float[] DistanceBounds = new float[]{-14.0f, 14.0f};
	private static readonly float[] LateralBounds = new float[]{-9.0f, 9.0f};

	// Use this for initialization
	void Start ()
	{
		
	}
	
	// Update is called once per frame
	void Update ()
	{
		
	}

	public void MockInitialize ()
	{
		// This must be called by external platform. Pass JSON.
		Initialize (getMockJSON ());
	}

	void Cleanup(){
		if (dataPoints != null) {
			foreach (GameObject data in dataPoints) {
				Destroy (data);
			}
		}
	}

	public void Initialize (String json)
	{
		Debug.Log ("Initialize Approach : json = " + json);
		Cleanup();
		var shotCount = 0;
		if (json == null || json.Length == 0) {
			// Get shot Count from JSON
			shotCount = 87;
			try{
				//StartCoroutine (AddDataPoints ());
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



	IEnumerator AddDataPoints(String json){
		var clubTrackApproachData = JSON.Parse(json);
		var shotData = clubTrackApproachData["shotOrientationDetails"];
		var shotCount = shotData.Count;
		dataPoints = new GameObject[shotCount];
		Debug.Log ("AddDataPoints shotData count  " + shotCount);

		// Origin of datapoint creation
		Vector3 origin = chartGameObject.transform.position;
		// Log of distances
		float[] shotDistanceLog = new float[shotCount];

		// Outter band of Dartbord scale is 21 x 21.
		// Outter max is 39 x 39
		// Plot the shots.

		for (int i = 0; i < shotCount; i++) {

			JSONNode shotOrientationDetail = shotData[i];
			Debug.Log ("shotOrientationDetail = " + shotOrientationDetail.ToString () + " count = " + i);

			var distance = shotOrientationDetail ["remainingDistance"]; // chip shot in-hole
			var angle = shotOrientationDetail ["offsetAngle"];// North being 0. Range[0-359]
			var lieType = shotOrientationDetail ["endingLieType"];
			shotDistanceLog [i] = distance;
			// Calculate distance and angle from origin.
			Vector3 newPosition = chartGameObject.transform.position + 
				Quaternion.AngleAxis(angle, Vector3.up) * Vector3.forward * distance;

			// Create instance
			GameObject clone;
			if(!lieType.Equals(LieTypes.Green.ToString())){
				// Miss range is [21 - 39]

				// Red
				clone = AddDataPoint (redDataPoint, newPosition);

			}else{
				// Hit range is [0-21]

				// White
				clone = AddDataPoint (whiteDataPoint, newPosition);
			}
			//Vector3 movement = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
			//Vector3 newPos = transform.position + movement;
			Vector3 offset = newPosition - chartGameObject.transform.position;
			clone.transform.position = chartGameObject.transform.position + Vector3.ClampMagnitude(offset, 14f);


			// Reassign parent to chart object for tidyness.
			clone.transform.parent = chartGameObject.transform;
			// Add to list
			dataPoints[i] = clone;

			yield return new WaitForSeconds(0);
		}


		// And now scale
		/*
		for (int i = 0; i < shotCount; i++) {
			var percent = shotDistanceLog [i] / shotDistanceLog.GetUpperBound(0);
			GameObject dataPoint = (GameObject)dataPoints [i];
			dataPoint.transform.position = dataPoint.transform.position * percent;
		}
		*/

		/*
	
		// Get the min and max bounds for both length and width of 3D space coords for greens hit on approach.
		float greenZMinBounds = DistanceBounds [0];
		float greenZMaxBounds = DistanceBounds [1];
		float greenXMinBounds = LateralBounds [0];
		float greenXMaxBounds = LateralBounds [1];
		// Get the min and max bounds for both length and width of 3D space coords for greens missed on approach.
		float missZMinBounds = DistanceBounds [0];
		float missZMaxBounds = DistanceBounds [1];
		float missXMinBounds = LateralBounds [0];
		float missXMaxBounds = LateralBounds [1];
	
		*/
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

		/*
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
				Debug.Log ("AddDataPoints ShotData is null! = " + shotData);

			}

			// Stall the loop for aesthetics as shots drop.
			yield return new WaitForSeconds(0);
		}
		*/
	}

	GameObject AddDataPoint(GameObject dataPoint, Vector3 location){
		return Instantiate(dataPoint, location, Quaternion.identity);
	}


	String getMockJSON ()
	{

		return "{\n" + "  \"numberOfRounds\": 0,\n" + "  \"percentHitGreen10\": 0,\n" + "  \"percentHitGreen20\": 0,\n" +

		"  \"percentHitGreen20Plus\": 0,\n" + "  \"percentMissedGreen\": 0,\n" + "  \"percentShortOfGreen\": 0,\n" +

		"  \"percentLongOfGreen\": 0,\n" + "  \"percentLeftOfGreen\": 0,\n" + "  \"percentRightOfGreen\": 0,\n" +

		"  \"percentGreenInRegulation\": 0,\n" + "  \"shotOrientationDetails\": [\n" + "    {\n" + "      \"remainingDistance\": 50,\n" +

		"      \"startingDistanceToHole\": 95,\n" + "      \"offsetAngle\": 20,\n" + "      \"shotId\": 0,\n" +

		"      \"clubId\": 23854881,\n" + "      \"scorecardId\": 0,\n" + "      \"holeId\": 0,\n" +

		"      \"startingLieType\": \"Fairway\",\n" + "      \"endingLieType\": \"Rough\",\n" + "      \"chipUpDown\": false\n" +

		"    },\n" + "    {\n" + "      \"remainingDistance\": 10,\n" + "      \"startingDistanceToHole\": 60,\n" +

		"      \"offsetAngle\": 10,\n" + "      \"shotId\": 1,\n" + "      \"clubId\": 23854881,\n" + "      \"scorecardId\": 0,\n" +

		"      \"holeId\": 0,\n" + "      \"startingLieType\": \"Fairway\",\n" + "      \"endingLieType\": \"Green\",\n" +

		"      \"chipUpDown\": false\n" + "    },\n" + "    {\n" + "      \"remainingDistance\": 10,\n" +

		"      \"startingDistanceToHole\": 64,\n" + "      \"offsetAngle\": 10,\n" + "      \"shotId\": 2,\n" +

		"      \"clubId\": 23854891,\n" + "      \"scorecardId\": 0,\n" + "      \"holeId\": 0,\n" +

		"      \"startingLieType\": \"Rough\",\n" + "      \"endingLieType\": \"Green\",\n" + "      \"chipUpDown\": false\n" +

		"    },\n" + "    {\n" + "      \"remainingDistance\": 7,\n" + "      \"startingDistanceToHole\": 94,\n" +

		"      \"offsetAngle\": 10,\n" + "      \"shotId\": 3,\n" + "      \"clubId\": 23854893,\n" + "      \"scorecardId\": 0,\n" +

		"      \"holeId\": 0,\n" + "      \"startingLieType\": \"Rough\",\n" + "      \"endingLieType\": \"Green\",\n" +

		"      \"chipUpDown\": false\n" + "    },\n" + "    {\n" + "      \"remainingDistance\": 62,\n" +

		"      \"startingDistanceToHole\": 134,\n" + "      \"offsetAngle\": 10,\n" + "      \"shotId\": 4,\n" +

		"      \"clubId\": 23854891,\n" + "      \"scorecardId\": 0,\n" + "      \"holeId\": 0,\n" +

		"      \"startingLieType\": \"Rough\",\n" + "      \"endingLieType\": \"Rough\",\n" + "      \"chipUpDown\": false\n" +

		"    },\n" + "    {\n" + "      \"remainingDistance\": 41,\n" + "      \"startingDistanceToHole\": 193,\n" +

		"      \"offsetAngle\": 263,\n" + "      \"shotId\": 5,\n" + "      \"clubId\": 23854894,\n" + "      \"scorecardId\": 0,\n" +

		"      \"holeId\": 0,\n" + "      \"startingLieType\": \"Rough\",\n" + "      \"endingLieType\": \"Rough\",\n" +

		"      \"chipUpDown\": false\n" + "    },\n" + "    {\n" + "      \"remainingDistance\": 11,\n" +

		"      \"startingDistanceToHole\": 197,\n" + "      \"offsetAngle\": 263,\n" + "      \"shotId\": 6,\n" +

		"      \"clubId\": 23854891,\n" + "      \"scorecardId\": 0,\n" + "      \"holeId\": 0,\n" +

		"      \"startingLieType\": \"Rough\",\n" + "      \"endingLieType\": \"Green\",\n" + "      \"chipUpDown\": false\n" +

		"    },\n" + "    {\n" + "      \"remainingDistance\": 21,\n" + "      \"startingDistanceToHole\": 223,\n" +

		"      \"offsetAngle\": 263,\n" + "      \"shotId\": 7,\n" + "      \"clubId\": 23854894,\n" + "      \"scorecardId\": 0,\n" +

		"      \"holeId\": 0,\n" + "      \"startingLieType\": \"Fairway\",\n" + "      \"endingLieType\": \"Green\",\n" +

		"      \"chipUpDown\": false\n" + "    },\n" + "    {\n" + "      \"remainingDistance\": 65,\n" +

		"      \"startingDistanceToHole\": 249,\n" + "      \"offsetAngle\": 165,\n" + "      \"shotId\": 8,\n" +

		"      \"clubId\": 23854894,\n" + "      \"scorecardId\": 0,\n" + "      \"holeId\": 0,\n" +

		"      \"startingLieType\": \"Fairway\",\n" + "      \"endingLieType\": \"Rough\",\n" + "      \"chipUpDown\": false\n" +

		"    },\n" + "    {\n" + "      \"remainingDistance\": 5,\n" + "      \"startingDistanceToHole\": 75,\n" +

		"      \"offsetAngle\": 10,\n" + "      \"shotId\": 9,\n" + "      \"clubId\": 23854882,\n" + "      \"scorecardId\": 0,\n" +

		"      \"holeId\": 0,\n" + "      \"startingLieType\": \"Rough\",\n" + "      \"endingLieType\": \"Green\",\n" +

		"      \"chipUpDown\": false\n" + "    }\n" + "  ]\n" + "}";

	}
}
