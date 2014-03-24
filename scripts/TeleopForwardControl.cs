using UnityEngine;
using System.Collections;

public class TeleopForwardControl : MonoBehaviour {
	
	// Update is called once per frame
	void Update () {
		float joyY = Input.GetKey(KeyCode.UpArrow) ? 1.0f : (Input.GetKey(KeyCode.DownArrow) ? -1.0f : 0.0f);
		
		this.rigidbody.AddTorque(new Vector3(0, 0, 40.0f * joyY));
	}
}
