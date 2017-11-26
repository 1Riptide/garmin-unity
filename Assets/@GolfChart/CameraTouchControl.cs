using UnityEngine;

public class CameraTouchControl : MonoBehaviour
{
	public GameObject target;
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

	bool isCameraPointedDown = false;
	// For counting taps OR clicks.
	int inputCount = 0; 
	// Camera position when looking down at chart.
	Vector3 cameraTopPosition = new Vector3(0,28,0);
	// Obtained at runtime.
	Vector3 cameraDefaultPosition; 
	// Speed of camera transition
	float cameraDoubleTapTransitionSpeed = .8f;
	// Time to compare DeltaTime with.
	float elapsedTime = 0;
	float lastTimeInputRecieved = 0;

	void Awake() {
		cam = GetComponent<Camera>();
		cameraDefaultPosition = cam.transform.position;
	}

	void Start() {
		// Call this method in (n) seconds, and repeat every (n) seconds
		InvokeRepeating("ResetClickCount", .8f, .8f);
		Camera.main.transform.position = Vector3.Lerp (Camera.main.transform.position, cameraTopPosition, cameraDoubleTapTransitionSpeed * Time.deltaTime);

	}

	void ResetClickCount(){
		//Debug.Log ("Resetting inputCount and lastTimeInputRecieved " +  lastTimeInputRecieved);
		inputCount = 0;
		elapsedTime = 0;
	}

	void Update() {		
		if (Input.touchSupported && Application.platform != RuntimePlatform.WebGLPlayer) {
			HandleTouch();
		} else {
			HandleMouse();
		}

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
				lastTimeInputRecieved = Time.deltaTime;
				inputCount++;
				if (inputCount >= 2) {
					//Reset
					inputCount = 0;
					// Handle double tap
					if (isCameraPointedDown) {
						Camera.main.transform.position = Vector3.Lerp (Camera.main.transform.position, cameraDefaultPosition, cameraDoubleTapTransitionSpeed * Time.deltaTime);
					} else {
						Camera.main.transform.position = Vector3.Lerp (Camera.main.transform.position, cameraTopPosition, cameraDoubleTapTransitionSpeed * Time.deltaTime);
					}
				} else {
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
		// On mouse down, capture it's position.
		// Otherwise, if the mouse is still down, pan the camera.
		if (Input.GetMouseButtonDown (0)) {
			lastTimeInputRecieved = Time.deltaTime;
			inputCount++;
			Debug.Log(elapsedTime + " inputCount " + inputCount + " isCameraPointedDown = " + isCameraPointedDown);
			if (inputCount >= 2) {
				//Reset
				inputCount = 0;
				// Handle double tap
				if (isCameraPointedDown) {
					Debug.Log ("Already pointing down. Go back");
					Camera.main.transform.position = Vector3.Lerp (Camera.main.transform.position, cameraDefaultPosition, cameraDoubleTapTransitionSpeed * Time.deltaTime);
				} else {
					Debug.Log ("Lerp to Top");
					Camera.main.transform.position = Vector3.Lerp (Camera.main.transform.position, cameraTopPosition, cameraDoubleTapTransitionSpeed * Time.deltaTime);
				}
			} else {
				lastPanPosition = Input.mousePosition;
			}
		} else if (Input.GetMouseButton (0)) {
			PanCamera (Input.mousePosition);
		}

		if (inputCount < 2) {
			// Check for scrolling to zoom the camera
			float scroll = Input.GetAxis ("Mouse ScrollWheel");
			ZoomCamera (scroll, ZoomSpeedMouse);
		}
	}

	void PanCamera(Vector3 newPanPosition) {

		// Determine how much to move the camera
		Vector3 offset = cam.ScreenToViewportPoint(lastPanPosition - newPanPosition);
		Vector3 move = new Vector3(offset.x * PanSpeed, 0, offset.y * PanSpeed);

		// Perform the movement
		transform.Translate(move, Space.Self);  

		// Ensure the camera remains within bounds.
		Vector3 pos = transform.position;
		pos.x = Mathf.Clamp(transform.position.x, positionBoundsX[0], positionBoundsX[1]);
		pos.z = Mathf.Clamp(transform.position.z, positionBoundsZ[0], positionBoundsZ[1]);
		pos.y = Mathf.Clamp (transform.position.y, positionBoundsY [0], positionBoundsY [1]);
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

	Quaternion ClampRotationAroundXAxis(Quaternion q)
	{
		q.x /= q.w;
		q.y /= q.w;
		q.z /= q.w;
		q.w = 1.0f;

		float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan (q.x);
		angleX = Mathf.Clamp (angleX, 42, 90);
		q.x = Mathf.Tan (0.5f * Mathf.Deg2Rad * angleX);

		return q;
	}
}