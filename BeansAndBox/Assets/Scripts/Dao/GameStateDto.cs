using System;

[Serializable]
public class GameStateDto
{
	public string participant;
	public int score;
	public BeanDto[] beans;
	public PlayGroundDto playground;
	public string worldTime;
	public ControllerStateDto[] controllerPath;

	public GameStateDto (string participant, int score, BeanDto[] beans, PlayGroundDto playground, DateTime time, ControllerStateDto[] controllerPath)
	{
		this.participant = participant;
		this.score = score;
		this.beans = beans;
		this.playground = playground;
		this.worldTime = time.ToString ("O");
		this.controllerPath = controllerPath;
	}

	public DateTime dateTime { get { return DateTime.Parse (worldTime); } }
}

[Serializable]
public class PlayGroundDto
{
	public float xMin;
	public float xMax;
	public float yMin;
	public float yMax;

	public PlayGroundDto (float xMin, float xMax, float yMin, float yMax)
	{
		this.xMin = xMin;
		this.xMax = xMax;
		this.yMin = yMin;
		this.yMax = yMax;
	}
}

[Serializable]
public class BeanDto
{
	public BeanStateDto[] states;

	public BeanDto (BeanStateDto[] states)
	{
		this.states = states;
	}
}

[Serializable]
public class BeanStateDto
{
	public float x;
	public float y;
	public float rotation;
	public float time;
	public bool wasCollected;
	public bool isGrabbed;

	public BeanStateDto (float x, float y, float rotation, float time, bool wasCollected, bool isGrabbed)
	{
		this.x = x;
		this.y = y;
		this.rotation = rotation;
		this.time = time;
		this.wasCollected = wasCollected;
		this.isGrabbed = isGrabbed;
	}
}