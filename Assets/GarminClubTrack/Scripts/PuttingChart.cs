using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuttingChart : MonoBehaviour, IGarmin3DChart {

	public static Stack<GameObject> dataPoints = new Stack<GameObject>();
	private bool isInitialized = false;
	public GameObject puttsOverlay;
	GameObject animatedBackground;
	Animator anim;
	public bool isEnabled {get; set;}
	// Use this for initialization
	void Start () {
		puttsOverlay.SetActive(isInitialized);
		animatedBackground = GameObject.Find ("Background");
		if (animatedBackground != null) {
			anim = animatedBackground.GetComponent<Animator> ();
			reverseBackgroundTransition ();
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (isEnabled && !isInitialized) {
			StartCoroutine(playBackgroundTransition());
		}else if(!isEnabled && isInitialized){
			reverseBackgroundTransition();
		}
	}

	IEnumerator playBackgroundTransition(){
		Debug.Log ("playBackgroundTransition()");
		isInitialized = true;
		anim.Play ("PlayAnimation");
		yield return new WaitForSeconds (2f);
		StartCoroutine(TogglePuttsOverlay ());
	}

	public void reverseBackgroundTransition(){
		Debug.Log ("reverseBackgroundTransition()");
		isInitialized = false;
		StartCoroutine(TogglePuttsOverlay ());
		anim.Play ("ReverseAnimation");
	}

	IEnumerator TogglePuttsOverlay()
	{
		Debug.Log ("TogglePuttsOverlay on ? " + isInitialized);
		puttsOverlay.SetActive(isInitialized);
		yield return null;
	}
}
