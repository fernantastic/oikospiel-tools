using UnityEngine;
using System.Collections;

[ExecuteInEditMode()]
public class ChangeRenderSettings : MonoBehaviour {

	public Color ambientLight = Color.black;
	public bool fog = false;
	public Color fogColor = Color.white;
	public float fogDensity = 0.001f;
	public float fogStartDistance = 0;
	public float fogEndDistance = 300;
	public FogMode fogMode = FogMode.Exponential;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		RenderSettings.ambientLight = ambientLight;
		RenderSettings.fog = fog;
		RenderSettings.fogColor = fogColor;
		RenderSettings.fogDensity = fogDensity;
		RenderSettings.fogStartDistance = fogStartDistance;
		RenderSettings.fogEndDistance = fogEndDistance;
		RenderSettings.fogMode = fogMode;
	}
}
