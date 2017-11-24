using UnityEngine;
using System.Collections;

public class ShotDistroChart : MonoBehaviour {

	public GameObject dataPoint;
	public GameObject center;
	public GameObject maxText;
	public GameObject mainText;
	public GameObject midText;
	// Light anim
	public Light lt;
	public float maxAngle;
	// min/max/mean
	int maxY;
	int minY;
	int avgY;

	private bool isCameraLocked = true;

	// Speed of camera transition
	float speed = .8f;
	// Camera position when looking down at chart.
	Vector3 topPosition = new Vector3(0,28,0);

	// Use this for initialization
	void Start () {
		StartCoroutine (addDataPoints ());
		StartCoroutine (cameraAttitude());
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

	IEnumerator addDataPoint(Vector3 location){
		Instantiate(dataPoint, location, Quaternion.identity);
		return null;
	}

	IEnumerator cameraAttitude(){
		yield return new WaitForSeconds (4);
		isCameraLocked = false;
	}
		
	void Update () {
		if (!isCameraLocked) {
			if (lt.spotAngle < maxAngle) {
				lt.spotAngle += 5 * Time.deltaTime;
			}
			Camera.main.transform.position = Vector3.Lerp (Camera.main.transform.position, topPosition, speed * Time.deltaTime);
		}

		// max text
		// min text
		// mid text
	
		maxText.transform.LookAt (Camera.main.transform.position);
		Vector3 targetPostition = new Vector3( maxText.transform.position.x, 
			Camera.main.transform.position.y, 
			Camera.main.transform.position.z) ;
		maxText.transform.LookAt(targetPostition);
		maxText.transform.Rotate(Vector3.up - new Vector3(0,180,0));

	}

	void LateUpdate() {
		Camera.main.transform.LookAt (center.transform);
	}
}
