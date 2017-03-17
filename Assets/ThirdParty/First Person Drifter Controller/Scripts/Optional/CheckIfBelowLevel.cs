// by @torahhorse

// Instructions:
// Place on player. OnBelowLevel will get called if the player ever falls below

using UnityEngine;
using System.Collections;

public class CheckIfBelowLevel : MonoBehaviour
{
	public float resetBelowThisY = -100f;
	public bool fadeInOnReset = true;
	
	private Vector3 startingPosition;
	
	void Awake()
	{
		startingPosition = transform.position;
	}
	
	void Update ()
	{
		if( transform.position.y < resetBelowThisY )
		{
			OnBelowLevel();
		}
	}
	
	private void OnBelowLevel()
	{
		Debug.Log("Player fell below level");
	
		// reset the player
		transform.position = startingPosition;
		
		if( fadeInOnReset )
		{
			// see if we already have a "camera fade on start"
			CameraFadeOnStart fade = GameObject.Find("Main Camera") != null ? GameObject.Find("Main Camera").GetComponent<CameraFadeOnStart>() : null;
			if( fade != null )
			{
				fade.Fade();
			}
			else
			{
				Debug.LogWarning("CheckIfBelowLevel couldn't find a CameraFadeOnStart on the main camera");
			}
		}
		
		// alternatively, you could just reload the current scene using this line:
		//Application.LoadLevel(Application.loadedLevel);
	}
}
