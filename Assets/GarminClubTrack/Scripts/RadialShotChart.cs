using System;
using UnityEngine;
using System.Collections;
using SimpleJSON;
using System.Linq;

public class RadialShotChart : MonoBehaviour
{
	protected enum LieTypes
	{
		Unknown,
		Teebox,
		Rough,
		Bunker,
		Fairway,
		Green,
		Waste
	};
		
	public GameObject chartGameObject;
	public GameObject hitDataPointPrefab;
	public GameObject missDataPointPrefab;
	protected GameObject[] dataPoints;
	public float maxRadialDistance = 19f;
	private Vector3 origin;

	public void Cleanup ()
	{
		if (dataPoints != null) {
			foreach (GameObject data in dataPoints) {
				Destroy (data);
			}
		}
	}

	public GameObject AddDataPoint (GameObject dataPoint, Vector3 location)
	{
		Debug.Log ("AddDataPoint : location.x = " + location.x + "  -  location.y = " + location.y);
		return Instantiate (dataPoint, origin + location, Quaternion.identity);
	}

	public float[] createDistanceLog (JSONNode data, String nodeName)
	{
		int nodeCount = data.Count;
		float[] shotDistanceLog = new float[nodeCount];
		var shotData = data [nodeName];
		Debug.Log ("createDistanceLog Json = " + data.ToString());
		for (int i = 0; i < nodeCount; i++) {
			JSONNode thisDetail = shotData [i];
			// Hard coded assumption.
			shotDistanceLog [i] = thisDetail ["remainingDistance"];
		}
		return shotDistanceLog;
	}
}