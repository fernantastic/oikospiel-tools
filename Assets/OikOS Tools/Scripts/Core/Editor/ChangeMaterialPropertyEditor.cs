
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
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace OikosTools {
[CustomEditor(typeof(ChangeMaterialProperty))]
public class ChangeMaterialPropertyEditor : Editor {

	public override void OnInspectorGUI ()
	{

		//base.OnInspectorGUI ();

		ChangeMaterialProperty t = target as ChangeMaterialProperty;
		
		EditorGUI.BeginChangeCheck();
		Undo.RecordObject(t, "Change value");

		Shader s = t.GetComponent<Renderer>().sharedMaterial.shader;
		List<string> descriptions = new List<string>();
		List<int> indices = new List<int>();
		for (int i = 0; i < ShaderUtil.GetPropertyCount(s); i++) {
			ShaderUtil.ShaderPropertyType type = ShaderUtil.GetPropertyType(s,i);
			if (type == ShaderUtil.ShaderPropertyType.Color || type == ShaderUtil.ShaderPropertyType.Float || type == ShaderUtil.ShaderPropertyType.Range) {
				descriptions.Add(ShaderUtil.GetPropertyDescription(s,i));
				indices.Add(i);
			}
		}
		int descriptionsIndex = EditorGUILayout.Popup("Property", indices.IndexOf(t.propertyIndex), descriptions.ToArray());
		if (descriptionsIndex > 0) {
			t.propertyIndex = indices[descriptionsIndex];
			t.stored_propertyName = ShaderUtil.GetPropertyName(s,t.propertyIndex);
			switch(ShaderUtil.GetPropertyType(s,t.propertyIndex)) {
			case ShaderUtil.ShaderPropertyType.Float:
			case ShaderUtil.ShaderPropertyType.Range:
				t.stored_propertyType = typeof(float).ToString();
				break;
			case ShaderUtil.ShaderPropertyType.Color:
				t.stored_propertyType = typeof(Color).ToString();
				break;
			}
		} else {
			t.propertyIndex = 0;
		}

		if (t.propertyIndex >= 0) {
			ShaderUtil.ShaderPropertyType type = ShaderUtil.GetPropertyType(s,t.propertyIndex);
			if (type == ShaderUtil.ShaderPropertyType.Float) {
				t.value_float = EditorGUILayout.FloatField("Value", t.value_float);
			} else if (type == ShaderUtil.ShaderPropertyType.Range) {
				t.value_float = EditorGUILayout.Slider("Value", t.value_float, ShaderUtil.GetRangeLimits(s,t.propertyIndex,1), ShaderUtil.GetRangeLimits(s,t.propertyIndex,2));
			} else if (type == ShaderUtil.ShaderPropertyType.Color) {
				t.value_color = EditorGUILayout.ColorField("Color", t.value_color);
			}
		}

		if (EditorGUI.EndChangeCheck()) {
			EditorUtility.SetDirty(t);
		}

	}
}
}