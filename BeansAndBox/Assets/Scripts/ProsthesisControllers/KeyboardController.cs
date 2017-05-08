using UnityEngine;
using System.Collections;

namespace ProsthesisControllers {
	public class KeyboardController :  ProsthesisController {

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
}