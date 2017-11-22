using UnityEngine;
using System.Collections;

public class ShotDistroChart : MonoBehaviour {

	public GameObject dataPoint;
	public GameObject center;
	// Light anim
	public Light lt;
	public float maxAngle;

	private bool isCameraLocked = true;

	// Speed of camera transition
	float speed = .8f;
	// Camera position when looking down at chart.
	Vector3 topPosition = new Vector3(0,28,0);

	// Use this for initialization
	void Start () {
		StartCoroutine (addDataPoints ());
		StartCoroutine (cameraAttitude ());
	}

	IEnumerator addDataPoints(){
		int shotCount = 750;
		for (int i = 0; i < shotCount; i++) {
			// x range is 8 thru -8
			// y range is 14 thru -14
			yield return new WaitForSeconds(Random.Range(0.001f, 0.01f));
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
		
	// Update is called once per frame
	void Update () {
		if (lt.spotAngle < maxAngle) {
			lt.spotAngle += 5 * Time.deltaTime;
		}

		if (!isCameraLocked) {
			Camera.main.transform.position = Vector3.Lerp (Camera.main.transform.position, topPosition, speed * Time.deltaTime);
		}
	}

	void LateUpdate() {
		Camera.main.transform.LookAt (center.transform);
	}
}
