using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace OikosTools {
	public class MusicalDialogCustomWindow : EditorWindow {

		Vector2 _instrumentScroll = Vector2.zero;

		SerializedObject serializedObject;

		void OnEnable() {
			serializedObject = null;
		}

		void OnDisable() {
			serializedObject = null;
		}

		void OnGUI() {
			if (Selection.objects.Length == 0 || Selection.activeGameObject == null)
				return;

			var d = Selection.activeGameObject.GetComponent<MusicalDialog>();
			if (d != null) {
				if (serializedObject == null || serializedObject.targetObject != d)
					serializedObject = new SerializedObject(d);

				if (d.instrumentMode == MusicalDialog.InstrumentMode.Custom) {
					EditorGUILayout.HelpBox("CUSTOM Mode: Each syllable will play their own sound. If a syllable's clip is empty, the default clip wil be played.", MessageType.Info);
					
					d.baseClip = (AudioClip)EditorGUILayout.ObjectField("Default clip", d.baseClip, typeof(AudioClip), false);

					EditorGUILayout.Space();

					EditorUtils.DrawArray(serializedObject, "randomClips");

					EditorGUILayout.Space();
					
					//GUILayout.BeginArea(GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(EditorGUIUtility.singleLineHeight) * d.usableSyllables));
					//_instrumentScroll = EditorGUILayout.BeginScrollView(_instrumentScroll, new GUILayoutOption[]{GUILayout.MinHeight(EditorGUIUtility.singleLineHeight), GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight * 10)});
					//bool usingRandomClips = false;
					_instrumentScroll = EditorGUILayout.BeginScrollView(_instrumentScroll);
					int i = 0;
					foreach(List<string> syllables in d._storedSyllables) {
						foreach(string syllable in syllables) {
							
							var s = d.syllables[i];
							
							// set the defaults for dialogs with new fields
							s.SetNewDefaults();

							GUILayout.BeginHorizontal();
							
							EditorGUILayout.LabelField(syllable.Trim('\n').Trim('_').Trim(' '), GUILayout.Width(60));

							GUI.enabled = !s.useRandomClip;
							//s.clip = (AudioClip)EditorGUI.ObjectField(GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, new GUILayoutOption[]{GUILayout.Height(EditorGUIUtility.singleLineHeight), GUILayout.Width(100)}), s.clip, typeof(AudioClip));
							s.clip = (AudioClip)EditorGUILayout.ObjectField(s.clip, typeof(AudioClip), false);
							GUI.enabled = true;
							s.localVolume = GUILayout.HorizontalSlider(s.localVolume, 0.0f, 1.0f, GUILayout.Width(60));
							s.useRandomClip = EditorGUILayout.ToggleLeft("Random",s.useRandomClip, GUILayout.Width(70));
							//if (s.useRandomClip) usingRandomClips = true;

							s.activateTrigger = (Trigger)EditorGUILayout.ObjectField(s.activateTrigger, typeof(Trigger), true);
							GUILayout.EndHorizontal();

							i++;
						}
					}
					EditorGUILayout.EndScrollView();
						//GUILayout.EndArea();
					
					
				} else {
					EditorGUILayout.HelpBox("Set the Musical Dialog's instruments to CUSTOM to edit them here", MessageType.Warning);
				}
			} else {
				EditorGUILayout.HelpBox("Select an object with a MusicalDialog component", MessageType.Warning);
			}
		}
	}
}
