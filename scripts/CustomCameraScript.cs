using UnityEngine;
using System.Collections;

public class CustomCameraScript : MonoBehaviour {
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown("r")) {
			OVRDevice.ResetOrientation(0);
		}
	}
}
