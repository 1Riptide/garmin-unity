using UnityEngine;
using System.Collections;

public class ShotDistroChart : MonoBehaviour {

	// Default shot object.
	public GameObject dataPoint;
	// For comera focus.
	public GameObject center;
	// Light for anim.
	public Light lt;
	// Max angle to animate light to.
	public float maxAngle;
	// min/max/mean
	int maxY;
	int minY;
	int avgY;

	// To prevent camera from moving unitil a predetermined time.
	private bool isCameraLocked = true;
	public int aPredeterminedTime = 4;

	// Speed of camera transition
	float speed = .8f;
	// Camera position when looking down at chart.
	Vector3 topPosition = new Vector3(0,28,0);

	// Use this for initialization
	void Start () {
		StartCoroutine (addDataPoints ());
		StartCoroutine (cameraAttitude());
		print ("Start - calling Android! ");
		callToAndroid ();
	}

	IEnumerator addDataPoints(){
		int shotCount = 750;
		for (int i = 0; i < shotCount; i++) {
			// x range is 8 thru -8
			// y range is 14 thru -14
			yield return new WaitForSeconds(0);
			addDataPoint(new Vector3(Random.Range(-8.0f, 8.0f), 0, Random.Range(-14.0f, 14.0f)));
		}
	}

	static void callToAndroid(){
		AndroidJavaClass pluginClass = new AndroidJavaClass("com.garmin.android.golf");
		print("callToAndroid called() : " + pluginClass.CallStatic<string>("callFromUnity"));
	}

	void addDataPoint(Vector3 location){
		Instantiate(dataPoint, location, Quaternion.identity);
	}

	IEnumerator cameraAttitude(){
		yield return new WaitForSeconds (aPredeterminedTime);
		isCameraLocked = false;
	}
		
	void Update () {
		if (!isCameraLocked) {
			if (lt.spotAngle < maxAngle) {
				lt.spotAngle += 5 * Time.deltaTime;
			}
			Camera.main.transform.position = Vector3.Lerp (Camera.main.transform.position, topPosition, speed * Time.deltaTime);
		}
	}

	void LateUpdate() {
		Camera.main.transform.LookAt (center.transform);
	}
}
