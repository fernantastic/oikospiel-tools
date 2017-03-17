using UnityEngine;
using UnityEditor;
using System.Collections;

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
