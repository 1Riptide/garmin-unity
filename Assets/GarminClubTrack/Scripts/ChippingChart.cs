using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;

public class ChippingChart : RadialShotChart, IGarmin3DChart {

	// Chip Shots in green percentage text objs.
	public GameObject hitGreenText;
	public GameObject middleOfGreenText;
	public GameObject longOfGreenText;

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
		var clubTrackChippingData = JSON.Parse (json);
		var shotData = clubTrackChippingData ["shotOrientationDetail"];
		var shotCount = shotData.Count;
		dataPoints = new GameObject[shotCount];
		Debug.Log ("### AddDataPoints shotData count  " + shotCount);

		// Hit Green Percentages
		TextMesh percentHitGreen10 = hitGreenText.GetComponent<TextMesh> ();
		TextMesh percentHitGreen20 = middleOfGreenText.GetComponent<TextMesh> ();
		TextMesh percentHitGreen30 = longOfGreenText.GetComponent<TextMesh> ();

		float hitGreen10Percent = clubTrackChippingData ["percentHitGreen10"];
		float hitGreen20Percent = clubTrackChippingData ["percentHitGreen20"];
		float hitGreen20PlusPercent = clubTrackChippingData ["percentHitGreen20Plus"];

		var hitGreen10PercentStr =  Mathf.Floor(hitGreen10Percent).ToString ();
		var hitGreen20PercentStr = Mathf.Floor(hitGreen20Percent).ToString ();
		var hitGreen20PlusPercentStr = Mathf.Floor(hitGreen20PlusPercent).ToString();

		percentHitGreen10.text = (hitGreen10PercentStr != GarminUtil.precisionPattern) ? hitGreen10PercentStr + "%" : GarminUtil.defaultPercentage + "%";
		percentHitGreen20.text = (hitGreen20PercentStr != GarminUtil.precisionPattern) ? hitGreen20PercentStr + "%" : GarminUtil.defaultPercentage + "%";
		percentHitGreen30.text = (hitGreen20PlusPercentStr != GarminUtil.precisionPattern) ? hitGreen20PlusPercentStr + "%" : GarminUtil.defaultPercentage + "%";

		// Origin of datapoint creation
		Vector3 origin = chartGameObject.transform.position;
		// Log of distances
		float[] shotDistanceLog = createDistanceLog (clubTrackChippingData, "shotOrientationDetail", "remainingDistance");
		// Outter band of Dartbord scale is 21 x 21.
		// Outter max is 33 x 33
		var maxValue = shotDistanceLog.Max ();
		Debug.Log ("shotOrientationDetail shotDistanceLog.Max = " + maxValue);
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

			//if (!lieType.Equals (LieTypes.Green.ToString ())) {
				// Miss range is [21 - 39]
				// Red
				//clone = AddDataPoint (missDataPointPrefab, newPosition);

			//} else {

				// Hit range is [0-21]
				// White
				clone = AddDataPoint (hitDataPointPrefab, newPosition);
				//}

				// Reassign parent to chart object for tidyness.
				clone.transform.parent = chartGameObject.transform;
				// Add to list
				dataPoints [i] = clone;

				yield return new WaitForSeconds (0);
			//}
		}

	}

	String getMockJSON ()
	{
		return "{\"numberOfRounds\":3,\"usingClubtrack\":true,\"percentHitGreen10\":43.48,\"percentHitGreen20\":13.04,\"percentHitGreen20Plus\":17.39,\"percentMissedGreen\":26.09,\"percentUpDown\":13.04,\"percentSandies\":0.0,\"shotOrientationDetail\":[{\"remainingDistance\":5.53,\"startingDistanceToHole\":35.39,\"offsetAngle\":37,\"shotId\":64505,\"clubId\":0,\"scorecardId\":17124,\"holeNumber\":1,\"startingLieType\":\"Fairway\",\"endingLieType\":\"Green\",\"onePuttAfter\":false,\"strokesGained\":-0.36},{\"remainingDistance\":7.55,\"startingDistanceToHole\":33.61,\"offsetAngle\":135,\"shotId\":64510,\"clubId\":0,\"scorecardId\":17124,\"holeNumber\":2,\"startingLieType\":\"Rough\",\"endingLieType\":\"Green\",\"onePuttAfter\":false,\"strokesGained\":-0.2},{\"remainingDistance\":7.08,\"startingDistanceToHole\":20.99,\"offsetAngle\":312,\"shotId\":64512,\"clubId\":0,\"scorecardId\":17124,\"holeNumber\":3,\"startingLieType\":\"Fairway\",\"endingLieType\":\"Green\",\"onePuttAfter\":false,\"strokesGained\":-0.52},{\"remainingDistance\":0.0,\"startingDistanceToHole\":36.58,\"offsetAngle\":334,\"shotId\":64516,\"clubId\":0,\"scorecardId\":17124,\"holeNumber\":5,\"startingLieType\":\"Rough\",\"endingLieType\":\"Green\",\"onePuttAfter\":false,\"strokesGained\":-0.17},{\"remainingDistance\":6.79,\"startingDistanceToHole\":33.97,\"offsetAngle\":169,\"shotId\":64523,\"clubId\":0,\"scorecardId\":17124,\"holeNumber\":8,\"startingLieType\":\"Rough\",\"endingLieType\":\"Green\",\"onePuttAfter\":false,\"strokesGained\":-0.2},{\"remainingDistance\":0.0,\"startingDistanceToHole\":33.49,\"offsetAngle\":180,\"shotId\":65189,\"clubId\":0,\"scorecardId\":17147,\"holeNumber\":3,\"startingLieType\":\"Fairway\",\"endingLieType\":\"Green\",\"onePuttAfter\":false,\"strokesGained\":-0.41},{\"remainingDistance\":5.95,\"startingDistanceToHole\":41.38,\"offsetAngle\":116,\"shotId\":65193,\"clubId\":0,\"scorecardId\":17147,\"holeNumber\":5,\"startingLieType\":\"Rough\",\"endingLieType\":\"Green\",\"onePuttAfter\":true,\"strokesGained\":-0.15},{\"remainingDistance\":0.0,\"startingDistanceToHole\":22.52,\"offsetAngle\":180,\"shotId\":65200,\"clubId\":23,\"scorecardId\":17147,\"holeNumber\":7,\"startingLieType\":\"Rough\",\"endingLieType\":\"Green\",\"onePuttAfter\":false,\"strokesGained\":-0.34},{\"remainingDistance\":19.23,\"startingDistanceToHole\":25.67,\"offsetAngle\":202,\"shotId\":65202,\"clubId\":0,\"scorecardId\":17147,\"holeNumber\":8,\"startingLieType\":\"Fairway\",\"endingLieType\":\"Rough\",\"onePuttAfter\":false,\"strokesGained\":-1.1},{\"remainingDistance\":0.0,\"startingDistanceToHole\":19.23,\"offsetAngle\":180,\"shotId\":65203,\"clubId\":23,\"scorecardId\":17147,\"holeNumber\":8,\"startingLieType\":\"Rough\",\"endingLieType\":\"Green\",\"onePuttAfter\":true,\"strokesGained\":-0.38},{\"remainingDistance\":0.0,\"startingDistanceToHole\":44.89,\"offsetAngle\":180,\"shotId\":68996,\"clubId\":56883,\"scorecardId\":17609,\"holeNumber\":10,\"startingLieType\":\"Rough\",\"endingLieType\":\"Green\",\"onePuttAfter\":false,\"strokesGained\":-0.11},{\"remainingDistance\":0.0,\"startingDistanceToHole\":12.55,\"offsetAngle\":180,\"shotId\":69013,\"clubId\":23,\"scorecardId\":17609,\"holeNumber\":15,\"startingLieType\":\"Rough\",\"endingLieType\":\"Green\",\"onePuttAfter\":false,\"strokesGained\":-0.54},{\"remainingDistance\":1.34,\"startingDistanceToHole\":15.6,\"offsetAngle\":21,\"shotId\":69021,\"clubId\":56883,\"scorecardId\":17609,\"holeNumber\":17,\"startingLieType\":\"Rough\",\"endingLieType\":\"Green\",\"onePuttAfter\":false,\"strokesGained\":-0.45},{\"remainingDistance\":12.34,\"startingDistanceToHole\":23.71,\"offsetAngle\":214,\"shotId\":69032,\"clubId\":56882,\"scorecardId\":17609,\"holeNumber\":2,\"startingLieType\":\"Fairway\",\"endingLieType\":\"Rough\",\"onePuttAfter\":false,\"strokesGained\":-0.94},{\"remainingDistance\":5.1,\"startingDistanceToHole\":12.34,\"offsetAngle\":211,\"shotId\":69033,\"clubId\":56882,\"scorecardId\":17609,\"holeNumber\":2,\"startingLieType\":\"Rough\",\"endingLieType\":\"Green\",\"onePuttAfter\":false,\"strokesGained\":-0.54},{\"remainingDistance\":0.0,\"startingDistanceToHole\":12.85,\"offsetAngle\":180,\"shotId\":69035,\"clubId\":56882,\"scorecardId\":17609,\"holeNumber\":2,\"startingLieType\":\"Rough\",\"endingLieType\":\"Green\",\"onePuttAfter\":false,\"strokesGained\":-0.53},{\"remainingDistance\":0.0,\"startingDistanceToHole\":18.44,\"offsetAngle\":180,\"shotId\":69040,\"clubId\":23,\"scorecardId\":17609,\"holeNumber\":3,\"startingLieType\":\"Fairway\",\"endingLieType\":\"Green\",\"onePuttAfter\":false,\"strokesGained\":-0.57},{\"remainingDistance\":18.96,\"startingDistanceToHole\":33.2,\"offsetAngle\":106,\"shotId\":69042,\"clubId\":0,\"scorecardId\":17609,\"holeNumber\":4,\"startingLieType\":\"Fairway\",\"endingLieType\":\"Bunker\",\"onePuttAfter\":false,\"strokesGained\":-0.96},{\"remainingDistance\":9.3,\"startingDistanceToHole\":18.96,\"offsetAngle\":115,\"shotId\":69043,\"clubId\":0,\"scorecardId\":17609,\"holeNumber\":4,\"startingLieType\":\"Bunker\",\"endingLieType\":\"Green\",\"onePuttAfter\":false,\"strokesGained\":-0.43},{\"remainingDistance\":21.17,\"startingDistanceToHole\":26.83,\"offsetAngle\":62,\"shotId\":69056,\"clubId\":56883,\"scorecardId\":17609,\"holeNumber\":8,\"startingLieType\":\"Rough\",\"endingLieType\":\"Rough\",\"onePuttAfter\":false,\"strokesGained\":-0.92},{\"remainingDistance\":19.51,\"startingDistanceToHole\":21.17,\"offsetAngle\":282,\"shotId\":69057,\"clubId\":56881,\"scorecardId\":17609,\"holeNumber\":8,\"startingLieType\":\"Rough\",\"endingLieType\":\"Bunker\",\"onePuttAfter\":false,\"strokesGained\":-0.91},{\"remainingDistance\":18.07,\"startingDistanceToHole\":19.51,\"offsetAngle\":13,\"shotId\":69058,\"clubId\":56883,\"scorecardId\":17609,\"holeNumber\":8,\"startingLieType\":\"Bunker\",\"endingLieType\":\"Rough\",\"onePuttAfter\":false,\"strokesGained\":-1.02},{\"remainingDistance\":0.0,\"startingDistanceToHole\":18.07,\"offsetAngle\":180,\"shotId\":69059,\"clubId\":56883,\"scorecardId\":17609,\"holeNumber\":8,\"startingLieType\":\"Rough\",\"endingLieType\":\"Green\",\"onePuttAfter\":true,\"strokesGained\":-0.38}]}";
	}
}
