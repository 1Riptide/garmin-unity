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
		touchController = GetComponent<CameraTouchControl> ();
	}
		
	void Start(){
		#if UNITY_ANDROID
		Debug.Log ("SignalReady() : sent to Android.");
		AndroidJavaObject javaObj = new AndroidJavaObject("com.garmin.android.apps.golf.ui.fragments.clubtrack.ClubTrackFragment");
		javaObj.Call("onUnityInitialized", "");
		#endif
		// Default
		//ChangeScene (SceneName.DRIVE.ToString());
	}

	// Update is called once per frame
	void Update () {
		if (defaultScene != null) {
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
		} else {
			// No default scene. Sleeping....
		}
	}

	public void ChangeScene(string sceneName){
		Debug.Log ("ChangeScene() called in Unity! sceneName = " + sceneName);
		if (defaultScene != null) {
			IGarmin3DChart chartInterface = defaultScene.GetComponent (typeof(IGarmin3DChart)) as IGarmin3DChart;
			if (chartInterface != null) {
				chartInterface.isEnabled = false;
			}
		}
		SceneName newSceneName = (SceneName) System.Enum.Parse( typeof( SceneName ), sceneName );
		isTransitioning = true;
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

			// Enable Scene
			if (defaultScene != null) {
				IGarmin3DChart chartInterface = defaultScene.GetComponent (typeof(IGarmin3DChart)) as IGarmin3DChart;
				chartInterface.isEnabled = true;
				touchController.SceneChanged ();
				Debug.Log ("ChangeScene : found scene reseting Camera Fov  - " + newSceneName);
			} else {
				Debug.Log ("ChangeScene : could not find scene " + newSceneName);
			}
		}
	}

	public void ToggleCameraAngle(){
		isCameraToggledDown = !isCameraToggledDown;
	}

	/*
	void FixedUpdate(){
		
		if (Input.GetKeyDown(KeyCode.Escape)) 
		{ 
			#if UNITY_ANDROID
			// Get the unity player activity
			AndroidJavaObject activity = 
				new AndroidJavaClass("com.unity3d.player.UnityPlayer")
					.GetStatic<AndroidJavaObject>("currentActivity");

			// call activity's boolean moveTaskToBack(boolean nonRoot) function
			// documentation: http://developer.android.com/reference/android/app/Activity.html#moveTaskToBack(boolean)
			activity.Call<bool>("moveTaskToBack", true);

			#endif
		}
	}
	*/
}
