using System;
using System.Linq;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

public class ApproachChartWithShots : RadialShotChart, IGarmin3DChart
{

	public void Initialize (String json)
	{
		if (json == null || json.Length == 0) {
			Debug.Log ("ApproachChartWithShots Initialize : json problems...Calling Initialize() : looks like json is empty? json = \n" + json);
		} else {
			try {
				Debug.Log ("ApproachChartWithShots Initialize() json is not null. Casting to JSON obj...");
				Cleanup ();
				StartCoroutine (AddDataPoints (json));
			} catch (Exception e) {
				Debug.Log ("ApproachChartWithShots Exception parsing JSON : " + e); 
			}
		}
	}

	IEnumerator AddDataPoints (String json)
	{
		var clubTrackApproachData = JSON.Parse (json);
		var shotData = clubTrackApproachData ["shotOrientationDetail"];
		var shotCount = shotData.Count;
		dataPoints = new GameObject[shotCount];
		Debug.Log ("ApproachChartWithShots AddDataPoints shotData count  " + shotCount);

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
			Debug.Log ("ApproachChartWithShots shotOrientationDetail = " + shotOrientationDetail.ToString () + " count = " + i);

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
				clone = AddDataPoint (missDataPointPrefab, newPosition);

			} else {
				// Hit range is [0-21]
				// White
				clone = AddDataPoint (hitDataPointPrefab, newPosition);
			}

			// Reassign parent to chart object for tidyness.
			clone.transform.parent = chartGameObject.transform;
			// Add to list
			dataPoints [i] = clone;

			yield return new WaitForSeconds (0);
		}
	}
}