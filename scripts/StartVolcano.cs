using UnityEngine;
using System.Collections;

public class StartVolcano : MonoBehaviour {
	public Transform flame;

	void OnTriggerEnter(Collider otherObject) {
		if (otherObject.tag == "Player") {
			Debug.Log("FINISHED! Now activating volcano.");
			flame.Translate(0f, 630f, 0f);
			Transform.FindObjectOfType<FrameScript>().Finished = true;
			GameObject.Find("Finish line").renderer.enabled = false;
		}
	}

}

