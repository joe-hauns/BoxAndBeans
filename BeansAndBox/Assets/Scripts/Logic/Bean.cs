using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Bean : MonoBehaviour
{

	/// <summary>
	/// The drag value for the physics engine, the bean should have when it was grabbed oncs by the player 
	/// and therefore gravity is enabled for the bean.
	/// </summary>
	///<summary> The part of the bean that can be grabbed by the player. </summary>
	public Collider grabable;
	/// <summary> The collider of the left non-grabable part of the bean. </summary>
	public SphereCollider leftSphere;
	/// <summary> The collider of the right non-grabable part of the bean. </summary>
	public SphereCollider rightSphere;

	/// <summary> 
	/// An array of all possible colors, a bean may have. The bean will choose one of the colors randomly
	/// when it is spawned. 
	/// </summary>
	public Color[] colors = new Color[] {
		Color.cyan,
		Color.magenta,
		Color.yellow
	};

	public List<BeanStateDto> states{ get; private set; }

	/// <summary> The maximum length between any two points on the colliders of this bean. </summary>
	public float MaxDiameter { 
		get { 
			var localLen = (leftSphere.center.magnitude + rightSphere.center.magnitude + leftSphere.radius + rightSphere.radius);
			return transform.localToWorldMatrix.MultiplyVector (Vector3.forward * localLen).magnitude;
		} 
	}

	public Rigidbody rigid { get; private set; }

	/// <summary> Point of time when the bean was collected in seconds from the game start. </summary>
	public float timeCollected { get; private set;}

	// true iff the bean did enter the target area.
	public bool wasCollected { 
		get{ return timeCollected != -1;} 
		set {
			if (value) {
				rigid.constraints = rigid.constraints & ~(RigidbodyConstraints.FreezeRotationY);
				gameObject.layer = 0;
				rigid.useGravity = true;

				this.timeCollected = gameTime.time;
			} else {
				rigid.constraints = rigid.constraints | (RigidbodyConstraints.FreezeRotationY);
				rigid.useGravity = false;

				this.timeCollected = -1;
			}
		} }

	private GameTime gameTime;

	private int grabCount;
	public bool isGrabbed {
		set {
			if (value) {
				if (grabCount++ == 0) {
					/* stays the same after first grab */
					gameObject.layer = 0;
				}
				rigid.constraints = rigid.constraints & ~(RigidbodyConstraints.FreezeRotationY);
			} else {
				rigid.constraints = rigid.constraints | (RigidbodyConstraints.FreezeRotationY);
			}
			/* on every grab */
			grabable.isTrigger = value;
			leftSphere.isTrigger = value;
			rightSphere.isTrigger = value;
			rigid.isKinematic = value;
			rigid.interpolation = value ? RigidbodyInterpolation.None : RigidbodyInterpolation.Interpolate;
		}
		get { return grabable.isTrigger; }
	}

	public bool wasEverGrabbed { get { return grabCount != 0; }} 
	// Use this for initialization
	void Awake ()
	{
		/* finding components */
		this.rigid = GetComponent<Rigidbody> ();
		this.gameTime = FindObjectOfType<GameTime> ();

		/* initializing */
		this.states = new List<BeanStateDto> ();
		this.wasCollected = false;
		this.grabCount = 0;

		/* setting randomized color */
		var renderer = GetComponent<Renderer> ();
		var material = new Material (renderer.material.shader);
		material.CopyPropertiesFromMaterial (renderer.material);
		material.color = colors [Random.Range (0, colors.Length)];
		renderer.material = material;
	}

	private float rotation { get{ 
			var euler = this.transform.rotation.eulerAngles; 
			return euler.y == 270 ? euler.x : 180 - euler.x;
		} 
	}

	void LateUpdate() 
	{
		if (gameTime.IsNewDocumentationFrame) {
			this.states.Add(new BeanStateDto(
				x: this.transform.position.x,
				y: this.transform.position.y,
				rotation: this.rotation,
				time: gameTime.time,
				wasCollected: this.wasCollected,
				isGrabbed: this.isGrabbed
			));
		}
	}

	public void Destroy() 
	{
		UnityEngine.Object.Destroy (this.gameObject);
	}
}
