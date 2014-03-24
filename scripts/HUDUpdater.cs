using UnityEngine;
using System; // necessary for TimeSpan


public class HUDUpdater : MonoBehaviour {
	public Transform uiroot;
	UILabel speedDisplay, distanceDisplay, timeDisplay, angleDisplay, calorieDisplay;
	float distance = 0;
	Vector3 lastPosition;
	float lastVelocity;
	float totalCalories;

	// Use this for initialization
	void Start () {
		uiroot = uiroot.Find("Camera").GetComponent<Transform>()
			.Find("Anchor").GetComponent<Transform>()
			.Find("Panel").GetComponent<Transform>();
		speedDisplay = uiroot.Find("SpeedDisplay").GetComponent<UILabel>();
		distanceDisplay = uiroot.Find("DistanceDisplay").GetComponent<UILabel>();
		timeDisplay = uiroot.Find("TimeDisplay").GetComponent<UILabel>();
		angleDisplay = uiroot.Find("AngleDisplay").GetComponent<UILabel>();
		calorieDisplay = uiroot.Find("CalorieDisplay").GetComponent<UILabel>();

		lastPosition = transform.position;
		lastVelocity = rigidbody.velocity.magnitude;
		totalCalories = 0;
	}

	// Update is called once per frame
	void Update () {
		// update speed
		speedDisplay.text = mpsToKph(rigidbody.velocity.magnitude).ToString("F1");

		// update distance travelled
		distance += Vector3.Distance(transform.position, lastPosition);
		lastPosition = transform.position;
		distanceDisplay.text = (distance/1000).ToString("F1");

		// update time
		TimeSpan ts = TimeSpan.FromSeconds(Time.time);
		if (ts.Hours == 0) {
			timeDisplay.text = string.Format("{0:00}:{1:00}", ts.Minutes, ts.Seconds);
		} else {
			timeDisplay.text = string.Format("{0:00}:{1:00}:{2:00}", ts.Hours, ts.Minutes, ts.Seconds);
		}

		// update angle
		angleDisplay.text = normalize(transform.rotation.eulerAngles.z).ToString("n1");

		// update calories
		float userMassKg = 65;
		float virtualBikeAngle = (float)normalize(transform.rotation.eulerAngles.z);
		if (virtualBikeAngle < 0) {
			virtualBikeAngle = 0;
		}
		float virtualBikeAngleRad = virtualBikeAngle * Mathf.PI / 180f;
		float accel = (rigidbody.velocity.magnitude - lastVelocity)/Time.deltaTime;
		if (accel < 0) {
			accel = 0;
		}
		float a = rigidbody.velocity.magnitude * ((float)Time.deltaTime) / 4186f;
		float b = 7.845f + 0.3872f * (float)Math.Pow(rigidbody.velocity.magnitude, 2);
		float c = 10.32f * userMassKg * ((float)Math.Tan(virtualBikeAngleRad) + 1.01f * accel/9.81f);
		float calories = a * (b + c);

		totalCalories += calories;                                         
		calorieDisplay.text = totalCalories.ToString("n1");

		lastVelocity = rigidbody.velocity.magnitude;
	}

	// convert meters per second to km per hour
	private float mpsToKph(float mps) {
		return mps * (float)3.6;
	}

	private double radToDeg(double angle) {
		return angle * 180/(2 * Math.PI);
	}

	private double degToRad(double angle) {
		return angle * (2 * Math.PI)/180;
	}

	// processes an angle to be betwee -180 and 180 degrees
	private double normalize(double val) {
		if (val > 180) {
			return val - 360;
		} else if (val < -180) {
			return val + 360;
		}
		return val;
	}
}
