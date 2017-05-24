using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class SimpleStateMachineControllerDto {
	public int electrode1;
	public int electrode2;
	public float threshold1;
	public float threshold2;
	public bool electrode1_open;
	public bool electrode1_rotate_clockwise;
}
