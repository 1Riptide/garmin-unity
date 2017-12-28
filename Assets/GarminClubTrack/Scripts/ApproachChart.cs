using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

public class ApproachChart : MonoBehaviour, IGarmin3DChart
{
	enum LieTypes
	{
Unknown,
		Teebox,
		Rough,
		Bunker,
		Fairway,
		Green,
		Waste}

	;
	// Default shot object.
	public GameObject whiteDataPoint;
	public GameObject redDataPoint;
	public GameObject chartGameObject;
	public static GameObject[] dataPoints;

	public bool isFocused { get; set; }

	public float maxRadialDistance = 19f;
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
	private static readonly float[] DistanceBounds = new float[]{ -14.0f, 14.0f };
	private static readonly float[] LateralBounds = new float[]{ -9.0f, 9.0f };

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

	void Cleanup ()
	{
		if (dataPoints != null) {
			foreach (GameObject data in dataPoints) {
				Destroy (data);
			}
		}
	}

	public void Initialize (String json)
	{
		Debug.Log ("Initialize Approach : json = " + json);
		Cleanup ();
		var shotCount = 0;
		if (json == null || json.Length == 0) {
			// Get shot Count from JSON
			shotCount = 87;
			try {
				//StartCoroutine (AddDataPoints ());
			} catch (Exception e) {
				Debug.Log ("Exception calling AddDataPoints : " + e);
			}
		} else {
			try {
				Debug.Log ("Initialize() json is not null. Casting to JSON obj...");
				StartCoroutine (AddDataPoints (json));
			} catch (Exception e) {
				Debug.Log ("Exception parsing JSON : " + e);
			}
		}
	}



	IEnumerator AddDataPoints (String json)
	{
		var clubTrackApproachData = JSON.Parse (json);
		var shotData = clubTrackApproachData ["shotOrientationDetails"];
		var shotCount = shotData.Count;
		dataPoints = new GameObject[shotCount];
		Debug.Log ("AddDataPoints shotData count  " + shotCount);

		// Origin of datapoint creation
		Vector3 origin = chartGameObject.transform.position;
		// Log of distances
		float[] shotDistanceLog = createDistanceLog (clubTrackApproachData);
		// Outter band of Dartbord scale is 21 x 21.
		// Outter max is 33 x 33
		var maxValue = shotDistanceLog.Max ();
		float scaleRatio = maxValue / maxRadialDistance;

		for (int i = 0; i < shotCount; i++) {

			JSONNode shotOrientationDetail = shotData [i];
			Debug.Log ("shotOrientationDetail = " + shotOrientationDetail.ToString () + " count = " + i);

			var distance = shotOrientationDetail ["remainingDistance"]; // chip shot in-hole
			var angle = shotOrientationDetail ["offsetAngle"];// North being 0. Range[0-359]
			var lieType = shotOrientationDetail ["endingLieType"];
			shotDistanceLog [i] = distance;
			// Calculate distance and angle from origin. *Scaled to fit screen*
			Vector3 newPosition = (chartGameObject.transform.position +
			                      Quaternion.AngleAxis (angle, Vector3.up) * Vector3.forward * (distance / scaleRatio));

			// Create instance
			GameObject clone;
			if (!lieType.Equals (LieTypes.Green.ToString ())) {
				// Miss range is [21 - 39]
				// Red
				clone = AddDataPoint (redDataPoint, newPosition);

			} else {
				// Hit range is [0-21]
				// White
				clone = AddDataPoint (whiteDataPoint, newPosition);
			}

			// Reassign parent to chart object for tidyness.
			clone.transform.parent = chartGameObject.transform;
			// Add to list
			dataPoints [i] = clone;

			yield return new WaitForSeconds (0);
		}
	}

	float[] createDistanceLog (JSONNode data)
	{
		int shotCount = data.Count;
		float[] shotDistanceLog = new float[shotCount];
		var shotData = data ["shotOrientationDetails"];
		for (int i = 0; i < shotCount; i++) {

			JSONNode shotOrientationDetail = shotData [i];
			Debug.Log ("shotOrientationDetail = " + shotOrientationDetail.ToString () + " count = " + i);

			var distance = shotOrientationDetail ["remainingDistance"]; // chip shot in-hole
			var angle = shotOrientationDetail ["offsetAngle"];// North being 0. Range[0-359]
			var lieType = shotOrientationDetail ["endingLieType"];
			shotDistanceLog [i] = distance;
		}
		return shotDistanceLog;
	}

	GameObject AddDataPoint (GameObject dataPoint, Vector3 location)
	{
		return Instantiate (dataPoint, location, Quaternion.identity);
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
