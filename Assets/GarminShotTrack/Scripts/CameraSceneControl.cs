using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSceneControl : MonoBehaviour {

	enum SceneName {DRIVE, APPROACH, CHIPPING, PUTTING};

	public Transform cameraRotationLookingDown;
	public float cameraTransitionSpeed;
	public GameObject dispersionScene;
	public GameObject approachScene;
	public GameObject chippingScene;
	public GameObject puttingScene;
	public GameObject defaultScene;
	SceneName currentSceneName;
	// Used to determine if we are looking straight down at the chart, or from an angle.
	public static bool isCameraToggledDown = false;

	CameraTouchControl touchController;
	bool isTransitioning = false;

	void Awake() {
		touchController = GetComponent<CameraTouchControl>();
	}
		
	// Update is called once per frame
	void Update () {
		Transform target = defaultScene.transform.Find ("CameraTarget");
		if (!isTransitioning && defaultScene.Equals (dispersionScene)) {
			if (!isCameraToggledDown) {
				if (!touchController.singleClick) {
					Camera.main.transform.rotation = Quaternion.Lerp (Camera.main.transform.rotation, target.rotation, touchController.cameraDoubleTapTransitionSpeed * Time.deltaTime);
					Camera.main.transform.position = Vector3.Lerp (Camera.main.transform.position, target.position, touchController.cameraDoubleTapTransitionSpeed * Time.deltaTime);
				} 
			} else {
				if (!touchController.singleClick) {
					Camera.main.transform.rotation = Quaternion.Lerp (Camera.main.transform.rotation, cameraRotationLookingDown.rotation, touchController.cameraDoubleTapTransitionSpeed * Time.deltaTime);
					Camera.main.transform.position = Vector3.Lerp (Camera.main.transform.position, cameraRotationLookingDown.position, touchController.cameraDoubleTapTransitionSpeed * Time.deltaTime);
				}
			}
		} else {
			Camera.main.transform.rotation = Quaternion.Lerp (Camera.main.transform.rotation, target.rotation, cameraTransitionSpeed );
			Camera.main.transform.position = Vector3.Lerp (Camera.main.transform.position, target.position, cameraTransitionSpeed );
			if (Camera.main.transform.position == target.position) {
				isTransitioning = false;
			}
		}
	}

	public void ChangeScene(string sceneName){
		Debug.Log ("ChangeScene() called in Unity! sceneName = " + sceneName);
		SceneName newSceneName = (SceneName) System.Enum.Parse( typeof( SceneName ), sceneName );
		isTransitioning = true;
		//defaultScene.isEnabled = false;
		if (newSceneName != null) {
			currentSceneName = newSceneName;
			switch (newSceneName) {
			case SceneName.APPROACH:
				defaultScene = approachScene;
				break;
			case SceneName.CHIPPING:
				defaultScene = chippingScene;
				break;
			case SceneName.PUTTING:
				defaultScene = puttingScene;
				break;
			// fallthrough is intentional
			case SceneName.DRIVE:
			default :
				defaultScene = dispersionScene;
				break;
			}
		}
	}

	public void ToggleCameraAngle(){
		isCameraToggledDown = !isCameraToggledDown;
	}

	// Force singleclick by calling with signature.
	public void ToggleCameraAngle(bool force){
		isCameraToggledDown = !isCameraToggledDown;
	}

}
