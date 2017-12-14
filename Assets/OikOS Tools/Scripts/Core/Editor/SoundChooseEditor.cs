
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

namespace OikosTools {
[CustomEditor(typeof(SoundChoose))]
public class SoundChooseEditor : Editor {

	public override void OnInspectorGUI ()
	{
		
		SoundChoose t = target as SoundChoose;
		
		EditorGUI.BeginChangeCheck();
		Undo.RecordObject(t, "Change SoundChoose");
		
		if (t.clips != null) {
			if (t.clips.Length > 0 && t.transform.childCount == 0)
				EditorGUILayout.HelpBox("If you want to apply effects, use the button to create the sound objects!", MessageType.Warning);
			
			if (t.clips.Length > 0 && t.GetComponentsInChildren<AudioSource>().Length > 0)
				EditorGUILayout.HelpBox("The script will use the audio sources in the child objects. No need to add clips!", MessageType.Warning);
			
			GUI.enabled = t.clips.Length > 0;
			if (GUILayout.Button(new GUIContent("Create sources from clips", !GUI.enabled ? "Assign clips first!" : ""))) {
				t.CreateSourcesFromClips();
			}
		}
		GUI.enabled = true;

		base.OnInspectorGUI ();

		EditorGUILayout.Space();
		t.onlyInsideCollider = EditorGUILayout.ToggleLeft("Only inside the collider", t.onlyInsideCollider);
		if (t.onlyInsideCollider && t.GetComponent<Collider>() == null)
			EditorGUILayout.HelpBox("Add a collider component", MessageType.Error);
		if (t.onlyInsideCollider && t.GetComponent<Collider>() != null && !t.GetComponent<Collider>().isTrigger)
			EditorGUILayout.HelpBox("Set the collider to 'Is Trigger'", MessageType.Error);
		
		
		if (EditorGUI.EndChangeCheck()) {
			EditorUtility.SetDirty(t);
		}
	}
	
}
}