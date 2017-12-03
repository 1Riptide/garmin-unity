using UnityEngine;

public class CameraTouchControl : MonoBehaviour
{
	public GameObject target;
	public Transform cameraRotationDefault;
	public Transform cameraRotationLookingDown;
	public float cameraTransitionSpeed;

	private Camera cam;
	private Vector3 lastPanPosition;
	private int panFingerId; // Touch mode only
	private bool wasZoomingLastFrame; // Touch mode only
	private Vector2[] lastZoomPositions; // Touch mode only
	private static readonly float PanSpeed = 20f;
	private static readonly float ZoomSpeedTouch = 0.1f;
	private static readonly float ZoomSpeedMouse = 0.5f;
	private static readonly float[] positionBoundsX = new float[]{-8f, 8f};
	private static readonly float[] positionBoundsZ = new float[]{-36f, -12f};
	private static readonly float[] positionBoundsY = new float[]{28f, 28f};
	private static readonly float[] ZoomBounds = new float[]{10f, 80f};

	// Used to determine if we are looking straight down at the chart, or from an angle.
	bool isCameraToggledDown = false;
	// For counting taps OR clicks.
	int inputCount = 0; 
	// Camera position when looking down at chart.
	Vector3 cameraTopPosition = new Vector3(0,28,0);
	// Obtained at runtime.
	Vector3 cameraDefaultPosition; 
	// Speed of camera transition
	float cameraDoubleTapTransitionSpeed =1.5f;
	// Time to compare DeltaTime with.
	float elapsedTime = 0;
	// A raycasting point used for camera targeting.
	Vector3 lookAtMe;
	bool isPannable = false;

	bool singleClick;
	float doubleClickTimeBasis = 0;
	float doubleClickThreshold = .35f;

	void Awake() {
		cam = GetComponent<Camera>();
		cameraDefaultPosition = cam.transform.position;
	}

	void Update() {		
		Debug.Log ("Update() - singleClick ?  " + singleClick + " pannable? " + isPannable + " isCameraToggledDown ? " + isCameraToggledDown);
		if (Input.touchSupported && Application.platform != RuntimePlatform.WebGLPlayer) {
			HandleTouch();
		} else {
			HandleMouse();
		}
		if (!isCameraToggledDown) {
			//Debug.Log ("Already pointing down. Go back");
			if (!singleClick) {
				Camera.main.transform.rotation = Quaternion.Lerp(Camera.main.transform.rotation, cameraRotationDefault.rotation,  cameraDoubleTapTransitionSpeed * Time.deltaTime);
				Camera.main.transform.position = Vector3.Lerp (Camera.main.transform.position, cameraDefaultPosition, cameraDoubleTapTransitionSpeed * Time.deltaTime);
			} 
		} else {
			//Debug.Log ("Lerp to Top");
			if (!singleClick) {
				Camera.main.transform.rotation = Quaternion.Lerp (Camera.main.transform.rotation, cameraRotationLookingDown.rotation, cameraDoubleTapTransitionSpeed * Time.deltaTime);
				Camera.main.transform.position = Vector3.Lerp (Camera.main.transform.position, cameraTopPosition, cameraDoubleTapTransitionSpeed * Time.deltaTime);
			}
		}
		/*
		RaycastHit hit;
		if (Physics.Raycast(transform.position, Vector3.forward, out hit)){
			lookAtMe = hit.point;
			Camera.main.transform.LookAt(lookAtMe);
		}
		*/
	}

	void HandleTouch() {
		switch(Input.touchCount) {
		case 1: 
			// If the touch began, capture its position and its finger ID.
			// Otherwise, if the finger ID of the touch doesn't match, skip it.
			Touch touch = Input.GetTouch (0);

			// Panning
			wasZoomingLastFrame = false;
			if (touch.phase == TouchPhase.Began) {
				Debug.Log(elapsedTime + " inputCount " + inputCount);
				//lastTimeInputRecieved = Time.deltaTime;
				inputCount++;
				if (inputCount >= 2) {
					//Reset
					inputCount = 0;
					isPannable = false;
				} else {
					isPannable = true;
					lastPanPosition = touch.position;
					panFingerId = touch.fingerId;
				}
			} else if (touch.fingerId == panFingerId && touch.phase == TouchPhase.Moved) {
				PanCamera (touch.position);
			}
			break;

		case 2: // Zooming
			Vector2[] newPositions = new Vector2[]{Input.GetTouch(0).position, Input.GetTouch(1).position};
			if (!wasZoomingLastFrame) {
				lastZoomPositions = newPositions;
				wasZoomingLastFrame = true;
			} else {
				// Zoom based on the distance between the new positions compared to the 
				// distance between the previous positions.
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
				print ("HandleMouse() NO double click time basis established"); 
				// Default state. No clicks recorded.
				lastPanPosition = Input.mousePosition;
			} else {
				// Last known state was singleClick - and another click came is recorded. Test:
				if ((Time.time - doubleClickTimeBasis) > doubleClickThreshold) {
					print ("HandleMouse() THIS IS A SECOND CLICK ....BUT You took too long. Treat as single click or pan.");
					lastPanPosition = Input.mousePosition;
					singleClick = true;
				} else {
					print ("HandleMouse() THIS IS A DOUBLE CLICK");
					ToggleCameraAngle ();
					singleClick = false;
				}
			} 
			doubleClickTimeBasis = Time.time;
		} else if (Input.GetMouseButton (0)) {
			PanCamera (Input.mousePosition);
		} 
	}

	void PanCamera(Vector3 newPanPosition) {
		//singleClick = true;
		// Determine how much to move the camera
		Vector3 offset = cam.ScreenToViewportPoint(lastPanPosition - newPanPosition);
		Vector3 move = new Vector3(offset.x * PanSpeed, 0, offset.y * PanSpeed);

		// Perform the movement
		transform.Translate(move, Space.World);  

		// Ensure the camera remains within bounds.
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

	void ToggleCameraAngle(){
		isCameraToggledDown = !isCameraToggledDown;
		print ("ToggleCameraAngle : is camera toggled down ? " + isCameraToggledDown);
	}
}