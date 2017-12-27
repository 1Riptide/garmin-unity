using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuttingChart : MonoBehaviour, IGarmin3DChart {

	public static Stack<GameObject> dataPoints = new Stack<GameObject>();
	private bool isInitialized = false;
	public GameObject puttsOverlay;
	// only used for testing.
	public GameObject mockUIOverlay;
	GameObject animatedBackground;
	Animator anim;
	public bool isEnabled {get; set;}

	void Awake () {
		// Attn web team. You might wanna hide this too!
		#if UNITY_ANDROID
		if(mockUIOverlay != null){
			mockUIOverlay.SetActive(false);
		}
		#endif
	}

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
