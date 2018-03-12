using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuttingChart : MonoBehaviour, IGarmin3DChart
{

	public static Stack<GameObject> dataPoints = new Stack<GameObject> ();
	private bool isInitialized = false;
	public GameObject puttsOverlay;
	// only used for testing.
	public GameObject mockUIOverlay;
	GameObject animatedBackground;
	GameObject animatedBall;
	Vector3 animatedBallDefaultVector;
	GameObject animatedBallShotTarget;
	Vector3 animatedBallShotDirection;
	Animator anim;
	Coroutine puttsOverlayEnumerator;
	Coroutine backgroundTransitionEnumerator;

	public bool isFocused { get; set; }

	void Awake ()
	{
		animatedBall = GameObject.Find ("Ball");
		GameObject ballTarget = GameObject.Find ("BallTarget");

		if (animatedBall != null) {
			Vector3 defaultBallPosition = animatedBall.transform.position;
			animatedBallDefaultVector = defaultBallPosition;
			animatedBall.SetActive (false);
			if (ballTarget != null) {
				animatedBallShotDirection = ballTarget.transform.position - animatedBall.transform.position;
				animatedBallShotDirection = animatedBallShotDirection.normalized;
			}
		}
	}

	void Start ()
	{
		puttsOverlay.SetActive (isInitialized);
		animatedBackground = GameObject.Find ("Background");
		if (animatedBackground != null) {
			anim = animatedBackground.GetComponent<Animator> ();
		}
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (isFocused && !isInitialized) {
			backgroundTransitionEnumerator = StartCoroutine (playBackgroundTransition ());
		} else if (!isFocused && isInitialized) {
			if (puttsOverlayEnumerator != null) {
				StopCoroutine (puttsOverlayEnumerator);
			}
			if (backgroundTransitionEnumerator != null) {
				StopCoroutine (backgroundTransitionEnumerator);
			}
			reverseBackgroundTransition ();
		}
	}

	IEnumerator playBackgroundTransition ()
	{
		Debug.Log ("playBackgroundTransition()");
		isInitialized = true;
		anim.Play ("PlayAnimation");
		yield return new WaitForSeconds (.9f);
		if (isFocused) {
			puttsOverlayEnumerator = StartCoroutine (TogglePuttsOverlay ());
			if (animatedBall != null) {
				animatedBall.SetActive (true);
				Rigidbody ballBody = animatedBall.transform.GetComponent<Rigidbody> ();
				if (ballBody.velocity == Vector3.zero) {
					animatedBall.transform.position = animatedBallDefaultVector;
					animatedBall.transform.GetComponent<Rigidbody> ().AddForce (animatedBallShotDirection * 499f, ForceMode.Acceleration);
				}
			}
		} 
	}

	public void reverseBackgroundTransition ()
	{
		Debug.Log ("reverseBackgroundTransition()");
		isInitialized = false;
		puttsOverlay.SetActive (false);

		anim.Play ("ReverseAnimation");
		if (animatedBall != null) {
			animatedBall.transform.position.Equals (animatedBallDefaultVector);
			Rigidbody ballBody = animatedBall.transform.GetComponent<Rigidbody> ();
			ballBody.velocity = Vector3.zero;
			ballBody.angularVelocity = Vector3.zero;
			animatedBall.SetActive (false);
		}
	}

	IEnumerator TogglePuttsOverlay ()
	{
		Debug.Log ("TogglePuttsOverlay on ? " + isInitialized);
		puttsOverlay.SetActive (isInitialized);
		yield return null;
	}

	public void MockInitialize ()
	{
		// This must be called by external platform. Pass JSON.
		// Initialize(getMockJSON());
	}

	public void Initialize (String json)
	{

	}
}
