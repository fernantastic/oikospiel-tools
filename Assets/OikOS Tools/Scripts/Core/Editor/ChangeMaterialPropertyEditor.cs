using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

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
