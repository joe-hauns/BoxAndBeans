using UnityEngine;
using System.Collections;
using Thalmic;

public class MyoDebug : MonoBehaviour {

	public ThalmicMyo myo;
	private Vector3 initFwd;
	public EchoStateController controller;

	// Use this for initialization
	void Start () {
		this.initFwd = myo.transform.forward;
	}
	
	// Update is called once per frame
	void Update () {
		var angles = myo.transform.eulerAngles;
		var xRotation = Quaternion.Euler (angles.x * Vector3.right);
		var yRotation = Quaternion.Euler (angles.y * Vector3.up);
		var zRotation = Quaternion.Euler (angles.z * Vector3.forward);
		var pos = controller.getPosition ();

		{
			var scale = 1f;
			var shift = new Vector3 (3, 0, 0);
			Debug.DrawLine (shift + scale * Vector3.zero, shift + scale * (xRotation * Vector3.forward), Color.red);
			Debug.DrawLine (shift + scale * Vector3.zero, shift + scale * (yRotation * Vector3.forward), Color.green);
			Debug.DrawLine (shift + scale * Vector3.zero, shift + scale * (zRotation * Vector3.forward), Color.blue);
		}

		{ // drawing controller position
			var scale = 2f;
			var shift = new Vector3 (-1, -1, 0);
			Debug.DrawLine (shift + scale * new Vector3 (0, 0, 0), shift + scale * new Vector3 (1, 0, 0));
			Debug.DrawLine (shift + scale * new Vector3 (1, 1, 0), shift + scale * new Vector3 (1, 0, 0));
			Debug.DrawLine (shift + scale * new Vector3 (1, 1, 0), shift + scale * new Vector3 (0, 1, 0));
			Debug.DrawLine (shift + scale * new Vector3 (0, 0, 0), shift + scale * new Vector3 (0, 1, 0));
			Debug.DrawLine (shift + scale * new Vector3 (.5f, .5f, 0), shift + scale * new Vector3 (pos.x, pos.y, 0), Color.cyan);
		}

		if (Input.GetKey (KeyCode.Space)) {
			myo.transform.forward = initFwd;
		}
	}
}
