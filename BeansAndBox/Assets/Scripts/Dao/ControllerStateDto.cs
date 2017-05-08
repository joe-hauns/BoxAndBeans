using UnityEngine;
using System;

[Serializable]
public class ControllerStateDto
{
	public float x;
	public float y;
	public float rotation;
	public float opened;
	public float openingVelocity;
	public float rotationVelocity;
	public float time;

	public ControllerStateDto (float x, float y, float rotation, float opened, float openingVelocity, float rotationVelocity, float time)
	{
		this.x = x;
		this.y = y;
		this.rotation = rotation;
		this.opened = opened;
		this.openingVelocity = openingVelocity;
		this.rotationVelocity = rotationVelocity;
		this.time = time;
	}
}
