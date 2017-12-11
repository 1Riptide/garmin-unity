using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSceneControl : MonoBehaviour {

	enum SceneName {DRIVE, APPROACH, CHIPPING, PUTTING};

	public float cameraTransitionSpeed;
	public GameObject driveScene;
	public GameObject approachScene;
	public GameObject chippingScene;
	public GameObject puttingScene;
	public GameObject defaultScene;
	// Used to determine if we are looking straight down at the chart, or from an angle.
	public static bool isCameraToggledDown = false;

	CameraTouchControl touchController;
	public bool isTransitioning = false;

	void Awake() {
		touchController = GetComponent<CameraTouchControl>();
	}

	void Start(){
		//ChangeScene ("PUTTING");
		if (defaultScene != null) {
			Transform target = defaultScene.transform.Find ("CameraTarget");
			Camera.main.transform.rotation = target.rotation;
			Camera.main.transform.position = target.position;
		}
	}
		
	// Update is called once per frame
	void Update () {
		Transform target = defaultScene.transform.Find ("CameraTarget");
		if (!isTransitioning && defaultScene.Equals (driveScene)) {		
			if (!isCameraToggledDown) {
				if (!touchController.singleClick) {
					Camera.main.transform.rotation = Quaternion.Lerp (Camera.main.transform.rotation, target.rotation, touchController.cameraDoubleTapTransitionSpeed * Time.deltaTime);
					Camera.main.transform.position = Vector3.Lerp (Camera.main.transform.position, target.position, touchController.cameraDoubleTapTransitionSpeed * Time.deltaTime);
				} 
			} else {
				if (!touchController.singleClick) {
					Transform topDownTarget = defaultScene.transform.Find ("CameraTargetTopDown");
					Camera.main.transform.rotation = Quaternion.Lerp (Camera.main.transform.rotation, topDownTarget.rotation, touchController.cameraDoubleTapTransitionSpeed * Time.deltaTime);
					Camera.main.transform.position = Vector3.Lerp (Camera.main.transform.position, topDownTarget.position, touchController.cameraDoubleTapTransitionSpeed * Time.deltaTime);
				}
			}
		} else {
			if (isTransitioning) {
				if (Camera.main.transform.position != target.position || Camera.main.transform.rotation != target.rotation) {
					Camera.main.transform.rotation = Quaternion.Lerp (Camera.main.transform.rotation, target.rotation, cameraTransitionSpeed);
					Camera.main.transform.position = Vector3.Lerp (Camera.main.transform.position, target.position, cameraTransitionSpeed);
				} else {
					isTransitioning = false;
				}
			}
		}
	}

	public void ChangeScene(string sceneName){
		Debug.Log ("ChangeScene() called in Unity! sceneName = " + sceneName);
		SceneName newSceneName = (SceneName) System.Enum.Parse( typeof( SceneName ), sceneName );
		isTransitioning = true;
		touchController.SceneChanged ();
		//defaultScene.isEnabled = false;
		if (newSceneName != null) {
			
			switch (newSceneName) {
			case SceneName.APPROACH:
				isCameraToggledDown = true;
				defaultScene = approachScene;
				break;
			case SceneName.CHIPPING:
				isCameraToggledDown = true;
				defaultScene = chippingScene;
				break;
			case SceneName.PUTTING:
				isCameraToggledDown = true;
				defaultScene = puttingScene;
				break;
			// fallthrough is intentional
			case SceneName.DRIVE:
			default :
				isCameraToggledDown = false;
				defaultScene = driveScene;
				break;
			}
		}
	}

	public void ToggleCameraAngle(){
		isCameraToggledDown = !isCameraToggledDown;
	}
}
