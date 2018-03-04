using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;
using UnityEngine.UI;

public class ApproachChart : MonoBehaviour, IGarmin3DChart, IGarminNestedChart
{
	// Default shot object.
	public GameObject whiteDataPoint;
	public GameObject redDataPoint;
	public GameObject chartGameObject;
	public GameObject scatterChartGameObject;
	public static GameObject[] dataPoints;

	public bool isFocused { get; set; }
	// is user looking here?
	public bool isDefaultState { get; set; }
	// Hide these GameObjects when clubTrack flag is missing or false. (user does not have feature).
	public GameObject hide4NonClubTrack;
	// Approach Shots in green percentage text objs.
	public GameObject hitGreenText;
	public GameObject middleOfGreenText;
	public GameObject longOfGreenText;

	// Approach Shots missed green percentage text objs.
	public GameObject missedGreenShortText;
	public GameObject missedGreenLongText;
	public GameObject missedGreenLeftText;
	public GameObject missedGreenRightText;

	public void MockInitialize ()
	{
		// This must be called by external platform. Pass JSON.
		Initialize (getMockJSON ());
	}

	public void Initialize (String json)
	{
		if (json == null || json.Length == 0) {
			Debug.Log ("ApproachChart Initialize : json problems...Calling Initialize() : looks like json is empty? json = \n" + json);
		} else {
			try {
				UpdateApproachStats (json);
				scatterChartGameObject.GetComponent<ApproachChartWithShots> ().Initialize (json);
			} catch (Exception e) {
				Debug.Log ("ApproachChart Exception parsing JSON : " + e);
			}
		}
	}

	void UpdateApproachStats (String json)
	{
		var clubTrackApproachData = JSON.Parse (json);

		/** GOLFAPP-3147
		 * Backstory: So now, instead of using property "usingClubtrack" to track if a user is using Clubtrack, 
		 * we are now operating off of the property "percentHitGreen" to denote a non-Clubtrak user.
		 */
		//bool usingClubTrack = clubTrackApproachData ["usingClubtrack"].AsBool;
		float hitGreenPercent = clubTrackApproachData ["percentHitGreen"];
		bool usingClubTrack = hitGreenPercent == null;

		Debug.Log ("ApproachChart UpdateApproachStats - json = \n" + usingClubTrack);
		// Hit Green Percentages
		TextMesh percentHitGreen10 = hitGreenText.GetComponent<TextMesh> ();
		TextMesh percentHitGreen20 = middleOfGreenText.GetComponent<TextMesh> ();
		TextMesh percentHitGreen30 = longOfGreenText.GetComponent<TextMesh> ();

		// Missed Green Percentages
		TextMesh missedGreenShort = missedGreenShortText.GetComponent<TextMesh> ();
		TextMesh missedGreenLong = missedGreenLongText.GetComponent<TextMesh> ();
		TextMesh missedGreenLeft = missedGreenLeftText.GetComponent<TextMesh> ();
		TextMesh missedGreenRight = missedGreenRightText.GetComponent<TextMesh> ();

		if (usingClubTrack) {
			float hitGreen10Percent = clubTrackApproachData ["percentHitGreen10"];
			float hitGreen20Percent = clubTrackApproachData ["percentHitGreen20"];
			float hitGreen20PlusPercent = clubTrackApproachData ["percentHitGreen20Plus"];

			var hitGreen10PercentStr = hitGreen10Percent.ToString (GarminUtil.floatFormat);
			var hitGreen20PercentStr = hitGreen20Percent.ToString (GarminUtil.floatFormat);
			var hitGreen20PlusPercentStr = hitGreen20PlusPercent.ToString (GarminUtil.floatFormat);

			percentHitGreen10.text = (hitGreen10Percent != null && hitGreen10PercentStr != GarminUtil.precisionPattern) ? hitGreen10PercentStr + "%" : GarminUtil.defaultPercentage + "%";
			percentHitGreen20.text = (hitGreen20Percent != null && hitGreen20PercentStr != GarminUtil.precisionPattern) ? hitGreen20PercentStr + "%" : GarminUtil.defaultPercentage + "%";
			percentHitGreen30.text = (hitGreen20PlusPercent != null && hitGreen20PlusPercentStr != GarminUtil.precisionPattern) ? hitGreen20PlusPercentStr + "%" : GarminUtil.defaultPercentage + "%";

		} else {
			hide4NonClubTrack.SetActive (false);
			var hitGreeenPercentStr = hitGreenPercent.ToString (GarminUtil.floatFormat);

			Debug.Log ("ApproachChart UpdateApproachStats - NO CLUB TRACK. percentHitGreen = " + hitGreenPercent);
			percentHitGreen10.text = (hitGreenPercent != null && hitGreeenPercentStr != GarminUtil.precisionPattern) ? hitGreeenPercentStr + "%" : GarminUtil.defaultPercentage + "%";
		}
		float shortOfGreenPercent = clubTrackApproachData ["percentShortOfGreen"];
		float longOfGreenPercent = clubTrackApproachData ["percentLongOfGreen"];
		float leftOfGreenPercent = clubTrackApproachData ["percentLeftOfGreen"];
		float rightOfGreenPercent = clubTrackApproachData ["percentRightOfGreen"];

		var shortOfGreenPercentStr = shortOfGreenPercent.ToString (GarminUtil.floatFormat);
		var longOfGreenPercentStr = longOfGreenPercent.ToString (GarminUtil.floatFormat);
		var leftOfGreenPercentStr = leftOfGreenPercent.ToString (GarminUtil.floatFormat);
		var rightOfGreenPercentStr = rightOfGreenPercent.ToString (GarminUtil.floatFormat);
		Debug.Log ("ApproachChart UpdateApproachStats - " + longOfGreenPercentStr);

		missedGreenShort.text = (shortOfGreenPercent != null && shortOfGreenPercentStr != GarminUtil.precisionPattern) ? shortOfGreenPercentStr + "%" : GarminUtil.defaultPercentage + "%";
		missedGreenLong.text = (longOfGreenPercent != null && longOfGreenPercentStr != GarminUtil.precisionPattern) ? longOfGreenPercentStr + "%" : GarminUtil.defaultPercentage + "%";
		missedGreenLeft.text = (leftOfGreenPercent != null && leftOfGreenPercentStr != GarminUtil.precisionPattern) ? leftOfGreenPercentStr + "%" : GarminUtil.defaultPercentage + "%"; 
		missedGreenRight.text = (rightOfGreenPercent != null && rightOfGreenPercentStr != GarminUtil.precisionPattern) ? rightOfGreenPercentStr + "%" : GarminUtil.defaultPercentage + "%"; 

	}

	/** Approach GCS Contract as of 02/20/18
	 * {
		  "numberOfRounds": 0,
		  "usingClubtrack": true,
		  "percentHitGreen": 0,
		  "percentHitGreen10": 0,
		  "percentHitGreen20": 0,
		  "percentHitGreen20Plus": 0,
		  "percentMissedGreen": 0,
		  "percentShortOfGreen": 0,
		  "percentLongOfGreen": 0,
		  "percentLeftOfGreen": 0,
		  "percentRightOfGreen": 0,
		  "percentGreenInRegulation": 0,
		  "shotOrientationDetail": [
		    {
		      "remainingDistance": 0,
		      "startingDistanceToHole": 0,
		      "offsetAngle": 0,
		      "shotId": 0,
		      "clubId": 0,
		      "scorecardId": 0,
		      "holeNumber": 0,
		      "startingLieType": "Unknown",
		      "endingLieType": "Unknown",
		      "onePuttAfter": true,
		      "strokesGained": 0
		    }
		  ]
		}
		*/

	String getMockJSON ()
	{
		return "{\"numberOfRounds\":3,\"percentHitGreen\":48.48,\"percentMissedGreen\":51.52,\"percentShortOfGreen\":39.39,\"percentLongOfGreen\":6.06,\"percentLeftOfGreen\":3.03,\"percentRightOfGreen\":3.03,\"percentGreenInRegulation\":0.0,\"shotOrientationDetail\":[{\"remainingDistance\":35.39,\"startingDistanceToHole\":104.21,\"offsetAngle\":211,\"shotId\":64504,\"clubId\":0,\"scorecardId\":17124,\"holeNumber\":1,\"startingLieType\":\"Rough\",\"endingLieType\":\"Fairway\",\"strokesGained\":-0.5},{\"remainingDistance\":73.18,\"startingDistanceToHole\":241.71,\"offsetAngle\":194,\"shotId\":64508,\"clubId\":0,\"scorecardId\":17124,\"holeNumber\":2,\"startingLieType\":\"Fairway\",\"endingLieType\":\"Rough\",\"strokesGained\":0.11},{\"remainingDistance\":33.61,\"startingDistanceToHole\":73.18,\"offsetAngle\":189,\"shotId\":64509,\"clubId\":0,\"scorecardId\":17124,\"holeNumber\":2,\"startingLieType\":\"Rough\",\"endingLieType\":\"Rough\",\"strokesGained\":-0.77},{\"remainingDistance\":20.99,\"startingDistanceToHole\":126.06,\"offsetAngle\":187,\"shotId\":64511,\"clubId\":0,\"scorecardId\":17124,\"holeNumber\":3,\"startingLieType\":\"TeeBox\",\"endingLieType\":\"Fairway\",\"strokesGained\":-0.44},{\"remainingDistance\":159.55,\"startingDistanceToHole\":295.17,\"offsetAngle\":345,\"shotId\":64513,\"clubId\":0,\"scorecardId\":17124,\"holeNumber\":4,\"startingLieType\":\"TeeBox\",\"endingLieType\":\"Rough\",\"strokesGained\":-0.46},{\"remainingDistance\":14.17,\"startingDistanceToHole\":159.55,\"offsetAngle\":325,\"shotId\":64514,\"clubId\":0,\"scorecardId\":17124,\"holeNumber\":4,\"startingLieType\":\"Rough\",\"endingLieType\":\"Fairway\",\"strokesGained\":0.01},{\"remainingDistance\":36.58,\"startingDistanceToHole\":122.52,\"offsetAngle\":137,\"shotId\":64515,\"clubId\":0,\"scorecardId\":17124,\"holeNumber\":5,\"startingLieType\":\"TeeBox\",\"endingLieType\":\"Rough\",\"strokesGained\":-0.78},{\"remainingDistance\":50.45,\"startingDistanceToHole\":230.99,\"offsetAngle\":153,\"shotId\":64518,\"clubId\":0,\"scorecardId\":17124,\"holeNumber\":6,\"startingLieType\":\"Fairway\",\"endingLieType\":\"Rough\",\"strokesGained\":-0.33},{\"remainingDistance\":3.8,\"startingDistanceToHole\":50.45,\"offsetAngle\":154,\"shotId\":64519,\"clubId\":0,\"scorecardId\":17124,\"holeNumber\":6,\"startingLieType\":\"Rough\",\"endingLieType\":\"Green\",\"strokesGained\":-0.06},{\"remainingDistance\":124.3,\"startingDistanceToHole\":212.31,\"offsetAngle\":189,\"shotId\":64520,\"clubId\":0,\"scorecardId\":17124,\"holeNumber\":7,\"startingLieType\":\"TeeBox\",\"endingLieType\":\"Rough\",\"strokesGained\":-0.89},{\"remainingDistance\":11.9,\"startingDistanceToHole\":124.3,\"offsetAngle\":285,\"shotId\":64521,\"clubId\":0,\"scorecardId\":17124,\"holeNumber\":7,\"startingLieType\":\"Rough\",\"endingLieType\":\"Green\",\"strokesGained\":0.18},{\"remainingDistance\":33.97,\"startingDistanceToHole\":157.96,\"offsetAngle\":264,\"shotId\":64522,\"clubId\":0,\"scorecardId\":17124,\"holeNumber\":8,\"startingLieType\":\"TeeBox\",\"endingLieType\":\"Rough\",\"strokesGained\":-0.7},{\"remainingDistance\":84.95,\"startingDistanceToHole\":203.58,\"offsetAngle\":177,\"shotId\":64524,\"clubId\":0,\"scorecardId\":17124,\"holeNumber\":9,\"startingLieType\":\"Fairway\",\"endingLieType\":\"Fairway\",\"strokesGained\":-0.41},{\"remainingDistance\":6.56,\"startingDistanceToHole\":95.99,\"offsetAngle\":60,\"shotId\":65183,\"clubId\":56878,\"scorecardId\":17147,\"holeNumber\":1,\"startingLieType\":\"Fairway\",\"endingLieType\":\"Green\",\"strokesGained\":-0.17},{\"remainingDistance\":0.0,\"startingDistanceToHole\":92.13,\"offsetAngle\":180,\"shotId\":65187,\"clubId\":56879,\"scorecardId\":17147,\"holeNumber\":2,\"startingLieType\":\"Rough\",\"endingLieType\":\"Green\",\"strokesGained\":0.04},{\"remainingDistance\":0.0,\"startingDistanceToHole\":149.68,\"offsetAngle\":180,\"shotId\":65190,\"clubId\":56869,\"scorecardId\":17147,\"holeNumber\":4,\"startingLieType\":\"TeeBox\",\"endingLieType\":\"Green\",\"strokesGained\":0.02},{\"remainingDistance\":0.0,\"startingDistanceToHole\":154.65,\"offsetAngle\":180,\"shotId\":65197,\"clubId\":56871,\"scorecardId\":17147,\"holeNumber\":6,\"startingLieType\":\"Fairway\",\"endingLieType\":\"Green\",\"strokesGained\":0.04},{\"remainingDistance\":22.52,\"startingDistanceToHole\":138.5,\"offsetAngle\":126,\"shotId\":65199,\"clubId\":56870,\"scorecardId\":17147,\"holeNumber\":7,\"startingLieType\":\"Fairway\",\"endingLieType\":\"Rough\",\"strokesGained\":-0.69},{\"remainingDistance\":0.0,\"startingDistanceToHole\":70.44,\"offsetAngle\":180,\"shotId\":65206,\"clubId\":56880,\"scorecardId\":17147,\"holeNumber\":9,\"startingLieType\":\"Rough\",\"endingLieType\":\"Green\",\"strokesGained\":-0.03},{\"remainingDistance\":129.65,\"startingDistanceToHole\":127.94,\"offsetAngle\":176,\"shotId\":69003,\"clubId\":0,\"scorecardId\":17609,\"holeNumber\":12,\"startingLieType\":\"TeeBox\",\"endingLieType\":\"TeeBox\",\"strokesGained\":-0.99},{\"remainingDistance\":0.0,\"startingDistanceToHole\":117.83,\"offsetAngle\":180,\"shotId\":69007,\"clubId\":56877,\"scorecardId\":17609,\"holeNumber\":13,\"startingLieType\":\"Fairway\",\"endingLieType\":\"Green\",\"strokesGained\":-0.09},{\"remainingDistance\":23.95,\"startingDistanceToHole\":155.35,\"offsetAngle\":247,\"shotId\":69008,\"clubId\":56875,\"scorecardId\":17609,\"holeNumber\":14,\"startingLieType\":\"TeeBox\",\"endingLieType\":\"Green\",\"strokesGained\":0.05},{\"remainingDistance\":8.19,\"startingDistanceToHole\":84.4,\"offsetAngle\":122,\"shotId\":69017,\"clubId\":56881,\"scorecardId\":17609,\"holeNumber\":16,\"startingLieType\":\"Fairway\",\"endingLieType\":\"Green\",\"strokesGained\":-0.19},{\"remainingDistance\":8.73,\"startingDistanceToHole\":100.2,\"offsetAngle\":138,\"shotId\":69025,\"clubId\":56880,\"scorecardId\":17609,\"holeNumber\":18,\"startingLieType\":\"Rough\",\"endingLieType\":\"Green\",\"strokesGained\":0.08},{\"remainingDistance\":63.0,\"startingDistanceToHole\":127.04,\"offsetAngle\":204,\"shotId\":69028,\"clubId\":56875,\"scorecardId\":17609,\"holeNumber\":1,\"startingLieType\":\"Fairway\",\"endingLieType\":\"Rough\",\"strokesGained\":-1.01},{\"remainingDistance\":12.08,\"startingDistanceToHole\":63.0,\"offsetAngle\":92,\"shotId\":69029,\"clubId\":56883,\"scorecardId\":17609,\"holeNumber\":1,\"startingLieType\":\"Rough\",\"endingLieType\":\"Green\",\"strokesGained\":-0.04},{\"remainingDistance\":23.71,\"startingDistanceToHole\":139.62,\"offsetAngle\":171,\"shotId\":69031,\"clubId\":56875,\"scorecardId\":17609,\"holeNumber\":2,\"startingLieType\":\"TeeBox\",\"endingLieType\":\"Fairway\",\"strokesGained\":-0.48},{\"remainingDistance\":18.44,\"startingDistanceToHole\":85.47,\"offsetAngle\":187,\"shotId\":69039,\"clubId\":56881,\"scorecardId\":17609,\"holeNumber\":3,\"startingLieType\":\"Fairway\",\"endingLieType\":\"Fairway\",\"strokesGained\":-0.61},{\"remainingDistance\":0.0,\"startingDistanceToHole\":90.96,\"offsetAngle\":44,\"shotId\":69046,\"clubId\":56881,\"scorecardId\":17609,\"holeNumber\":5,\"startingLieType\":\"Rough\",\"endingLieType\":\"Green\",\"strokesGained\":0.05},{\"remainingDistance\":89.4,\"startingDistanceToHole\":301.88,\"offsetAngle\":168,\"shotId\":69048,\"clubId\":0,\"scorecardId\":17609,\"holeNumber\":6,\"startingLieType\":\"TeeBox\",\"endingLieType\":\"Rough\",\"strokesGained\":-0.18},{\"remainingDistance\":8.52,\"startingDistanceToHole\":89.4,\"offsetAngle\":90,\"shotId\":69049,\"clubId\":56880,\"scorecardId\":17609,\"holeNumber\":6,\"startingLieType\":\"Rough\",\"endingLieType\":\"Green\",\"strokesGained\":0.04},{\"remainingDistance\":2.55,\"startingDistanceToHole\":140.01,\"offsetAngle\":137,\"shotId\":69051,\"clubId\":56872,\"scorecardId\":17609,\"holeNumber\":7,\"startingLieType\":\"TeeBox\",\"endingLieType\":\"Green\",\"strokesGained\":0.01},{\"remainingDistance\":4.28,\"startingDistanceToHole\":90.46,\"offsetAngle\":225,\"shotId\":69063,\"clubId\":56879,\"scorecardId\":17609,\"holeNumber\":9,\"startingLieType\":\"Rough\",\"endingLieType\":\"Green\",\"strokesGained\":0.05}]}";
	}
}
