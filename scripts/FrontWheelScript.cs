using UnityEngine;
using System.Collections;

public class FrontWheelScript : MonoBehaviour {
	// Update is called once per frame
	void Update () {
		Transform modelTransform = transform.FindChild("FrontWheelModel");
		WheelCollider collider = GetComponent<WheelCollider>();

		modelTransform.RotateAround(
			modelTransform.position,
			transform.right,
			collider.rpm * 360f / 60f * Time.deltaTime);
	}
}
