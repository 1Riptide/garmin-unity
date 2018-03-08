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
		Initialize (getMockJSONNoClubTrack ());
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
		var hitGreenPercent = clubTrackApproachData ["percentHitGreen"];
		bool usingClubTrack = !hitGreenPercent.IsNull;

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

			var hitGreen10PercentStr = Mathf.Floor (hitGreen10Percent).ToString ();
			var hitGreen20PercentStr = Mathf.Floor (hitGreen20Percent).ToString ();
			var hitGreen20PlusPercentStr = Mathf.Floor (hitGreen20PlusPercent).ToString ();

			percentHitGreen10.text = (hitGreen10PercentStr != GarminUtil.precisionPattern) ? hitGreen10PercentStr + "%" : GarminUtil.defaultPercentage + "%";
			percentHitGreen20.text = (hitGreen20PercentStr != GarminUtil.precisionPattern) ? hitGreen20PercentStr + "%" : GarminUtil.defaultPercentage + "%";
			percentHitGreen30.text = (hitGreen20PlusPercentStr != GarminUtil.precisionPattern) ? hitGreen20PlusPercentStr + "%" : GarminUtil.defaultPercentage + "%";

		} else {
			hide4NonClubTrack.SetActive (false);
			var hitGreeenPercentStr = Mathf.Floor (hitGreenPercent).ToString ();

			Debug.Log ("ApproachChart UpdateApproachStats - NO CLUB TRACK. percentHitGreen = " + hitGreenPercent);
			percentHitGreen10.text = (hitGreeenPercentStr != GarminUtil.precisionPattern) ? hitGreeenPercentStr + "%" : GarminUtil.defaultPercentage + "%";
		}
		float shortOfGreenPercent = clubTrackApproachData ["percentShortOfGreen"];
		float longOfGreenPercent = clubTrackApproachData ["percentLongOfGreen"];
		float leftOfGreenPercent = clubTrackApproachData ["percentLeftOfGreen"];
		float rightOfGreenPercent = clubTrackApproachData ["percentRightOfGreen"];

		var shortOfGreenPercentStr = Mathf.Floor (shortOfGreenPercent).ToString ();
		var longOfGreenPercentStr = Mathf.Floor (longOfGreenPercent).ToString ();
		var leftOfGreenPercentStr = Mathf.Floor (leftOfGreenPercent).ToString ();
		var rightOfGreenPercentStr = Mathf.Floor (rightOfGreenPercent).ToString ();
		Debug.Log ("ApproachChart UpdateApproachStats - " + longOfGreenPercentStr);

		missedGreenShort.text = (shortOfGreenPercentStr != GarminUtil.precisionPattern) ? shortOfGreenPercentStr + "%" : GarminUtil.defaultPercentage + "%";
		missedGreenLong.text = (longOfGreenPercentStr != GarminUtil.precisionPattern) ? longOfGreenPercentStr + "%" : GarminUtil.defaultPercentage + "%";
		missedGreenLeft.text = (leftOfGreenPercentStr != GarminUtil.precisionPattern) ? leftOfGreenPercentStr + "%" : GarminUtil.defaultPercentage + "%"; 
		missedGreenRight.text = (rightOfGreenPercentStr != GarminUtil.precisionPattern) ? rightOfGreenPercentStr + "%" : GarminUtil.defaultPercentage + "%"; 

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

	String getMockJSONNoClubTrack ()
	{
		return "{\\r\\n    \\\"numberOfRounds\\\": 3,\\r\\n    \\\"percentHitGreen10\\\": 2.86,\\r\\n    \\\"percentHitGreen20\\\": 0,\\r\\n    \\\"percentHitGreen20Plus\\\": 22.86,\\r\\n    \\\"percentMissedGreen\\\": 74.29,\\r\\n    \\\"percentShortOfGreen\\\": 40,\\r\\n    \\\"percentLongOfGreen\\\": 8.57,\\r\\n    \\\"percentLeftOfGreen\\\": 20,\\r\\n    \\\"percentRightOfGreen\\\": 5.71,\\r\\n    \\\"percentGreenInRegulation\\\": 34.57,\\r\\n    \\\"shotOrientationDetail\\\": [\\r\\n        {\\r\\n            \\\"remainingDistance\\\": 26.65,\\r\\n            \\\"startingDistanceToHole\\\": 138.43,\\r\\n            \\\"offsetAngle\\\": 151,\\r\\n            \\\"shotId\\\": 74277,\\r\\n            \\\"clubId\\\": 20823175,\\r\\n            \\\"scorecardId\\\": 17755,\\r\\n            \\\"holeNumber\\\": 3,\\r\\n            \\\"startingLieType\\\": \\\"Bunker\\\",\\r\\n            \\\"endingLieType\\\": \\\"Fairway\\\",\\r\\n            \\\"strokesGained\\\": -0.25\\r\\n        },\\r\\n        {\\r\\n            \\\"remainingDistance\\\": 22.63,\\r\\n            \\\"startingDistanceToHole\\\": 196.92,\\r\\n            \\\"offsetAngle\\\": 226,\\r\\n            \\\"shotId\\\": 74293,\\r\\n            \\\"clubId\\\": 20823168,\\r\\n            \\\"scorecardId\\\": 17755,\\r\\n            \\\"holeNumber\\\": 7,\\r\\n            \\\"startingLieType\\\": \\\"Fairway\\\",\\r\\n            \\\"endingLieType\\\": \\\"Fairway\\\",\\r\\n            \\\"strokesGained\\\": -0.17\\r\\n        },\\r\\n        {\\r\\n            \\\"remainingDistance\\\": 8.36,\\r\\n            \\\"startingDistanceToHole\\\": 133.5,\\r\\n            \\\"offsetAngle\\\": 42,\\r\\n            \\\"shotId\\\": 74304,\\r\\n            \\\"clubId\\\": 20823175,\\r\\n            \\\"scorecardId\\\": 17755,\\r\\n            \\\"holeNumber\\\": 10,\\r\\n            \\\"startingLieType\\\": \\\"Fairway\\\",\\r\\n            \\\"endingLieType\\\": \\\"Green\\\",\\r\\n            \\\"strokesGained\\\": -0.02\\r\\n        },\\r\\n        {\\r\\n            \\\"remainingDistance\\\": 2.2,\\r\\n            \\\"startingDistanceToHole\\\": 86.15,\\r\\n            \\\"offsetAngle\\\": 346,\\r\\n            \\\"shotId\\\": 74332,\\r\\n            \\\"clubId\\\": 20823180,\\r\\n            \\\"scorecardId\\\": 17755,\\r\\n            \\\"holeNumber\\\": 17,\\r\\n            \\\"startingLieType\\\": \\\"Fairway\\\",\\r\\n            \\\"endingLieType\\\": \\\"Green\\\",\\r\\n            \\\"strokesGained\\\": 0.34\\r\\n        },\\r\\n        {\\r\\n            \\\"remainingDistance\\\": 26.68,\\r\\n            \\\"startingDistanceToHole\\\": 184.74,\\r\\n            \\\"offsetAngle\\\": 241,\\r\\n            \\\"shotId\\\": 74335,\\r\\n            \\\"clubId\\\": 20823168,\\r\\n            \\\"scorecardId\\\": 17755,\\r\\n            \\\"holeNumber\\\": 18,\\r\\n            \\\"startingLieType\\\": \\\"Fairway\\\",\\r\\n            \\\"endingLieType\\\": \\\"Bunker\\\",\\r\\n            \\\"strokesGained\\\": -0.44\\r\\n        },\\r\\n        {\\r\\n            \\\"remainingDistance\\\": 10.22,\\r\\n            \\\"startingDistanceToHole\\\": 90.85,\\r\\n            \\\"offsetAngle\\\": 288,\\r\\n            \\\"shotId\\\": 74340,\\r\\n            \\\"clubId\\\": 20823179,\\r\\n            \\\"scorecardId\\\": 17756,\\r\\n            \\\"holeNumber\\\": 1,\\r\\n            \\\"startingLieType\\\": \\\"Rough\\\",\\r\\n            \\\"endingLieType\\\": \\\"Green\\\",\\r\\n            \\\"strokesGained\\\": 0.05\\r\\n        },\\r\\n        {\\r\\n            \\\"remainingDistance\\\": 24.84,\\r\\n            \\\"startingDistanceToHole\\\": 149.58,\\r\\n            \\\"offsetAngle\\\": 268,\\r\\n            \\\"shotId\\\": 74343,\\r\\n            \\\"clubId\\\": 20823174,\\r\\n            \\\"scorecardId\\\": 17756,\\r\\n            \\\"holeNumber\\\": 2,\\r\\n            \\\"startingLieType\\\": \\\"TeeBox\\\",\\r\\n            \\\"endingLieType\\\": \\\"Rough\\\",\\r\\n            \\\"strokesGained\\\": -0.63\\r\\n        },\\r\\n        {\\r\\n            \\\"remainingDistance\\\": 15.39,\\r\\n            \\\"startingDistanceToHole\\\": 140.48,\\r\\n            \\\"offsetAngle\\\": 289,\\r\\n            \\\"shotId\\\": 74350,\\r\\n            \\\"clubId\\\": 20823175,\\r\\n            \\\"scorecardId\\\": 17756,\\r\\n            \\\"holeNumber\\\": 4,\\r\\n            \\\"startingLieType\\\": \\\"TeeBox\\\",\\r\\n            \\\"endingLieType\\\": \\\"Rough\\\",\\r\\n            \\\"strokesGained\\\": -0.49\\r\\n        },\\r\\n        {\\r\\n            \\\"remainingDistance\\\": 7.02,\\r\\n            \\\"startingDistanceToHole\\\": 124.54,\\r\\n            \\\"offsetAngle\\\": 249,\\r\\n            \\\"shotId\\\": 74354,\\r\\n            \\\"clubId\\\": 20823176,\\r\\n            \\\"scorecardId\\\": 17756,\\r\\n            \\\"holeNumber\\\": 5,\\r\\n            \\\"startingLieType\\\": \\\"Fairway\\\",\\r\\n            \\\"endingLieType\\\": \\\"Green\\\",\\r\\n            \\\"strokesGained\\\": 0.04\\r\\n        },\\r\\n        {\\r\\n            \\\"remainingDistance\\\": 11.33,\\r\\n            \\\"startingDistanceToHole\\\": 141.32,\\r\\n            \\\"offsetAngle\\\": 72,\\r\\n            \\\"shotId\\\": 74357,\\r\\n            \\\"clubId\\\": 20823175,\\r\\n            \\\"scorecardId\\\": 17756,\\r\\n            \\\"holeNumber\\\": 6,\\r\\n            \\\"startingLieType\\\": \\\"TeeBox\\\",\\r\\n            \\\"endingLieType\\\": \\\"Rough\\\",\\r\\n            \\\"strokesGained\\\": -0.38\\r\\n        },\\r\\n        {\\r\\n            \\\"remainingDistance\\\": 7.1,\\r\\n            \\\"startingDistanceToHole\\\": 91.96,\\r\\n            \\\"offsetAngle\\\": 51,\\r\\n            \\\"shotId\\\": 74365,\\r\\n            \\\"clubId\\\": 20823179,\\r\\n            \\\"scorecardId\\\": 17756,\\r\\n            \\\"holeNumber\\\": 8,\\r\\n            \\\"startingLieType\\\": \\\"Fairway\\\",\\r\\n            \\\"endingLieType\\\": \\\"Green\\\",\\r\\n            \\\"strokesGained\\\": -0.06\\r\\n        },\\r\\n        {\\r\\n            \\\"remainingDistance\\\": 18.35,\\r\\n            \\\"startingDistanceToHole\\\": 174.32,\\r\\n            \\\"offsetAngle\\\": 299,\\r\\n            \\\"shotId\\\": 74377,\\r\\n            \\\"clubId\\\": 20823169,\\r\\n            \\\"scorecardId\\\": 17756,\\r\\n            \\\"holeNumber\\\": 11,\\r\\n            \\\"startingLieType\\\": \\\"TeeBox\\\",\\r\\n            \\\"endingLieType\\\": \\\"Rough\\\",\\r\\n            \\\"strokesGained\\\": -0.46\\r\\n        },\\r\\n        {\\r\\n            \\\"remainingDistance\\\": 8.59,\\r\\n            \\\"startingDistanceToHole\\\": 138.25,\\r\\n            \\\"offsetAngle\\\": 93,\\r\\n            \\\"shotId\\\": 74385,\\r\\n            \\\"clubId\\\": 20823175,\\r\\n            \\\"scorecardId\\\": 17756,\\r\\n            \\\"holeNumber\\\": 13,\\r\\n            \\\"startingLieType\\\": \\\"Rough\\\",\\r\\n            \\\"endingLieType\\\": \\\"Green\\\",\\r\\n            \\\"strokesGained\\\": 0.27\\r\\n        },\\r\\n        {\\r\\n            \\\"remainingDistance\\\": 7.05,\\r\\n            \\\"startingDistanceToHole\\\": 98.55,\\r\\n            \\\"offsetAngle\\\": 23,\\r\\n            \\\"shotId\\\": 74405,\\r\\n            \\\"clubId\\\": 20823179,\\r\\n            \\\"scorecardId\\\": 17756,\\r\\n            \\\"holeNumber\\\": 18,\\r\\n            \\\"startingLieType\\\": \\\"Fairway\\\",\\r\\n            \\\"endingLieType\\\": \\\"Green\\\",\\r\\n            \\\"strokesGained\\\": -0.04\\r\\n        },\\r\\n        {\\r\\n            \\\"remainingDistance\\\": 140.83,\\r\\n            \\\"startingDistanceToHole\\\": 313.26,\\r\\n            \\\"offsetAngle\\\": 185,\\r\\n            \\\"shotId\\\": 74484,\\r\\n            \\\"clubId\\\": 20823044,\\r\\n            \\\"scorecardId\\\": 17758,\\r\\n            \\\"holeNumber\\\": 2,\\r\\n            \\\"startingLieType\\\": \\\"Rough\\\",\\r\\n            \\\"endingLieType\\\": \\\"Fairway\\\",\\r\\n            \\\"strokesGained\\\": 0.08\\r\\n        },\\r\\n        {\\r\\n            \\\"remainingDistance\\\": 39.39,\\r\\n            \\\"startingDistanceToHole\\\": 140.83,\\r\\n            \\\"offsetAngle\\\": 166,\\r\\n            \\\"shotId\\\": 74485,\\r\\n            \\\"clubId\\\": 0,\\r\\n            \\\"scorecardId\\\": 17758,\\r\\n            \\\"holeNumber\\\": 2,\\r\\n            \\\"startingLieType\\\": \\\"Fairway\\\",\\r\\n            \\\"endingLieType\\\": \\\"Rough\\\",\\r\\n            \\\"strokesGained\\\": -0.84\\r\\n        },\\r\\n        {\\r\\n            \\\"remainingDistance\\\": 12.07,\\r\\n            \\\"startingDistanceToHole\\\": 132.14,\\r\\n            \\\"offsetAngle\\\": 32,\\r\\n            \\\"shotId\\\": 74489,\\r\\n            \\\"clubId\\\": 20823054,\\r\\n            \\\"scorecardId\\\": 17758,\\r\\n            \\\"holeNumber\\\": 3,\\r\\n            \\\"startingLieType\\\": \\\"TeeBox\\\",\\r\\n            \\\"endingLieType\\\": \\\"Green\\\",\\r\\n            \\\"strokesGained\\\": -0.07\\r\\n        },\\r\\n        {\\r\\n            \\\"remainingDistance\\\": 21.51,\\r\\n            \\\"startingDistanceToHole\\\": 136.83,\\r\\n            \\\"offsetAngle\\\": 181,\\r\\n            \\\"shotId\\\": 74496,\\r\\n            \\\"clubId\\\": 0,\\r\\n            \\\"scorecardId\\\": 17758,\\r\\n            \\\"holeNumber\\\": 4,\\r\\n            \\\"startingLieType\\\": \\\"Rough\\\",\\r\\n            \\\"endingLieType\\\": \\\"Bunker\\\",\\r\\n            \\\"strokesGained\\\": -0.38\\r\\n        },\\r\\n        {\\r\\n            \\\"remainingDistance\\\": 43.14,\\r\\n            \\\"startingDistanceToHole\\\": 187.78,\\r\\n            \\\"offsetAngle\\\": 170,\\r\\n            \\\"shotId\\\": 74501,\\r\\n            \\\"clubId\\\": 0,\\r\\n            \\\"scorecardId\\\": 17758,\\r\\n            \\\"holeNumber\\\": 5,\\r\\n            \\\"startingLieType\\\": \\\"Fairway\\\",\\r\\n            \\\"endingLieType\\\": \\\"Rough\\\",\\r\\n            \\\"strokesGained\\\": -0.61\\r\\n        },\\r\\n        {\\r\\n            \\\"remainingDistance\\\": 26.11,\\r\\n            \\\"startingDistanceToHole\\\": 160.94,\\r\\n            \\\"offsetAngle\\\": 226,\\r\\n            \\\"shotId\\\": 74505,\\r\\n            \\\"clubId\\\": 0,\\r\\n            \\\"scorecardId\\\": 17758,\\r\\n            \\\"holeNumber\\\": 6,\\r\\n            \\\"startingLieType\\\": \\\"Fairway\\\",\\r\\n            \\\"endingLieType\\\": \\\"Rough\\\",\\r\\n            \\\"strokesGained\\\": -0.61\\r\\n        },\\r\\n        {\\r\\n            \\\"remainingDistance\\\": 152.01,\\r\\n            \\\"startingDistanceToHole\\\": 299.11,\\r\\n            \\\"offsetAngle\\\": 172,\\r\\n            \\\"shotId\\\": 74513,\\r\\n            \\\"clubId\\\": 20823044,\\r\\n            \\\"scorecardId\\\": 17758,\\r\\n            \\\"holeNumber\\\": 8,\\r\\n            \\\"startingLieType\\\": \\\"TeeBox\\\",\\r\\n            \\\"endingLieType\\\": \\\"Rough\\\",\\r\\n            \\\"strokesGained\\\": -0.43\\r\\n        },\\r\\n        {\\r\\n            \\\"remainingDistance\\\": 64.21,\\r\\n            \\\"startingDistanceToHole\\\": 152.01,\\r\\n            \\\"offsetAngle\\\": 194,\\r\\n            \\\"shotId\\\": 74514,\\r\\n            \\\"clubId\\\": 0,\\r\\n            \\\"scorecardId\\\": 17758,\\r\\n            \\\"holeNumber\\\": 8,\\r\\n            \\\"startingLieType\\\": \\\"Rough\\\",\\r\\n            \\\"endingLieType\\\": \\\"Fairway\\\",\\r\\n            \\\"strokesGained\\\": -0.46\\r\\n        },\\r\\n        {\\r\\n            \\\"remainingDistance\\\": 16.17,\\r\\n            \\\"startingDistanceToHole\\\": 64.21,\\r\\n            \\\"offsetAngle\\\": 294,\\r\\n            \\\"shotId\\\": 74515,\\r\\n            \\\"clubId\\\": 20823056,\\r\\n            \\\"scorecardId\\\": 17758,\\r\\n            \\\"holeNumber\\\": 8,\\r\\n            \\\"startingLieType\\\": \\\"Fairway\\\",\\r\\n            \\\"endingLieType\\\": \\\"Rough\\\",\\r\\n            \\\"strokesGained\\\": -0.8\\r\\n        },\\r\\n        {\\r\\n            \\\"remainingDistance\\\": 106.64,\\r\\n            \\\"startingDistanceToHole\\\": 242.9,\\r\\n            \\\"offsetAngle\\\": 172,\\r\\n            \\\"shotId\\\": 74522,\\r\\n            \\\"clubId\\\": 0,\\r\\n            \\\"scorecardId\\\": 17758,\\r\\n            \\\"holeNumber\\\": 11,\\r\\n            \\\"startingLieType\\\": \\\"Rough\\\",\\r\\n            \\\"endingLieType\\\": \\\"Fairway\\\",\\r\\n            \\\"strokesGained\\\": -0.07\\r\\n        },\\r\\n        {\\r\\n            \\\"remainingDistance\\\": 12.75,\\r\\n            \\\"startingDistanceToHole\\\": 106.34,\\r\\n            \\\"offsetAngle\\\": 334,\\r\\n            \\\"shotId\\\": 74523,\\r\\n            \\\"clubId\\\": 0,\\r\\n            \\\"scorecardId\\\": 17758,\\r\\n            \\\"holeNumber\\\": 11,\\r\\n            \\\"startingLieType\\\": \\\"Fairway\\\",\\r\\n            \\\"endingLieType\\\": \\\"Rough\\\",\\r\\n            \\\"strokesGained\\\": -0.59\\r\\n        },\\r\\n        {\\r\\n            \\\"remainingDistance\\\": 107.18,\\r\\n            \\\"startingDistanceToHole\\\": 183.06,\\r\\n            \\\"offsetAngle\\\": 175,\\r\\n            \\\"shotId\\\": 74525,\\r\\n            \\\"clubId\\\": 0,\\r\\n            \\\"scorecardId\\\": 17758,\\r\\n            \\\"holeNumber\\\": 12,\\r\\n            \\\"startingLieType\\\": \\\"TeeBox\\\",\\r\\n            \\\"endingLieType\\\": \\\"Rough\\\",\\r\\n            \\\"strokesGained\\\": -0.94\\r\\n        },\\r\\n        {\\r\\n            \\\"remainingDistance\\\": 27.04,\\r\\n            \\\"startingDistanceToHole\\\": 107.18,\\r\\n            \\\"offsetAngle\\\": 2,\\r\\n            \\\"shotId\\\": 74526,\\r\\n            \\\"clubId\\\": 0,\\r\\n            \\\"scorecardId\\\": 17758,\\r\\n            \\\"holeNumber\\\": 12,\\r\\n            \\\"startingLieType\\\": \\\"Rough\\\",\\r\\n            \\\"endingLieType\\\": \\\"Rough\\\",\\r\\n            \\\"strokesGained\\\": -0.61\\r\\n        },\\r\\n        {\\r\\n            \\\"remainingDistance\\\": 28.01,\\r\\n            \\\"startingDistanceToHole\\\": 159.15,\\r\\n            \\\"offsetAngle\\\": 153,\\r\\n            \\\"shotId\\\": 74531,\\r\\n            \\\"clubId\\\": 0,\\r\\n            \\\"scorecardId\\\": 17758,\\r\\n            \\\"holeNumber\\\": 13,\\r\\n            \\\"startingLieType\\\": \\\"Fairway\\\",\\r\\n            \\\"endingLieType\\\": \\\"Rough\\\",\\r\\n            \\\"strokesGained\\\": -0.65\\r\\n        },\\r\\n        {\\r\\n            \\\"remainingDistance\\\": 20.54,\\r\\n            \\\"startingDistanceToHole\\\": 64.44,\\r\\n            \\\"offsetAngle\\\": 51,\\r\\n            \\\"shotId\\\": 74535,\\r\\n            \\\"clubId\\\": 20823183,\\r\\n            \\\"scorecardId\\\": 17758,\\r\\n            \\\"holeNumber\\\": 14,\\r\\n            \\\"startingLieType\\\": \\\"Rough\\\",\\r\\n            \\\"endingLieType\\\": \\\"Bunker\\\",\\r\\n            \\\"strokesGained\\\": 0.29\\r\\n        },\\r\\n        {\\r\\n            \\\"remainingDistance\\\": 23.42,\\r\\n            \\\"startingDistanceToHole\\\": 139.37,\\r\\n            \\\"offsetAngle\\\": 198,\\r\\n            \\\"shotId\\\": 74540,\\r\\n            \\\"clubId\\\": 0,\\r\\n            \\\"scorecardId\\\": 17758,\\r\\n            \\\"holeNumber\\\": 15,\\r\\n            \\\"startingLieType\\\": \\\"Fairway\\\",\\r\\n            \\\"endingLieType\\\": \\\"Bunker\\\",\\r\\n            \\\"strokesGained\\\": -0.64\\r\\n        },\\r\\n        {\\r\\n            \\\"remainingDistance\\\": 25.73,\\r\\n            \\\"startingDistanceToHole\\\": 219.83,\\r\\n            \\\"offsetAngle\\\": 7,\\r\\n            \\\"shotId\\\": 74545,\\r\\n            \\\"clubId\\\": 0,\\r\\n            \\\"scorecardId\\\": 17758,\\r\\n            \\\"holeNumber\\\": 16,\\r\\n            \\\"startingLieType\\\": \\\"Fairway\\\",\\r\\n            \\\"endingLieType\\\": \\\"Bunker\\\",\\r\\n            \\\"strokesGained\\\": -0.17\\r\\n        },\\r\\n        {\\r\\n            \\\"remainingDistance\\\": 91.42,\\r\\n            \\\"startingDistanceToHole\\\": 277.39,\\r\\n            \\\"offsetAngle\\\": 155,\\r\\n            \\\"shotId\\\": 74549,\\r\\n            \\\"clubId\\\": 20823044,\\r\\n            \\\"scorecardId\\\": 17758,\\r\\n            \\\"holeNumber\\\": 17,\\r\\n            \\\"startingLieType\\\": \\\"TeeBox\\\",\\r\\n            \\\"endingLieType\\\": \\\"Rough\\\",\\r\\n            \\\"strokesGained\\\": -0.29\\r\\n        },\\r\\n        {\\r\\n            \\\"remainingDistance\\\": 12.03,\\r\\n            \\\"startingDistanceToHole\\\": 91.42,\\r\\n            \\\"offsetAngle\\\": 87,\\r\\n            \\\"shotId\\\": 74550,\\r\\n            \\\"clubId\\\": 20823056,\\r\\n            \\\"scorecardId\\\": 17758,\\r\\n            \\\"holeNumber\\\": 17,\\r\\n            \\\"startingLieType\\\": \\\"Rough\\\",\\r\\n            \\\"endingLieType\\\": \\\"Green\\\",\\r\\n            \\\"strokesGained\\\": -0.03\\r\\n        },\\r\\n        {\\r\\n            \\\"remainingDistance\\\": 141.32,\\r\\n            \\\"startingDistanceToHole\\\": 319.71,\\r\\n            \\\"offsetAngle\\\": 178,\\r\\n            \\\"shotId\\\": 74554,\\r\\n            \\\"clubId\\\": 20823044,\\r\\n            \\\"scorecardId\\\": 17758,\\r\\n            \\\"holeNumber\\\": 18,\\r\\n            \\\"startingLieType\\\": \\\"TeeBox\\\",\\r\\n            \\\"endingLieType\\\": \\\"Fairway\\\",\\r\\n            \\\"strokesGained\\\": -0.06\\r\\n        },\\r\\n        {\\r\\n            \\\"remainingDistance\\\": 22.95,\\r\\n            \\\"startingDistanceToHole\\\": 141.32,\\r\\n            \\\"offsetAngle\\\": 150,\\r\\n            \\\"shotId\\\": 74555,\\r\\n            \\\"clubId\\\": 0,\\r\\n            \\\"scorecardId\\\": 17758,\\r\\n            \\\"holeNumber\\\": 18,\\r\\n            \\\"startingLieType\\\": \\\"Fairway\\\",\\r\\n            \\\"endingLieType\\\": \\\"Rough\\\",\\r\\n            \\\"strokesGained\\\": -0.68\\r\\n        }\\r\\n    ]\\r\\n}";
	}
}
