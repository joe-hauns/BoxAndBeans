using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pose = Thalmic.Myo.Pose;

public class ProsthesisMovementController : MonoBehaviour {

	public ControllerLoader loader;
	public BoxCollider2D playground;
	public float maxRotationVelocity; // in degrees per second
	public float minOpeningDuration;  // in seconds

	private float zOffset; // stays always the same
	private float currentAngle;
	private Rigidbody rigid;
	private Animator animator;
	private GameTime gameTime;
	public ProsthesisController controller;

	void Awake () {
		this.rigid = GetComponent<Rigidbody> ();
		this.animator = GetComponent<Animator> ();
		this.gameTime = FindObjectOfType<GameTime> ();
	}

	// Use this for initialization
	void Start () {
		this.zOffset = this.gameObject.transform.position.z;
		this.currentAngle = 0;
	}

	// Update is called once per frame
	void FixedUpdate () {
		Move();
		Rotate();
		Open();
		FixConstraints ();
	}

	void FixConstraints () {
		/* rotation constraints are fixed manually due to bugs in the physics engine */
		var r = this.rigid.rotation.eulerAngles;
		this.rigid.rotation = Quaternion.Euler (new Vector3 (0, 0, r.z));
	}

	private float openingCoefficient { get { return animator.GetCurrentAnimatorStateInfo (0).normalizedTime; }}

	void Open(){
		float openVelocity = controller.getOpeningVelocity () / minOpeningDuration;

		/* stopping animation if finished */
		float opened = openingCoefficient;
		if (opened >= 1 && openVelocity > 0)
			openVelocity = 0;
		if (opened <= 0 && openVelocity < 0)
			openVelocity = 0;

		animator.SetFloat ("openVelocity", openVelocity);
	}

	void Move() {	
		var ctrlPos = toWorldPosition (controller.getPosition ());
		var targetPos = new Vector3 (ctrlPos.x, ctrlPos.y, zOffset);
		var currentPos = rigid.transform.position;

		Debug.DrawLine (currentPos, targetPos, Color.cyan);

		Vector3 targetVelocity = (targetPos - currentPos) / Time.deltaTime;

	
		rigid.velocity = targetVelocity.normalized * targetVelocity.magnitude;
	}

	float MinCircleDistance(float degFrom, float degTo) {
		if (degFrom <= degTo) {
			var cw = degTo - degFrom;
			var ccw = cw - 360;

			if (Mathf.Abs (cw) < Mathf.Abs (ccw))
				return cw;
			else
				return ccw;
		} else {
			return -MinCircleDistance (degTo, degFrom);
		}
	}

	void Rotate() {	/* rotating */
		float targetAngularVelocity = -controller.getRotationVelocity () * maxRotationVelocity;
		currentAngle = (currentAngle + targetAngularVelocity * Time.deltaTime + 360) % 360;

		var euler = rigid.rotation.eulerAngles;

		var angVelo = new Vector3 (MinCircleDistance(euler.x, 0), MinCircleDistance(euler.y, 0), MinCircleDistance(euler.z, currentAngle)) / Time.deltaTime; // #4; best
		rigid.angularVelocity =  Mathf.Deg2Rad * angVelo;
	}

	Vector2 toWorldPosition(Vector2 ctrlPosition) {
		var cropped = Vector2.Min(Vector2.Max(ctrlPosition, Vector2.zero), Vector2.one);
		var min = playground.bounds.min;
		var max = playground.bounds.max;
		return new Vector2 (
			cropped.x * (max.x - min.x) + min.x,
			cropped.y * (max.y - min.y) + min.y
		);
	}

	public ControllerStateDto State() {
		var position = this.transform.position;
		return new ControllerStateDto (
			x: position.x,
			y: position.y,
			rotation: currentAngle,
			opened: openingCoefficient,
			openingVelocity: controller.getOpeningVelocity(),
			rotationVelocity: controller.getRotationVelocity(),
			time: gameTime.time
		);
	}
}

