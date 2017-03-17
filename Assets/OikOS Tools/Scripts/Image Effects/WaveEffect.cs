using UnityEngine;

namespace OikosTools {
[RequireComponent(typeof(Camera))]
[ExecuteInEditMode()]
public class WaveEffect : ImageEffect {

	public float intensity = 0.05f;
	public Vector2 waviness = Vector2.one * 50;
	public Texture2D displacement;

	void Update() {
		material.SetFloat("_Intensity", intensity);
		material.SetFloat("_WavinessX", waviness.x);
		material.SetFloat("_WavinessY", waviness.y);
		material.SetTexture("_DispMap", displacement);
	}

	public override void OnRenderImage (RenderTexture source, RenderTexture destination)
	{
		Graphics.Blit(source, destination, material);
	}
}
}