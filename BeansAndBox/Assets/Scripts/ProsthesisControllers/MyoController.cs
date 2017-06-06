using UnityEngine;
using System.Collections;

public abstract class MyoController : ProsthesisController
{

	protected ThalmicMyo myo;
	[Range (0, 90)] public float maxHorizontalAngle = 30f;
	[Range (0, 90)] public float maxVerticalAngle = 25f;

	private Vector3 calibrationAngles;

	void Awake() {
		this.myo = FindObjectOfType<ThalmicMyo> ();
		_Awake ();
	}

	void OnEnable() {
		var hub = FindObjectOfType<ThalmicHub> ();
		if (!hub.hubInitialized) {
			Debug.Log ("attempting to reset hub");
			hub.ResetHub ();
		}
		_OnEnable ();
	}

	void OnDisable() {
		_OnDisable ();
	}

	protected virtual void _OnEnable () {}
	protected virtual void _OnDisable () {}

	// Update is called once per frame
	void Update ()
	{
		_Update ();
		if (Input.GetKeyDown (KeyCode.Space)) {
			Debug.Log ("calibrating");
			calibrationAngles = myo.transform.localEulerAngles; //+ new Vector3(0.5f * (maxAngleUp + maxAngleDown) - maxAngleDown, 0,0);
		}
	}

	protected abstract void _Update ();
	protected abstract void _Awake ();

	override public Vector2 getPosition ()
	{
		var rotation = myo.transform.localRotation.eulerAngles - calibrationAngles;
		rotation.x = cropAngleRange (rotation.x);
		rotation.y = cropAngleRange (rotation.y);


		var vertical = Mathf.Min (maxVerticalAngle, Mathf.Max (-maxVerticalAngle, -rotation.x));
		var horizontal = Mathf.Min (maxHorizontalAngle, Mathf.Max (-maxHorizontalAngle, rotation.y));

		var x = (horizontal + maxHorizontalAngle) / (2 * maxHorizontalAngle);
		var y = (vertical + maxVerticalAngle) / (2 * maxVerticalAngle);
		return new Vector2 (x, y);
	}

	/**
	 * @param angle any angle
	 * @return the equivalent angle within [-180; 180]
	 */
	private float cropAngleRange (float angle)
	{
		angle %= 360;
		if (angle > 180)
			return angle - 360;
		else if (angle < -180)
			return angle + 360;
		else
			return angle;
	}

}
