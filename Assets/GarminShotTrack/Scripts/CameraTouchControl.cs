using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class CameraTouchControl : MonoBehaviour
{
	private Camera cam;
	private Vector3 lastPanPosition;
	private bool wasZoomingLastFrame; // Touch mode only
	private Vector2[] lastZoomPositions; // Touch mode only
	private static readonly float PanSpeed = 20f;
	private static readonly float ZoomSpeedTouch = 0.1f;
	private static readonly float ZoomSpeedMouse = 0.5f;
	private static readonly float[] positionBoundsX = new float[]{-8f, 8f};
	private static readonly float[] positionBoundsZ = new float[]{-36f, -12f};
	private static readonly float[] positionBoundsY = new float[]{28f, 28f};
	private static readonly float[] ZoomBounds = new float[]{10f, 80f};

	// For counting taps OR clicks.
	int inputCount = 0; 
	// Camera position when looking down at chart.
	public Vector3 cameraTopPosition = new Vector3(0,28,0);
	// Obtained at runtime.
	//public Vector3 cameraDefaultPosition; 
	// Speed of camera transition
	public float cameraDoubleTapTransitionSpeed =1.5f;
	public bool singleClick;
	float doubleClickTimeBasis = 0;
	// How long is considered a double click/touch
	float doubleClickThreshold = .35f;
	float defaultFOV;

	CameraSceneControl sceneController;

	void Awake() {
		cam = GetComponent<Camera>();
		defaultFOV = cam.fieldOfView;
		sceneController = GetComponent<CameraSceneControl>();
	}

	void Update() {
		if (sceneController.defaultScene != null && sceneController.defaultScene.Equals (sceneController.driveScene)) {
			if (Input.touchSupported && Application.platform != RuntimePlatform.WebGLPlayer) {
				HandleTouch ();
			} else {
				HandleMouse ();
			}
		}
	}

	void HandleTouch() {
		switch(Input.touchCount) {
		case 1: 
			// If the touch began, capture its position and its finger ID. Otherwise, if the finger ID of the touch doesn't match, skip it.
			Touch touch = Input.GetTouch (0);
			if (touch.phase == TouchPhase.Moved) {
				PanCamera (touch.position);
			} else if (touch.phase == TouchPhase.Began) {
				if (doubleClickTimeBasis == 0) {
					// Default state. No touches recorded.
					lastPanPosition = Input.mousePosition;
				} else {
					// Last known state was singleClick - and another click came is recorded. Test:
					if ((Time.time - doubleClickTimeBasis) > doubleClickThreshold) {
						singleClick = true;
						lastPanPosition = Input.mousePosition;
					} else {
						singleClick = false;
						sceneController.ToggleCameraAngle ();
					}
				} 
				doubleClickTimeBasis = Time.time;
			}
			break;
		case 2: // Zooming
			Vector2[] newPositions = new Vector2[]{Input.GetTouch(0).position, Input.GetTouch(1).position};
			if (!wasZoomingLastFrame) {
				lastZoomPositions = newPositions;
				wasZoomingLastFrame = true;
			} else {
				// Zoom based on the distance between the new positions compared to the distance between the previous positions.
				float newDistance = Vector2.Distance(newPositions[0], newPositions[1]);
				float oldDistance = Vector2.Distance(lastZoomPositions[0], lastZoomPositions[1]);
				float offset = newDistance - oldDistance;
				ZoomCamera(offset, ZoomSpeedTouch);
				lastZoomPositions = newPositions;
			}
			break;
		default: 
			wasZoomingLastFrame = false;
			break;
		}

	}

	void HandleMouse() {
		if (inputCount < 2) {
			float scroll = Input.GetAxis ("Mouse ScrollWheel");
			ZoomCamera (scroll, ZoomSpeedMouse);
		}
		if (Input.GetMouseButtonDown (0)) {
			if (doubleClickTimeBasis == 0) {
				// Default state. No clicks recorded.
				lastPanPosition = Input.mousePosition;
			} else {
				// Last known state was singleClick - and another click came is recorded. Test:
				if ((Time.time - doubleClickTimeBasis) > doubleClickThreshold) {
					lastPanPosition = Input.mousePosition;
					singleClick = true;
				} else {
					sceneController.ToggleCameraAngle ();
					singleClick = false;
				}
			} 
			doubleClickTimeBasis = Time.time;
		} else if (Input.GetMouseButton (0)) {
			PanCamera (Input.mousePosition);
		} 
	}

	void PanCamera(Vector3 newPanPosition) {
		// Determine how much to move the camera
		Vector3 offset = cam.ScreenToViewportPoint(lastPanPosition - newPanPosition);
		Vector3 move = new Vector3(offset.x * PanSpeed, 0, offset.y * PanSpeed);

		// Perform the movement
		transform.Translate(move, Space.World);  

		// Ensure the camera remains within bounds. (maybe not...double tap/click fixes all)
		Vector3 pos = transform.position;
		//pos.x = Mathf.Clamp(transform.position.x, positionBoundsX[0], positionBoundsX[1]);
		//pos.z = Mathf.Clamp(transform.position.z, positionBoundsZ[0], positionBoundsZ[1]);
		//pos.y = Mathf.Clamp (transform.position.y, positionBoundsY [0], positionBoundsY [1]);
		transform.position = pos;

		// Cache the position
		lastPanPosition = newPanPosition;
	}

	void ZoomCamera(float offset, float speed) {
		if (offset == 0) {
			return;
		}
		cam.fieldOfView = Mathf.Clamp(cam.fieldOfView - (offset * speed), ZoomBounds[0], ZoomBounds[1]);
	}

	public void SceneChanged(){
		cam.fieldOfView = defaultFOV;
	}
}