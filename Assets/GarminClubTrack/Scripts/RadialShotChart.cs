using System;
using UnityEngine;
using System.Collections;
using SimpleJSON;
using System.Linq;

public class RadialShotChart : MonoBehaviour
{
	public enum LieTypes
	{
		Unknown,
		Teebox,
		Rough,
		Bunker,
		Fairway,
		Green,
		Waste
	};
	// Default. Override in editor.
	public float maxRadialDistance = 19f;	
	public GameObject chartGameObject;
	public GameObject hitDataPointPrefab;
	public GameObject missDataPointPrefab;
	protected GameObject[] dataPoints;
	public bool isFocused { get; set; }

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
		return Instantiate (dataPoint, location, Quaternion.identity);
	}

	public float[] createDistanceLog (JSONNode data, String nodeName, String propertyName)
	{
		int nodeCount = data.Count;
		float[] shotDistanceLog = new float[nodeCount];
		var shotData = data [nodeName];
		for (int i = 0; i < nodeCount; i++) {
			JSONNode thisDetail = shotData [i];
			// Hard coded assumption. 
			shotDistanceLog [i] = thisDetail [propertyName];
		}
		return shotDistanceLog;
	}
}