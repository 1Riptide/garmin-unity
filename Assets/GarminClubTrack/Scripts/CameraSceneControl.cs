using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSceneControl : MonoBehaviour
{

	enum SceneName
	{
		DRIVE,
		APPROACH,
		CHIPPING,
		PUTTING
	};

	private SceneName sceneNameEnum;
	public GameObject defaultScene;
	public GameObject driveScene;
	public GameObject approachScene;
	public GameObject chippingScene;
	public GameObject puttingScene;
	public float cameraTransitionSpeed = .7f;

	// Used to determine if we are looking straight down at the chart, or from an angle.
	public static bool isCameraToggledDown = false;

	CameraTouchControl touchController;
	public bool isIntroTransitioning = false;
	public bool isTransitioning = false;
	private bool isPuttingSceneReady = false;

	void Awake ()
	{
		touchController = GetComponent<CameraTouchControl> ();
		isIntroTransitioning = true;
	}

	void Start ()
	{
		#if UNITY_ANDROID
		Debug.Log ("SignalReady() : sent to Android.");
		AndroidJavaObject javaObj = new AndroidJavaObject ("com.garmin.android.apps.golf.ui.fragments.clubtrack.ClubTrackFragment");
		javaObj.Call ("onUnityInitialized", "");
		Screen.fullScreen = false;
		#endif
	}
		
	void Update ()
	{
		if (defaultScene != null) {
			Transform target = defaultScene.transform.Find ("CameraTarget");
			Transform topDownTarget = defaultScene.transform.Find ("CameraTargetTopDown");

			if (!isTransitioning) {
				// Not transitioning....
				switch (sceneNameEnum) {
				case SceneName.DRIVE:
					if (!isCameraToggledDown) {
						if (!touchController.singleClick) {
							Camera.main.transform.rotation = Quaternion.Lerp (Camera.main.transform.rotation, target.rotation, touchController.cameraDoubleTapTransitionSpeed * Time.deltaTime);
							Camera.main.transform.position = Vector3.Lerp (Camera.main.transform.position, target.position, touchController.cameraDoubleTapTransitionSpeed * Time.deltaTime);
						} 
					} else {
						if (!touchController.singleClick) {
							Camera.main.transform.rotation = Quaternion.Lerp (Camera.main.transform.rotation, topDownTarget.rotation, touchController.cameraDoubleTapTransitionSpeed * Time.deltaTime);
							Camera.main.transform.position = Vector3.Lerp (Camera.main.transform.position, topDownTarget.position, touchController.cameraDoubleTapTransitionSpeed * Time.deltaTime);
						}
					}
					break;
				case SceneName.APPROACH:
					if (!isCameraToggledDown) {
						if (!touchController.singleClick) {
							Camera.main.transform.rotation = Quaternion.Lerp (Camera.main.transform.rotation, topDownTarget.rotation, (touchController.cameraDoubleTapTransitionSpeed * 2) * Time.deltaTime);
							Camera.main.transform.position = Vector3.Lerp (Camera.main.transform.position, topDownTarget.position, (touchController.cameraDoubleTapTransitionSpeed * 2) * Time.deltaTime);
						} 
					} else {
						if (!touchController.singleClick) {
							Camera.main.transform.rotation = Quaternion.Lerp (Camera.main.transform.rotation, target.rotation, (touchController.cameraDoubleTapTransitionSpeed * 2) * Time.deltaTime);
							Camera.main.transform.position = Vector3.Lerp (Camera.main.transform.position, target.position, (touchController.cameraDoubleTapTransitionSpeed * 2) * Time.deltaTime);
						}
					}
					break;
				case SceneName.PUTTING:
					puttingSceneBehavior (target, topDownTarget);
					break;
				}
			} else {
				if (defaultScene.Equals (puttingScene)) {
					puttingSceneBehavior (target, topDownTarget);
				} else {
					// In middle of transition...do this.
					defaultSceneBehavior (target, topDownTarget);
				}
			}
		} else {
			// No default scene. Sleeping....
		}
	}

	void puttingSceneBehavior(Transform target, Transform topDownTarget){
		if (!isPuttingSceneReady) {
			if (Camera.main.transform.position != target.position || Camera.main.transform.rotation != target.rotation) {
				Camera.main.transform.rotation = Quaternion.Lerp (Camera.main.transform.rotation, target.rotation, cameraTransitionSpeed /3);
				Camera.main.transform.position = Vector3.Lerp (Camera.main.transform.position, target.position, cameraTransitionSpeed / 3);
			} else {
				Debug.Log ("Update: Putting Scene READY!");
				isPuttingSceneReady = true;
			}
		} else {
			// Behave as though transitioning (putts is special, has two transitons)
			defaultSceneBehavior (target, topDownTarget);
		}
	}

	void defaultSceneBehavior(Transform target, Transform topDownTarget){
		// We are transitioning...
		Transform targetTransform = target;
		if (defaultScene.Equals (puttingScene)) {
			targetTransform = topDownTarget;
			Debug.Log ("Update: Putting Scene Transisitoning.");
			isPuttingSceneReady = false;
		}
		if (defaultScene.Equals (approachScene)){
			targetTransform = topDownTarget;
		}

		// Slow it down for intro.
		var speed = cameraTransitionSpeed;
		if (isIntroTransitioning) {
			Debug.Log ("isINTRO TRANSITIONING>>>>SLOW!!");
			speed = .2f;
		}
		if (Camera.main.transform.position != targetTransform.position || Camera.main.transform.rotation != targetTransform.rotation) {
			Camera.main.transform.rotation = Quaternion.Lerp (Camera.main.transform.rotation, targetTransform.rotation, speed);
			Camera.main.transform.position = Vector3.Lerp (Camera.main.transform.position, targetTransform.position, speed);
		} else {
			isIntroTransitioning = false;
			isTransitioning = false;
		}
	}

	public void ChangeScene (string sceneName)
	{
		Debug.Log ("ChangeScene() called in Unity! sceneName = " + sceneName);
		// Reset isFocused flag on old scene.
		if (defaultScene != null) {
			IGarmin3DChart chartInterface = defaultScene.GetComponent (typeof(IGarmin3DChart)) as IGarmin3DChart;
			if (chartInterface != null) {
				chartInterface.isFocused = false;
			}
		}

		sceneNameEnum = (SceneName)System.Enum.Parse (typeof(SceneName), sceneName);
		isIntroTransitioning = false;
		isTransitioning = true;
		isPuttingSceneReady = false;
		switch (sceneNameEnum) {
			case SceneName.APPROACH:
				defaultScene = approachScene;	
				IGarminNestedChart nestedChartInterface = defaultScene.GetComponent (typeof(IGarminNestedChart)) as IGarminNestedChart;
				nestedChartInterface.isDefaultState = true;
				isCameraToggledDown = false;
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
			chartInterface.isFocused = true;
			touchController.ResetFOV ();
			Debug.Log ("ChangeScene : found scene reseting Camera Fov  - " + sceneNameEnum);
		} else {
			Debug.Log ("ChangeScene : could not find scene " + sceneNameEnum);
		}
	}

	public void ToggleCameraAngle ()
	{
		isCameraToggledDown = !isCameraToggledDown;
	}

	public bool getIsCameraToggledDown(){
		return isCameraToggledDown;
	}
}
