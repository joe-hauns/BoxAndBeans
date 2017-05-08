using UnityEngine;

public class GameTime : MonoBehaviour {
	public float minDocumentationFrameLengthInSec = 0.05f;

	private float gameStart;
	private float currentDocFrameStart;

	void Awake() 
	{
		gameStart = Time.time;
		currentDocFrameStart = Time.time;
		IsNewDocumentationFrame = true;
	}

	void Update() 
	{
		if (Time.time - currentDocFrameStart > minDocumentationFrameLengthInSec) {
			IsNewDocumentationFrame = true;
			while (Time.time - currentDocFrameStart > minDocumentationFrameLengthInSec)
				currentDocFrameStart += minDocumentationFrameLengthInSec;
		} else {
			IsNewDocumentationFrame = false;
		}
			
	}

	public bool IsNewDocumentationFrame { get; private set; }

	public void Reset() { 
		gameStart = Time.time; 
		currentDocFrameStart = Time.time;
	}

	public float time { get { return Time.time - gameStart; } }
}

