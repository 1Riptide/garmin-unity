using UnityEngine;
using System.Collections;
/*
 * Maps Json to C# Object for Shot Dispersion Shot Data.
{
  "numberOfRounds": 0,
  "percentFairwayLeft": 0,
  "percentFairwayRight": 0,
  "percentFairwayHit": 0,
  "minShotDistance": 0,
  "maxShotDistance": 0,
  "avgShotDistance": 0,
  "minDispersionDistance": 0,
  "maxDispersionDistance": 0,
  "shotDispersionDetails": [
    {
      "shotId": 0,
      "scorecardId": 0,
      "holeNumber": 0,
      "shotTime": "2017-12-05",
      "clubId": 0,
      "dispersionDistance": 0,
      "shotDistance": 0,
      "fairwayShotOutcome": "LEFT"
    }
  ]
}
*/
[System.Serializable]
public class ClubTrackDriveDataDTO
{
	public int numberOfRounds;
	public float percentFairwayLeft { get; set;}
	public float percentFairwayRight { get; set;}
	public float percentFairwayHit { get; set;}
	public float minShotDistance { get; set;}
	public float maxShotDistance { get; set;}
	public float avgShotDistance { get; set;}
	public float minDispersionDistance { get; set;}
	public float maxDispersionDistance { get; set;}
	public ShotDispersionDetail[] shotDispersionDetails;

	public static ClubTrackDriveDataDTO CreateFromJSON(string jsonString)
	{
		return JsonUtility.FromJson<ClubTrackDriveDataDTO>(jsonString);
	}
}

[System.Serializable]
public class ShotDispersionDetail
{
	public int shotId { get; set;}
	public int scorecardId { get; set;}
	public int holeNumber { get; set;}
	public string shotTime { get; set;}
	public int clubId { get; set;}
	public float dispersionDistance { get; set;}
	public float shotDistance { get; set;}
	public string fairwayShotOutcome { get; set;}
	public static ShotDispersionDetail CreateFromJSON(string jsonString)
	{
		return JsonUtility.FromJson<ShotDispersionDetail>(jsonString);
	}
}