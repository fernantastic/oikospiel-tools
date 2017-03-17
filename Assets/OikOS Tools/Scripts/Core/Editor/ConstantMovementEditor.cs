using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(ConstantMovement))]
public class ConstantMovementEditor : Editor {
	
	public override void OnInspectorGUI ()
	{
		
		//base.OnInspectorGUI ();
		
		ConstantMovement t = target as ConstantMovement;
		
		EditorGUI.BeginChangeCheck();
		Undo.RecordObject(t, "Change value");
		

		t.type = (ConstantMovement.Type)EditorGUILayout.EnumPopup("Type", t.type);
		t.velocity = EditorGUILayout.Vector3Field("Velocity", t.velocity);
		t.space = (Space)EditorGUILayout.EnumPopup("According to", t.space);
		if (t.type == ConstantMovement.Type.MoveBySinewave || t.type == ConstantMovement.Type.RotateBySinewave || t.type == ConstantMovement.Type.ScaleBySinewave) {
			t.sineFrequency = EditorGUILayout.FloatField("Sinewave speed", t.sineFrequency);
		}
		
		if (EditorGUI.EndChangeCheck()) {
			EditorUtility.SetDirty(t);
		}
		
	}
}
