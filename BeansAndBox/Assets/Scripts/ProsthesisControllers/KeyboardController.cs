using UnityEngine;
using System.Collections;

public class KeyboardController :  ProsthesisController {

	public override string controllerName { get { return "Keyboard Controller"; } }
	public override string defaultConfigFile { get{ return ""; } }
	public override bool needsConfiguration { get{ return false; } }
	public override ConfigResult SetConfiguration (string configPath) { return ConfigResult.OK; }

	private Vector2 curPos;
	public float velocity; // [ m / s ]

	void Start() {
		this.curPos = new Vector2 (0.5f,0.5f);
	}

	void FixedUpdate() {

		if (Input.GetKey(KeyCode.LeftArrow))
			curPos.x -= velocity * Time.deltaTime;
		else if (Input.GetKey(KeyCode.RightArrow))
			curPos.x += velocity * Time.deltaTime;

		if (Input.GetKey(KeyCode.DownArrow))
			curPos.y -= velocity * Time.deltaTime;
		else if (Input.GetKey(KeyCode.UpArrow))
			curPos.y += velocity * Time.deltaTime;

	}

	override public float getOpeningVelocity() {
		if (Input.GetKey(KeyCode.W))
			return 1;
		else if (Input.GetKey(KeyCode.S)) 
			return -1;
		else
			return 0;
	}

	override public Vector2 getPosition() {
		return curPos;
	}

	override public float getRotationVelocity() {
		if (Input.GetKey(KeyCode.D))
			return -1;
		else if (Input.GetKey(KeyCode.A)) 
			return 1;
		else
			return 0;
	}
}
