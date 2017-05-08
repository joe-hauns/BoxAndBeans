using System;

[Serializable]
public class ControllerMovementDto
{
	public ControllerStateDto[] path;

	public ControllerMovementDto (ControllerStateDto[] path)
	{
		this.path = path;
	}
}
