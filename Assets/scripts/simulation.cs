using UnityEngine;
using System.Collections;

public class simulation : MonoBehaviour {

	public GameObject pitchUpMaxTransform;
	public GameObject pitchDownMaxTransform;
	float pitchDownMax;
	float pitchUpMax;
	bool isPitchingDown = true;
	// Use this for initialization
	void Start () {
		pitchDownMax = Quaternion.Angle (transform.rotation,  pitchDownMaxTransform.transform.rotation);
		pitchUpMax = Quaternion.Angle (transform.rotation, pitchUpMaxTransform.transform.rotation);
	}

	// Update is called once per frame
	void Update () {
		pitchDownMax = Quaternion.Angle (transform.rotation,  pitchDownMaxTransform.transform.rotation);
		pitchUpMax = Quaternion.Angle (transform.rotation, pitchUpMaxTransform.transform.rotation);
		if (isPitchingDown) {
			Debug.Log ("Pitching Down. Rotation : pitchDownMax" + pitchDownMax + " current pitch : ");
			if (pitchDownMax > 2) {
				// Rotate the object around its local X axis at 10 degrees per second
				transform.Rotate (Vector3.right * (Time.deltaTime * 10), Space.World);
			} else {
				// Go the other way.
				isPitchingDown = false;
			}
		} else {
			if (pitchUpMax > 1) {
				// Rotate the object around its local X axis at 10 degrees per second
				transform.Rotate (Vector3.left * (Time.deltaTime * 10), Space.World);
			} else {
				// Go the other way.
				isPitchingDown = true;
			}
		}
	}
}
