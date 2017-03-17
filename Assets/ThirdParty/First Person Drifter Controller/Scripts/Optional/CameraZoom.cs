// by @torahhorse

using UnityEngine;
using System.Collections;

// allows player to zoom in the FOV when holding a button down
[RequireComponent (typeof (Camera))]
public class CameraZoom : MonoBehaviour
{
	public float normalFOV = 75f;
	public float zoomFOV = 30.0f;
	public float zoomSpeed = 9f;
	
	private float targetFOV;
	
	void Start ()
	{
		GetComponent<Camera>().fieldOfView = normalFOV;
	}
	
	void Update ()
	{
		if( Input.GetButton("Fire2") )
		{
			targetFOV = zoomFOV;
		}
		else
		{
			targetFOV = normalFOV;
		}
		
		UpdateZoom();
	}
	
	private void UpdateZoom()
	{
		GetComponent<Camera>().fieldOfView = Mathf.Lerp(GetComponent<Camera>().fieldOfView, targetFOV, zoomSpeed * Time.deltaTime);
	}
}
