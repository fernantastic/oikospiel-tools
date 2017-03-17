using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class EditorTools {

	public static string AnimationPopup(string Label, Animation Target, string AnimationValue) {
		List<string> clips = new List<string>();
		clips.Add(" ");
		if (Target != null) {
			foreach (AnimationState ast in Target) {
				clips.Add(ast.name);
			}
		}
		int clipIndex = EditorGUILayout.Popup(Label, clips.IndexOf(AnimationValue), clips.ToArray());
		if (clipIndex >=0)
			AnimationValue = clips[clipIndex];
		else
			AnimationValue = "";

		if (Target == null) {
			EditorGUILayout.HelpBox("To play an animation, add an Animation component to this object", MessageType.Info);
		}

		return AnimationValue;
	}
}
