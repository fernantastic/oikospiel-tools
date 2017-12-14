
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
using System.Collections;

namespace OikosTools {
[RequireComponent(typeof(Renderer))]
public class ChangeMaterialProperty : MonoBehaviour {

	[SerializeField()]
	public int propertyIndex = 0;
	[SerializeField()]
	public string stored_propertyType;
	[SerializeField()]
	public string stored_propertyName;

	public float value_float = 0;
	public Color value_color = Color.white;

	// Update is called once per frame
	void Update () {
		if (propertyIndex >= 0) {
			Material m = GetComponent<Renderer>().material;
			if (stored_propertyType == "System.Single") {
				if (m.GetFloat(stored_propertyName) != value_float)
					m.SetFloat(stored_propertyName, value_float);
			} else if (stored_propertyType == "UnityEngine.Color") {
			//	if (m.GetColor(stored_propertyName) != value_color)
					m.SetColor(stored_propertyName, value_color);
			}
		}
	}
}
}