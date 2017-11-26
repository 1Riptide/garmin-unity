using UnityEngine;
using System.Collections;

public class ShotDistroChart : MonoBehaviour {

	// Default shot object.
	public GameObject dataPoint;
	// For comera focus.
	public GameObject target;
	// min/max/mean
	int maxY;
	int minY;
	int avgY;

	// Use this for initialization
	void Start () {
		StartCoroutine (addDataPoints ());
		callToAndroid ();
	}

	IEnumerator addDataPoints(){
		int shotCount = 150;
		for (int i = 0; i < shotCount; i++) {
			// x range is 8 thru -8
			// y range is 14 thru -14
			yield return new WaitForSeconds(0);
			addDataPoint(new Vector3(Random.Range(-8.0f, 8.0f), 0, Random.Range(-14.0f, 14.0f)));
		}
	}

	static void callToAndroid(){
		AndroidJavaClass pluginClass = new AndroidJavaClass("com.garmin.android.apps.golf.ui.fragments.shottrack.ShotTrackFragment");
		//print("callToAndroid called() : " + pluginClass.CallStatic<string>("callFromUnity"));
	}

	void addDataPoint(Vector3 location){
		Instantiate(dataPoint, location, Quaternion.identity);
	}
}
