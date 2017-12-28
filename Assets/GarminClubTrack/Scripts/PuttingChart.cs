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
			reverseBackgroundTransition ();
		}
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (isFocused && !isInitialized) {
			StartCoroutine (playBackgroundTransition ());
		} else if (!isFocused && isInitialized) {
			reverseBackgroundTransition ();
		}
	}

	IEnumerator playBackgroundTransition ()
	{
		Debug.Log ("playBackgroundTransition()");
		isInitialized = true;
		anim.Play ("PlayAnimation");
		yield return new WaitForSeconds (1.9f);
		StartCoroutine (TogglePuttsOverlay ());
		if (animatedBall != null) {
			animatedBall.SetActive (true);
			animatedBall.transform.position = animatedBallDefaultVector;
			animatedBall.transform.GetComponent<Rigidbody> ().AddForce (animatedBallShotDirection * 499f, ForceMode.Acceleration);
		}
	}

	public void reverseBackgroundTransition ()
	{
		Debug.Log ("reverseBackgroundTransition()");
		isInitialized = false;
		StartCoroutine (TogglePuttsOverlay ());
		anim.Play ("ReverseAnimation");
		if (animatedBall != null) {
			animatedBall.transform.position.Equals (animatedBallDefaultVector);
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
		//Initialize(getMockJSON());
	}

	public void Initialize (String json)
	{

	}
}
