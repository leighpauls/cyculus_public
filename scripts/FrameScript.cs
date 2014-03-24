using UnityEngine;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System;
using System.Text;
using System.Threading;

public class FrameScript : MonoBehaviour {
	// thread communication variables
	private bool mUsingEmulation;
	private float mLastMessageTime = -1f;
	private float mRealCurSpeed = 0.0f;
	private float mRealTurnAngle = 0.0f;
	private Thread mReceiveThread = null;
	private object mThreadLock = new object();

	public bool Finished {get; set;}

	// pitch control output
	private UdpClient mPitchOutput;
	private UdpClient mForceOutput;

	// flywheel emulation
	private FlywheelEmulation mFlywheelEmulation;

	// control state variables
	private float mPrevSpeed;

	private const float DRIVER_TIMEOUT_PERIOD = 1f;

	// Use this for initialization
	void Start () {
		mUsingEmulation = true;
		mReceiveThread = new Thread(new ThreadStart(getBytes));
		mReceiveThread.IsBackground = true;
		mReceiveThread.Start();

		mPitchOutput = new UdpClient();
		mForceOutput = new UdpClient();

		mFlywheelEmulation = new FlywheelEmulation();
		mPrevSpeed = 0f;
	}

	private float normalizeAngleDeg(float angle) {
		angle = angle % 360f;
		if (angle > 180f) {
			angle -= 360f;
		}
		return angle;
	}

	// Update is called once per frame
	void FixedUpdate () {
		if (Input.GetKey(KeyCode.Q)) {
			Application.Quit();
		}
		lock (mThreadLock) {
			if (!mUsingEmulation) {
				// check for a driver timeout
				if (mLastMessageTime < 0f) {
					mLastMessageTime = Time.time;
				}
				if (Time.time - mLastMessageTime > DRIVER_TIMEOUT_PERIOD) {
					mUsingEmulation = true;
					mFlywheelEmulation.Reset();
				}
			}

			RearWheelScript rearWheel = GetComponentInChildren<RearWheelScript>();
			ForkScript fork = GetComponentInChildren<ForkScript>();

			if (Finished) {
				rearWheel.FlywheelSpeed = 0f;
			} else if (mUsingEmulation) {
				// update the flywheel emulation
				mFlywheelEmulation.update(rearWheel.ReactionForce, Time.fixedDeltaTime);
				rearWheel.FlywheelSpeed = mFlywheelEmulation.FlywheelSpeedRps * 0.25f * 0.7f * Mathf.PI;

				// set the turn angle according to the keyboard
				float turnAngle = 0f;
				if (Input.GetKey(KeyCode.LeftArrow)) {
					turnAngle = -20f;
				} else if (Input.GetKey(KeyCode.RightArrow)) {
					turnAngle = 20f;
				}
				
				fork.setForkAngle(turnAngle);
			} else {
				// get the values from the driver thread
				string forceMessage = "pedalForce " + rearWheel.ReactionForce + "\n";
				mForceOutput.Send(Encoding.ASCII.GetBytes(forceMessage), forceMessage.Length, "localhost", 5679);
				Debug.Log(forceMessage);
				rearWheel.FlywheelSpeed = mRealCurSpeed;
				fork.setForkAngle(mRealTurnAngle);
			}

			// determine the in-game acceleration
			float curSpeed = rearWheel.FlywheelSpeed;
			float acceleration = (curSpeed - mPrevSpeed) / Time.fixedDeltaTime;
			mPrevSpeed = curSpeed;

			const float ACCELERATION_TILT_FACTOR = 0.5f;
			float virtualBikeAngleDeg = transform.rotation.eulerAngles.z;
			float virtualBikeAngleRad = virtualBikeAngleDeg * Mathf.PI / 180f;
			float physicalBikeAngleRad = virtualBikeAngleRad; 
				// - Mathf.Atan2(
				// 	-9.81f - acceleration * Mathf.Sin(virtualBikeAngleRad) * ACCELERATION_TILT_FACTOR,
				// 	-acceleration * Mathf.Cos(virtualBikeAngleRad) * ACCELERATION_TILT_FACTOR)
				// - Mathf.PI / 2f;
			float physicalBikeAngleDeg = normalizeAngleDeg(physicalBikeAngleRad * 180f / Mathf.PI);

			string message = "pitchDeg " + physicalBikeAngleDeg + "\n";
			mPitchOutput.Send(Encoding.ASCII.GetBytes(message), message.Length, "localhost", 5678);
//			Debug.Log(message);
		}

		balanceBike();
	}

	/// <summary>
	/// Reset the bike's orientation so that it's upright, without accedentally putting the wheels below the ground
	/// </summary>
	private void balanceBike() {
		// find the bottom of the wheels, or where they are contacting the ground
		WheelHit hit;
		WheelCollider frontWheel = transform.FindChild("ForkHolder").FindChild("Fork").FindChild("FrontWheelCollider").GetComponent<WheelCollider>();
		Vector3 frontContact;
		if (frontWheel.GetGroundHit(out hit)) {
			frontContact = hit.point;
		} else {
			frontContact = frontWheel.transform.position - frontWheel.radius * frontWheel.transform.up;
		}

		WheelCollider rearWheel = transform.FindChild("RearWheelCollider").GetComponent<WheelCollider>();
		Vector3 rearContact;
		if (rearWheel.GetGroundHit(out hit)) {
			rearContact = hit.point;
		} else {
			rearContact = rearWheel.transform.position - rearWheel.radius * rearWheel.transform.up;
		}

		// rotate the bike about the axis connecting the wheel contact points
		Vector3 contactAxis = frontContact - rearContact;

		Vector3 curFramePlaneNormal = Vector3.Cross(contactAxis, transform.up);
		Vector3 desiredFramePlaneNormal = Vector3.Cross(contactAxis, Vector3.up);
		float errorAngle;
		Vector3 errorRotationAxis;
		Quaternion.FromToRotation(curFramePlaneNormal, desiredFramePlaneNormal).ToAngleAxis(out errorAngle, out errorRotationAxis);
		transform.RotateAround(rearContact, errorRotationAxis, errorAngle);
	}

	void getBytes() {
		IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 5680);
		UdpClient client = new UdpClient(anyIP);
		while (true) {
			// Debug.Log("Trying to update");
			IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
			byte[] data = client.Receive(ref sender);

			// turn off emulation mode
			lock (mThreadLock) {
				mUsingEmulation = false;
				mLastMessageTime = -1f;
			}

			string text = Encoding.ASCII.GetString(data);
			string[] words = text.Split(' ');
			if (words[0] == "controls" && words.Length == 3) {
				lock (mThreadLock) {
					mRealTurnAngle = float.Parse(words[1]);
					mRealCurSpeed = float.Parse(words[2]);
					// Debug.Log("Speed: " + mRealCurSpeed);
				}
			} else {
				Debug.Log(String.Concat("Unreadable message: ", text));
			}
		}
	}
}
