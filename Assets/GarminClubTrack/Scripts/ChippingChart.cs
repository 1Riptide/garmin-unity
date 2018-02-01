using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;

public class ChippingChart : RadialShotChart, IGarmin3DChart {

	// Testing only.
	public void MockInitialize ()
	{
		Initialize (getMockJSON ());
	}

	public void Initialize (String json)
	{
		if (json == null || json.Length == 0) {
			Debug.Log ("Initialize : json problems...Calling Initialize() : looks like json is empty? json = \n" + json);
		} else {
			try {
				Debug.Log ("Initialize() json is not null. Casting to JSON obj...");
				Cleanup ();
				StartCoroutine (AddDataPoints (json));
			} catch (Exception e) {
				Debug.Log ("Exception parsing JSON : " + e); 
			}
		}
	}

	IEnumerator AddDataPoints (String json)
	{
		var clubTrackApproachData = JSON.Parse (json);
		var shotData = clubTrackApproachData ["shotOrientationDetail"];
		var shotCount = shotData.Count;
		dataPoints = new GameObject[shotCount];
		Debug.Log ("AddDataPoints shotData count  " + shotCount);

		// Origin of datapoint creation
		Vector3 origin = chartGameObject.transform.position;
		// Log of distances
		float[] shotDistanceLog = createDistanceLog (clubTrackApproachData, "shotOrientationDetail", "remainingDistance");
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
				//clone = AddDataPoint (missDataPointPrefab, newPosition);

			} else {

				// Hit range is [0-21]
				// White
				clone = AddDataPoint (hitDataPointPrefab, newPosition);
				//}

				// Reassign parent to chart object for tidyness.
				clone.transform.parent = chartGameObject.transform;
				// Add to list
				dataPoints [i] = clone;

				yield return new WaitForSeconds (0);
			}
		}

	}

	String getMockJSON ()
	{
		return "{\n" + "\"numberOfRounds\": 10,\n" + "\"percentHitGreen10\": 40,\n" + "\"percentHitGreen20\": 20,\n" +
			"\"percentHitGreen20Plus\": 40,\n" + "\"percentUpDown\": 25,\n" + "\"percentSandies\": 0,\n" +
			"\"shotOrientationDetail\": [\n" + "{\n" + "\"remainingDistance\": 2.07,\n" + "\"startingDistanceToHole\": 17.6,\n" +
			"\"offsetAngle\": 137.8,\n" + "\"shotId\": 1,\n" + "\"clubId\": 52600,\n" + "\"scorecardId\": 5678,\n" +
			"\"holeNumber\": 1,\n" + "\"startingLieType\": \"Fairway\",\n" + "\"endingLieType\": \"Green\",\n" +
			"\"onePuttAfter\": true\n" + "},\n" + "{\n" + "\"remainingDistance\": 0.89,\n" + "\"startingDistanceToHole\": 3.4,\n" +
			"\"offsetAngle\": 46.8,\n" + "\"shotId\": 2,\n" + "\"clubId\": 52602,\n" + "\"scorecardId\": 5678,\n" +
			"\"holeNumber\": 3,\n" + "\"startingLieType\": \"Rough\",\n" + "\"endingLieType\": \"Green\",\n" +
			"\"onePuttAfter\": false\n" + "},\n" + "{\n" + "\"remainingDistance\": 7.88,\n" + "\"startingDistanceToHole\": 16.2,\n" +
			"\"offsetAngle\": 86.7,\n" + "\"shotId\": 3,\n" + "\"clubId\": 52602,\n" + "\"scorecardId\": 5678,\n" +
			"\"holeNumber\": 7,\n" + "\"startingLieType\": \"Bunker\",\n" + "\"endingLieType\": \"Green\",\n" +
			"\"onePuttAfter\": false\n" + "},\n" + "{\n" + "\"remainingDistance\": 10.70,\n" + "\"startingDistanceToHole\": 14.8,\n" +
			"\"offsetAngle\": 249.6,\n" + "\"shotId\": 4,\n" + "\"clubId\": 52602,\n" + "\"scorecardId\": 5678,\n" +
			"\"holeNumber\": 10,\n" + "\"startingLieType\": \"Fairway\",\n" + "\"endingLieType\": \"Green\",\n" +
			"\"onePuttAfter\": false\n" + "},\n" + "{\n" + "\"remainingDistance\": 4.87,\n" + "\"startingDistanceToHole\": 7.4,\n" +
			"\"offsetAngle\": 237.4,\n" + "\"shotId\": 5,\n" + "\"clubId\": 52602,\n" + "\"scorecardId\": 5678,\n" +
			"\"holeNumber\": 11,\n" + "\"startingLieType\": \"Fairway\",\n" + "\"endingLieType\": \"Green\",\n" +
			"\"onePuttAfter\": true\n" + "},\n" + "{\n" + "\"remainingDistance\": 2.29,\n" + "\"startingDistanceToHole\": 4.7,\n" +
			"\"offsetAngle\": 5.4,\n" + "\"shotId\": 6,\n" + "\"clubId\": 52602,\n" + "\"scorecardId\": 5678,\n" +
			"\"holeNumber\": 16,\n" + "\"startingLieType\": \"Rough\",\n" + "\"endingLieType\": \"Rough\",\n" +
			"\"onePuttAfter\": false\n" + "},\n" + "{\n" + "\"remainingDistance\": 12.76,\n" + "\"startingDistanceToHole\": 9.9,\n" +
			"\"offsetAngle\": 63.7,\n" + "\"shotId\": 7,\n" + "\"clubId\": 52603,\n" + "\"scorecardId\": 5678,\n" +
			"\"holeNumber\": 17,\n" + "\"startingLieType\": \"Fairway\",\n" + "\"endingLieType\": \"Rough\",\n" +
			"\"onePuttAfter\": true\n" + "},\n" + "{\n" + "\"remainingDistance\": 14.67,\n" + "\"startingDistanceToHole\": 11.8,\n" +
			"\"offsetAngle\": 111.3,\n" + "\"shotId\": 8,\n" + "\"clubId\": 52602,\n" + "\"scorecardId\": 5678,\n" +
			"\"holeNumber\": 18,\n" + "\"startingLieType\": \"Rough\",\n" + "\"endingLieType\": \"Bunker\",\n" +
			"\"onePuttAfter\": false\n" + "}\n" + "]\n" + "}\n";

	}
}
