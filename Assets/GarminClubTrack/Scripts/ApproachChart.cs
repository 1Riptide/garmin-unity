using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;
using UnityEngine.UI;

public class ApproachChart : MonoBehaviour, IGarmin3DChart, IGarminNestedChart
{
	enum LieTypes
	{
		Unknown,
		Teebox,
		Rough,
		Bunker,
		Fairway,
		Green,
		Waste
	};

	private string defaultPercentage = "--";
	// Default shot object.
	public GameObject whiteDataPoint;
	public GameObject redDataPoint;
	public GameObject chartGameObject;
	public static GameObject[] dataPoints;

	public bool isFocused { get; set; }
	// is user looking here?
	public bool isDefaultState { get; set; }
	// assuming chart has multiples. Can be anything.

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

	// Use this for initialization
	void Start ()
	{
		isDefaultState = true;
		MockInitialize ();	
	}

	public void MockInitialize ()
	{
		// This must be called by external platform. Pass JSON.
		Initialize (getMockJSON ());
	}

	public void Initialize (String json)
	{
		if (json == null || json.Length == 0) {
			Debug.Log ("Exception calling AddDataPoints : ");
		} else {
			try {
				Debug.Log ("Initialize() json is not null. Casting to JSON obj...");
				UpdateApproachStats (json);
			} catch (Exception e) {
				Debug.Log ("Exception parsing JSON : " + e);
			}
		}
	}

	void UpdateApproachStats (String json)
	{
		var clubTrackApproachData = JSON.Parse (json);
		bool usingClubTrack = clubTrackApproachData ["usingClubtrack"];

		Debug.Log ("UpdateApproachStats - json = \n" + usingClubTrack);
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
			var hitGreen10Percent = clubTrackApproachData ["percentHitGreen10"];
			var hitGreen20Percent = clubTrackApproachData ["percentHitGreen20"];
			var hitGreen20PlusPercent = clubTrackApproachData ["percentHitGreen20Plus"];

			percentHitGreen10.text = (hitGreen10Percent != null && hitGreen10Percent != "0") ? hitGreen10Percent + "%" : defaultPercentage + "%";
			percentHitGreen20.text = (hitGreen20Percent != null && hitGreen20Percent != "0") ? hitGreen20Percent + "%" : defaultPercentage + "%";
			percentHitGreen30.text = (hitGreen20PlusPercent != null && hitGreen20PlusPercent != "0") ? hitGreen20PlusPercent + "%" : defaultPercentage + "%";

		} else {
			hide4NonClubTrack.SetActive (false);
			var hitGreenPercent = clubTrackApproachData ["percentHitGreen"];
			percentHitGreen10.text = (hitGreenPercent != null && hitGreenPercent != "0") ? hitGreenPercent + "%" : defaultPercentage + "%";
		}
		var shortOfGreenPercent = clubTrackApproachData ["percentShortOfGreen"];
		var longOfGreenPercent = clubTrackApproachData ["percentLongOfGreen"];
		var leftOfGreenPercent = clubTrackApproachData ["percentLeftOfGreen"];
		var rightOfGreenPercent = clubTrackApproachData ["percentRightOfGreen"];

		missedGreenShort.text = (shortOfGreenPercent != null && shortOfGreenPercent != "0") ? shortOfGreenPercent + "%" : defaultPercentage + "%";
		missedGreenLong.text = (longOfGreenPercent != null && longOfGreenPercent != "0") ? longOfGreenPercent + "%" : defaultPercentage + "%";
		missedGreenLeft.text = (leftOfGreenPercent != null && leftOfGreenPercent != "0") ? leftOfGreenPercent + "%" : defaultPercentage + "%"; 
		missedGreenRight.text = (rightOfGreenPercent != null && rightOfGreenPercent != "0") ? rightOfGreenPercent + "%" : defaultPercentage + "%"; 

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

		return "{\n" + "  \"numberOfRounds\": 0,\n" + "  \"percentHitGreen10\": 0,\n" + "  \"percentHitGreen20\": 0,\n" +

		"  \"percentHitGreen20Plus\": 0,\n" + "  \"percentMissedGreen\": 0,\n" + "  \"percentShortOfGreen\": 0,\n" +

		"  \"percentLongOfGreen\": 0,\n" + "  \"percentLeftOfGreen\": 0,\n" + "  \"percentRightOfGreen\": 0,\n" +

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
