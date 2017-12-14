
// OikOS Toolkit - Visual game making tools for Unity

// Developed by Fernando Ramallo
// Copyright (C) 2017 David Kanaga

// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do so,
// subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

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