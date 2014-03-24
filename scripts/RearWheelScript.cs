using UnityEngine;
using System.Collections;

public class RearWheelScript : MonoBehaviour {

	// total mass of the bike
	const float systemMass = 70f + 6f;

	public float ReactionForce {get; private set;}

	private float prevFlywheelSpeed = 0f;
	public float FlywheelSpeed {get; set;}
	
	// Update is called once per frame

	void FixedUpdate () {
		float deltaTime = Time.fixedDeltaTime;
		var wheel = GetComponent<WheelCollider>();

		// find out how much the speed of the flywheel changed since last cycle
		Vector3 curVirtualVelocity = wheel.attachedRigidbody.velocity;
		Vector3 forwardNormal = wheel.attachedRigidbody.transform.right;
		float virtualForwardSpeed = Vector3.Dot(curVirtualVelocity, forwardNormal);

		// that decerleration dictates the backforce to the rider
		float deceleration = (virtualForwardSpeed - prevFlywheelSpeed) / deltaTime;
		ReactionForce = deceleration * systemMass;
		prevFlywheelSpeed = FlywheelSpeed;

		// replace the game's forward velocity with the flywheel speed
		curVirtualVelocity -= virtualForwardSpeed * forwardNormal;
		curVirtualVelocity += FlywheelSpeed * wheel.attachedRigidbody.transform.right;
		wheel.attachedRigidbody.velocity = curVirtualVelocity;

		// update the animation
		Transform modelTransform = transform.FindChild("RearWheelModel");
		modelTransform.RotateAround(
			modelTransform.position,
			transform.right,
			FlywheelSpeed * deltaTime * 360.0f / (0.7f * Mathf.PI));

	}
	
}
