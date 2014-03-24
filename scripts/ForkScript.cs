using UnityEngine;
using System.Collections;

public class ForkScript : MonoBehaviour {

	const float MAX_TURN_ANGLE = 35f;
	
	public void setForkAngle(float newAngle) {
		newAngle = Mathf.Clamp(newAngle, -MAX_TURN_ANGLE, MAX_TURN_ANGLE);
		transform.localRotation = Quaternion.Euler(0, newAngle, 0);
	}
	
}
