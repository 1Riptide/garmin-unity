using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Will force GameObject to always face the camera, while appearing flat against background.
 */
public class ChartTextBehavior : MonoBehaviour {

	void Update () {
		transform.LookAt (Camera.main.transform.position);
		Vector3 targetPostition = new Vector3( transform.position.x, 
			Camera.main.transform.position.y, 
			Camera.main.transform.position.z) ;
		transform.LookAt(targetPostition);
		bool isFlipped = false;

		// Southern latitudes (relative to camera) will flip vertically to face camera as it 
		// passed over head. This little nugget will prevent that.
		if (transform.rotation.x < 0) {
			isFlipped = true;
			transform.Rotate (new Vector3 (transform.rotation.x+180,0, 0));
		} else {
			transform.Rotate (Vector3.up - new Vector3 (0, 180, 0));
		}

		// Clamp rotation to prevent showing backside or flipping under foundation plane.
		Vector3 currentRotation = transform.localRotation.eulerAngles;
		if (isFlipped) {
			currentRotation.x = Mathf.Clamp (currentRotation.x, 90, 90);
		} else {
			currentRotation.x = Mathf.Clamp(currentRotation.x, 0, 90);
		}

		currentRotation.y = Mathf.Clamp(currentRotation.y, 0, 0);
		currentRotation.z = Mathf.Clamp(currentRotation.z, 0, 0);
		transform.localRotation = Quaternion.Euler (currentRotation);
	}
}
