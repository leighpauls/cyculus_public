using UnityEngine;
using System.Collections;

/// <summary>
/// Emulates the dynamics of the flywheel so you can test using only the keyboard
/// </summary>
public class FlywheelEmulation {

	public float FlywheelSpeedRps { get; private set; }

	// newton meters per (rotation per second)
	private const float VISCUS_FRICTION = 0.1f;
	// (newton meters)
	private const float USER_STALL_TORQUE = 1000f;
	// rps
	private const float USER_FREE_SPIN = 15f / (Mathf.PI * 0.7f);
	// newton meters
	private const float BRAKING_TORQUE = 200f;

	// Kg m^2 
	private const float FLYWHEEL_INERTIA = 1.7f;

	// torque multiplier
	private const float EFFICIENCY = 0.8f;

	public FlywheelEmulation () {
		Reset();
	}

	/// <summary>
	/// Update the state of the flywheel
	/// </summary>
	/// <param name="externalTorques">External torque in Nm</param>
	/// <param name="deltaTime">Time since the last update, in seconds</param>
	public void update (float externalTorques, float deltaTime) {
		float netTorque = externalTorques;
		netTorque -= FlywheelSpeedRps * VISCUS_FRICTION;

		if (Input.GetKey(KeyCode.DownArrow)) {
			netTorque -= BRAKING_TORQUE;
		} else if (Input.GetKey(KeyCode.UpArrow)) {
			// model the person like a DC motor
			netTorque += Mathf.Clamp(USER_STALL_TORQUE * (1 - FlywheelSpeedRps / USER_FREE_SPIN), 0f, USER_STALL_TORQUE);
		}
		float accelerationRpss = EFFICIENCY * (netTorque / FLYWHEEL_INERTIA) * (0.5f / Mathf.PI);
		FlywheelSpeedRps += accelerationRpss * deltaTime;
		// backdrive prevention
		if (FlywheelSpeedRps < 0f) {
			FlywheelSpeedRps = 0f;
		}
	}

	public void Reset() {
		FlywheelSpeedRps = 0f;
	}
}
