using UnityEngine;
using System.Collections;

namespace ProsthesisControllers
{
	public abstract class MyoController : ProsthesisController
	{

		protected ThalmicMyo myo;
		[Range (0, 90)] public float maxHorizontalAngle = 45f;
		[Range (0, 90)] public float maxAngleUp = 55f;
		[Range (0, 90)] public float maxAngleDown = 5f;

		private Vector3 calibrationAngles = Vector3.zero;

		void Awake() {
			this.myo = FindObjectOfType<ThalmicMyo> ();
			_Awake ();
		}

		// Update is called once per frame
		void Update ()
		{
			_Update ();
			if (Input.GetKeyDown (KeyCode.Space)) {
				Debug.Log ("calibrating");
				calibrationAngles = myo.transform.localEulerAngles;
			}
		}

		protected abstract void _Update ();
		protected abstract void _Awake ();

		override public Vector2 getPosition ()
		{
			var rotation = myo.transform.localRotation.eulerAngles - calibrationAngles;
			rotation.x = cropAngleRange (rotation.x);
			rotation.y = cropAngleRange (rotation.y);


			var vertical = Mathf.Min (maxAngleUp, Mathf.Max (-maxAngleDown, -rotation.x));
			var horizontal = Mathf.Min (maxHorizontalAngle, Mathf.Max (-maxHorizontalAngle, rotation.y));

			var x = (horizontal + maxHorizontalAngle) / (2 * maxHorizontalAngle);
			var y = (vertical + maxAngleDown) / (maxAngleUp + maxAngleDown);
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
}