using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;
using UnityEngine.UI;

public class ApproachChart : MonoBehaviour, IGarmin3DChart, IGarminNestedChart
{
	private string defaultPercentage = "--";
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

	// Formatting
	private string floatFormat = "#.00";
	private string precisionPattern = ".00";

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
				scatterChartGameObject.GetComponent<ApproachChartWithShots> ().Initialize(json);
			} catch (Exception e) {
				Debug.Log ("ApproachChart Exception parsing JSON : " + e);
			}
		}
	}

	void UpdateApproachStats (String json)
	{
		var clubTrackApproachData = JSON.Parse (json);
		bool usingClubTrack = clubTrackApproachData ["usingClubtrack"];

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

			var hitGreen10PercentStr = hitGreen10Percent.ToString(floatFormat);
			var hitGreen20PercentStr = hitGreen20Percent.ToString(floatFormat);
			var hitGreen20PlusPercentStr = hitGreen20PlusPercent.ToString(floatFormat);

			percentHitGreen10.text = (hitGreen10Percent != null && hitGreen10PercentStr != precisionPattern) ? hitGreen10PercentStr + "%" : defaultPercentage + "%";
			percentHitGreen20.text = (hitGreen20Percent != null && hitGreen20PercentStr != precisionPattern) ? hitGreen20PercentStr + "%" : defaultPercentage + "%";
			percentHitGreen30.text = (hitGreen20PlusPercent != null && hitGreen20PlusPercentStr != precisionPattern) ? hitGreen20PlusPercentStr + "%" : defaultPercentage + "%";

		} else {
			hide4NonClubTrack.SetActive (false);
			float hitGreenPercent = clubTrackApproachData ["percentHitGreen"];
			var hitGreeenPercentStr = hitGreenPercent.ToString(floatFormat);

			Debug.Log ("ApproachChart UpdateApproachStats - NO CLUB TRACK. percentHitGreen = " + hitGreenPercent);
			percentHitGreen10.text = (hitGreenPercent != null && hitGreeenPercentStr != precisionPattern) ? hitGreeenPercentStr + "%" : defaultPercentage + "%";
		}
		float shortOfGreenPercent = clubTrackApproachData ["percentShortOfGreen"];
		float longOfGreenPercent = clubTrackApproachData ["percentLongOfGreen"];
		float leftOfGreenPercent = clubTrackApproachData ["percentLeftOfGreen"];
		float rightOfGreenPercent = clubTrackApproachData ["percentRightOfGreen"];

		var shortOfGreenPercentStr = shortOfGreenPercent.ToString(floatFormat);
		var longOfGreenPercentStr = longOfGreenPercent.ToString(floatFormat);
		var leftOfGreenPercentStr = leftOfGreenPercent.ToString(floatFormat);
		var rightOfGreenPercentStr = rightOfGreenPercent.ToString(floatFormat);
		Debug.Log ("ApproachChart UpdateApproachStats - " + longOfGreenPercentStr);

		missedGreenShort.text = (shortOfGreenPercent != null && shortOfGreenPercentStr != precisionPattern) ? shortOfGreenPercentStr + "%" : defaultPercentage + "%";
		missedGreenLong.text = (longOfGreenPercent != null && longOfGreenPercentStr != precisionPattern) ? longOfGreenPercentStr + "%" : defaultPercentage + "%";
		missedGreenLeft.text = (leftOfGreenPercent != null && leftOfGreenPercentStr != precisionPattern) ? leftOfGreenPercentStr + "%" : defaultPercentage + "%"; 
		missedGreenRight.text = (rightOfGreenPercent != null && rightOfGreenPercentStr != precisionPattern) ? rightOfGreenPercentStr + "%" : defaultPercentage + "%"; 

	}
		
	/** Approach GCS Contract as of 1/24/18
	 {
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

		return "{\n" + "  \"numberOfRounds\": 0,\n"+ "  \"percentHitGreen\": 20,\n"  + "  \"percentHitGreen10\": 0,\n" + "  \"percentHitGreen20\": 0,\n" +

		"  \"percentHitGreen20Plus\": 10.333333,\n" + "  \"percentMissedGreen\": 0,\n" + "  \"percentShortOfGreen\": 0,\n" +

		"  \"percentLongOfGreen\": 10.222222,\n" + "  \"percentLeftOfGreen\": 0,\n" + "  \"percentRightOfGreen\": 0,\n" +

			"  \"percentGreenInRegulation\": 0,\n" + "  \"shotOrientationDetail\": [\n" + "    {\n" + "      \"remainingDistance\": 50,\n" +

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
