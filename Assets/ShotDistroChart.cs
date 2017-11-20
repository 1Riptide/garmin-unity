using UnityEngine;
using System.Collections;

public class ShotDistroChart : MonoBehaviour {

	public GameObject prefab;

	// Use this for initialization
	void Start () {
		StartCoroutine (addDataPoints ());
	}

	IEnumerator addDataPoints(){
		int shotCount = 1490;
		for (int i = 0; i < shotCount; i++) {
			// x range is 8 thru -8
			// y range is 14 thru -14
			yield return new WaitForSeconds(Random.Range(0.01f, 0.04f));
			addDataPoint(new Vector3(Random.Range(-8.0f, 8.0f), 0, Random.Range(-14.0f, 14.0f)));
		}
	}

	IEnumerator addDataPoint(Vector3 location){
		//yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
		Instantiate(prefab, location, Quaternion.identity);
		return null;
	}
	// Update is called once per frame
	void Update () {
	
	}
}
