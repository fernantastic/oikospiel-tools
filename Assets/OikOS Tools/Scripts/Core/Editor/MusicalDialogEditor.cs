using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using AudioSynthesis.Midi;
using System.Linq;

namespace OikosTools {
	[CustomEditor(typeof(MusicalDialog))]
	public class MusicalDialogEditor : Editor {

		string _lastText = "";

		//Vector2 _instrumentScroll = Vector2.zero;

		public override void OnInspectorGUI ()
		{
			//base.OnInspectorGUI ();
			MusicalDialog d = target as MusicalDialog;
			GUIStyle gs;

			EditorGUI.BeginChangeCheck();
			Undo.RecordObject(d, "Change Musical Editor");

			EditorGUILayout.Space();

			EditorGUILayout.BeginHorizontal();
			gs = new GUIStyle();
			//gs.alignment = TextAnchor.MiddleCenter;
			gs.fontStyle = FontStyle.Bold;
			gs.fontSize = 14;
			EditorGUILayout.LabelField("Dialog:", gs);

			EditorGUI.indentLevel = 0;

			gs = new GUIStyle();
			gs.fontSize = 9;
			gs.fontStyle = FontStyle.Italic;
			gs.alignment = TextAnchor.MiddleRight;
			EditorGUILayout.LabelField("Use spaces and _ (underscores) to separate into syllables.", gs);

			EditorGUILayout.EndHorizontal();

			bool isDialogDirty = false;
			
			EditorGUILayout.BeginHorizontal();
			GUI.enabled = d.dialogMode != MusicalDialog.DialogMode.Sentence;
			if (GUILayout.Button("SENTENCE")) { d.dialogMode = MusicalDialog.DialogMode.Sentence; isDialogDirty = true; }
			GUI.enabled = d.dialogMode != MusicalDialog.DialogMode.TextFile;
			if (GUILayout.Button("TEXT FILE")) { d.dialogMode = MusicalDialog.DialogMode.TextFile; isDialogDirty = true; }
			GUI.enabled = true;
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.Space();

			if (d.dialogMode == MusicalDialog.DialogMode.Sentence) {

				gs = new GUIStyle(GUI.skin.textArea);
				gs.fontSize = 14;
				gs.padding = new RectOffset(10,10,10,10);
				d.dialog = EditorGUILayout.TextArea(d.dialog, gs, GUILayout.ExpandHeight(false));
				if (d.usableSyllables == 0 || _lastText != d.dialog) {
					isDialogDirty = true;
					_lastText = d.dialog;
					d.gameObject.name = "Dialog (" + d.dialog.Substring(0,Mathf.Min(d.dialog.Length,6)) + "...)";
				}

			} else if (d.dialogMode == MusicalDialog.DialogMode.TextFile) {

				if (!string.IsNullOrEmpty(d._editor_textFile_path) && d.textFile == null) {
					// restore last used text file
					d.textFile = AssetDatabase.LoadAssetAtPath(d._editor_textFile_path, typeof(Object));
				}
				Object lastTextFile = d.textFile;
				d.textFile = EditorGUILayout.ObjectField("Text File", d.textFile, typeof(Object), false);
				d._editor_textFile_path = AssetDatabase.GetAssetPath(d.textFile);
				if (d.textFile != null && !string.IsNullOrEmpty(d._editor_textFile_path) && System.IO.Path.GetExtension(d._editor_textFile_path) != ".txt") {
					EditorGUILayout.HelpBox("Only .txt files work with the musical dialog", MessageType.Error);
					//d.textFile = null;
					d._editor_textFile_path = null;
				}
				if (d.textFile != null && !string.IsNullOrEmpty(d._editor_textFile_path) && !d._editor_textFile_path.Contains("Resources")) {
					EditorGUILayout.HelpBox("The text file must be inside the RESOURCES folder", MessageType.Error);
					d._editor_textFile_path = null;
					isDialogDirty = true;
				}
				if (d.textFile == null || d.textFile != null && (lastTextFile != d.textFile) || d._storedDialogs.Count == 0)
					isDialogDirty = true;

				/*
				gs = GUI.skin.textArea;
				gs.fontSize = 7;
				gs.padding = new RectOffset(10,10,10,10);
				string s = "";
				for(int i = 0; i < Mathf.Min(d._storedDialogs.Count, 2); i++) {
					s += (i > 0 ? "\n\n" : "") + d._storedDialogs[i];
				}
				s += "\n ... ";
				EditorGUILayout.LabelField(s, gs);
				*/

				if (d.textFile == null) 
					EditorGUILayout.HelpBox("Load a text file", MessageType.Warning);
				else if (d._editor_textFile_path != null)
					EditorGUILayout.HelpBox("Text file loaded, with " + d._storedDialogs.Count + " separate dialogs :) \nPreview: '" + (d._storedDialogs.Count > 0 ? d._storedDialogs[0].Substring(0, Mathf.Min(30,d._storedDialogs[0].Length)).Trim('\n') : "") + "...'", MessageType.Info);
			}

			EditorGUILayout.BeginHorizontal();
			
			if (GUILayout.Button("Reload Dialog", GUILayout.MaxWidth(120)) || isDialogDirty) 
				d.ParseDialog();

			gs = new GUIStyle();
			gs.fontSize = 12;
			gs.fontStyle = FontStyle.Bold;
			gs.alignment = TextAnchor.MiddleRight;
			EditorGUILayout.LabelField( (d.dialogMode == MusicalDialog.DialogMode.TextFile ? "Dialogs: " +  d._storedDialogs.Count + "  " : "") + "Syllables: " + d.usableSyllables + "  ", gs);

			EditorGUILayout.EndHorizontal();

			if (d.usableSyllables == 0) EditorGUILayout.HelpBox("No syllables could be loaded", MessageType.Error);

			EditorGUI.indentLevel = 0;

			EditorGUILayout.Space();

			gs = new GUIStyle();
			//gs.alignment = TextAnchor.MiddleCenter;
			gs.fontStyle = FontStyle.Bold;
			gs.fontSize = 14;
			EditorGUILayout.LabelField("Instruments:", gs);

			EditorGUI.indentLevel = 0;

			EditorGUILayout.BeginHorizontal();
			GUI.enabled = d.instrumentMode != MusicalDialog.InstrumentMode.Single;
			if (GUILayout.Button("SINGLE")) d.instrumentMode = MusicalDialog.InstrumentMode.Single;
			GUI.enabled = d.instrumentMode != MusicalDialog.InstrumentMode.Ordered;
			if (GUILayout.Button("IN ORDER")) d.instrumentMode = MusicalDialog.InstrumentMode.Ordered;
			GUI.enabled = d.instrumentMode != MusicalDialog.InstrumentMode.Random;
			if (GUILayout.Button("RANDOM")) d.instrumentMode = MusicalDialog.InstrumentMode.Random;
			GUI.enabled = d.instrumentMode != MusicalDialog.InstrumentMode.Custom;
			if (GUILayout.Button("CUSTOM")) d.instrumentMode = MusicalDialog.InstrumentMode.Custom;
			/*if (d.dialogMode == MusicalDialog.DialogMode.Sentence) {
				GUI.enabled = d.instrumentMode != MusicalDialog.InstrumentMode.Custom;
				if (GUILayout.Button("CUSTOM")) d.instrumentMode = MusicalDialog.InstrumentMode.Custom;
			} else {
				if (d.instrumentMode == MusicalDialog.InstrumentMode.Custom)
					d.instrumentMode = MusicalDialog.InstrumentMode.Single;
			}*/
			GUI.enabled = true;
			EditorGUILayout.EndHorizontal();

			if (d.instrumentMode == MusicalDialog.InstrumentMode.Single) {
				EditorGUILayout.HelpBox("SINGLE Mode: All syllables will play the same sound.", MessageType.Info);
				d.baseClip = (AudioClip)EditorGUILayout.ObjectField("Default clip", d.baseClip, typeof(AudioClip), false);
			}
			if (d.instrumentMode == MusicalDialog.InstrumentMode.Ordered) {
				EditorGUILayout.HelpBox("RANDOM Mode: All syllables will play sounds from this list in order.", MessageType.Info);

				EditorUtils.DrawArray(serializedObject, "orderedClips");
				
			}
			if (d.instrumentMode == MusicalDialog.InstrumentMode.Custom) {
				EditorGUILayout.HelpBox("CUSTOM Mode: Each syllable will play their own sound. If a syllable's clip is empty, the default clip wil be played.", MessageType.Info);

				if (GUILayout.Button("OPEN CUSTOM EDITOR")) EditorWindow.GetWindow(typeof(MusicalDialogCustomWindow));
				/*
				d.baseClip = (AudioClip)EditorGUILayout.ObjectField("Default clip", d.baseClip, typeof(AudioClip), false);
				
				//GUILayout.BeginArea(GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(EditorGUIUtility.singleLineHeight) * d.usableSyllables));
				//_instrumentScroll = EditorGUILayout.BeginScrollView(_instrumentScroll, new GUILayoutOption[]{GUILayout.MinHeight(EditorGUIUtility.singleLineHeight), GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight * 10)});
				bool usingRandomClips = false;
				_instrumentScroll = EditorGUILayout.BeginScrollView(_instrumentScroll, GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight * Mathf.Min(d.usableSyllables+1,10)));
				int i = 0;
				foreach(List<string> syllables in d._storedSyllables) {
					foreach(string syllable in syllables) {
						
						var s = d.syllables[i];

						GUILayout.BeginHorizontal();
						
						EditorGUILayout.LabelField(syllable.Trim('\n').Trim('_').Trim(' '), GUILayout.Width(60));

						GUI.enabled = !s.useRandomClip;
						//s.clip = (AudioClip)EditorGUI.ObjectField(GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, new GUILayoutOption[]{GUILayout.Height(EditorGUIUtility.singleLineHeight), GUILayout.Width(100)}), s.clip, typeof(AudioClip));
						s.clip = (AudioClip)EditorGUILayout.ObjectField(s.clip, typeof(AudioClip), false);
						GUI.enabled = true;
						s.useRandomClip = EditorGUILayout.ToggleLeft("R",s.useRandomClip, GUILayout.Width(30));
						if (s.useRandomClip) usingRandomClips = true;

						s.activateTrigger = (Trigger)EditorGUILayout.ObjectField(s.activateTrigger, typeof(Trigger), true);
						GUILayout.EndHorizontal();

						i++;
					}
				}
				EditorGUILayout.EndScrollView();
					//GUILayout.EndArea();
				EditorGUILayout.Space();
				if (usingRandomClips && d.randomClips.Length == 0) EditorGUILayout.HelpBox("Some syllables play random clips but you didn't add any!", MessageType.Error);
				DrawArray(serializedObject, "randomClips");
				*/
				
			}
			if (d.instrumentMode == MusicalDialog.InstrumentMode.Random) {
				EditorGUILayout.HelpBox("RANDOM Mode: All syllables will play random sounds from this list.", MessageType.Info);

				EditorUtils.DrawArray(serializedObject, "randomClips");
				
			}
			EditorGUI.indentLevel = 0;

			EditorGUILayout.Space();

			gs = new GUIStyle();
			//gs.alignment = TextAnchor.MiddleCenter;
			gs.fontStyle = FontStyle.Bold;
			gs.fontSize = 14;
			EditorGUILayout.LabelField("Melody:", gs);

			EditorGUI.indentLevel = 0;

			d.useMidi = EditorGUILayout.ToggleLeft("Use MIDI melody", d.useMidi);
			GUI.enabled = d.useMidi;
			if (!string.IsNullOrEmpty(d._editor_midiFile_path) && d.midiFile == null) {
				// restore last used midi file
				d.midiFile = AssetDatabase.LoadAssetAtPath(d._editor_midiFile_path, typeof(Object));
			}
			Object lastMidiFile = d.midiFile;
			d.midiFile = EditorGUILayout.ObjectField("MIDI File", d.midiFile, typeof(Object), false);
			d._editor_midiFile_path = AssetDatabase.GetAssetPath(d.midiFile);
			if (d.midiFile != null && !string.IsNullOrEmpty(d._editor_midiFile_path) && System.IO.Path.GetExtension(d._editor_midiFile_path) != ".mid") {
				EditorGUILayout.HelpBox("Only .mid files work with the musical dialog", MessageType.Error);
				//d.midiFile = null;
				d._editor_midiFile_path = null;
			}

			EditorGUILayout.BeginHorizontal();
			if (d.midiFile != null && (d.midiEvents.Count == 0 || lastMidiFile != d.midiFile) || GUILayout.Button("Reload Notes", GUILayout.MaxWidth(120)))
				d.ParseMidiFile();

			gs = new GUIStyle();
			gs.fontSize = 12;
			gs.fontStyle = FontStyle.Bold;
			gs.alignment = TextAnchor.MiddleRight;
			int chords = 0;
			foreach(MusicalDialogMidiEvent e in d.midiEvents) { if (e.notes.Count > 1) chords++; }
			EditorGUILayout.LabelField("Notes: " + d.midiEvents.Count + " Chords: " + chords, gs);

			EditorGUILayout.EndHorizontal();

			

			if (d.useMidi) {
				if (d.midiEvents.Count > 0 && d.usableSyllables > d.midiEvents.Count) EditorGUILayout.HelpBox("There are more syllables than notes so the song will loop around", MessageType.Warning);
				if (d.midiEvents.Count > 0 && d.usableSyllables < d.midiEvents.Count) EditorGUILayout.HelpBox("There aren't as many syllables as notes so the song won't play completely", MessageType.Info);
				if (d.midiEvents.Count == 0 && d.usableSyllables > 0) EditorGUILayout.HelpBox("No notes found!", MessageType.Error);
			}

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Pitch change (semitones +/-)");
			d.pitchSemitones = EditorGUILayout.IntField(d.pitchSemitones);
			EditorGUILayout.EndHorizontal();

			//d.baseNote = (NotePitch)EditorGUILayout.EnumPopup("Base Note", d.baseNote);


			GUI.enabled = true;

			EditorGUILayout.Space();

			gs = new GUIStyle();
			//gs.alignment = TextAnchor.MiddleCenter;
			gs.fontStyle = FontStyle.Bold;
			gs.fontSize = 14;
			EditorGUILayout.LabelField("Settings:", gs);

			EditorGUI.indentLevel = 0;

			EditorUtils.DrawArray(serializedObject, "styles");

			EditorGUILayout.Space();

			d.volume = EditorGUILayout.Slider("Volume", d.volume, 0, 1);
			d.audioEffects_enabled = EditorGUILayout.ToggleLeft("Copy audio effects from an Audio Source", d.audioEffects_enabled);
			if (d.audioEffects_enabled) {
				EditorGUI.indentLevel++;
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Audio effects source:", GUILayout.MinWidth(120));
				d.audioEffects_source = (AudioSource)EditorGUILayout.ObjectField(d.audioEffects_source, typeof(AudioSource), true);
				EditorGUILayout.EndHorizontal();
				
				// check if there's other stuff that'd be problematic
				if (d.audioEffects_source != null) {
					d.audioEffects_source.playOnAwake = false;
					GameObject go = d.audioEffects_source.gameObject;
					string[] validTypes = new string[] {
						"UnityEngine.Transform",
						"UnityEngine.AudioSource",
						"UnityEngine.AudioEchoFilter",
						"UnityEngine.AudioChorusFilter",
						"UnityEngine.AudioReverbFilter",
						"UnityEngine.AudioLowPassFilter",
						"UnityEngine.AudioHighPassFilter",
						"UnityEngine.AudioDistortionFilter"
					};
					foreach(Component co in go.GetComponents<Component>()) {
						if (!validTypes.Contains(co.GetType().ToString())) {
							EditorGUILayout.HelpBox("The audio effects source object has an invalid component, remove it to avoid trouble!: " + co.GetType().ToString(), MessageType.Error);
						}
					}
				}

				EditorGUI.indentLevel--;
			}

			d.autoTalk = EditorGUILayout.ToggleLeft("Talk automatically", d.autoTalk);
			EditorGUI.indentLevel = 1;
			if (d.autoTalk) {
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Autotalk pause (milliseconds)");
				d.autoTalk_delay = EditorGUILayout.FloatField(d.autoTalk_delay);
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("End-of-paragraph pause (milliseconds)");
				d.autoTalk_endofparagraph_delay = EditorGUILayout.FloatField(d.autoTalk_endofparagraph_delay);
				EditorGUILayout.EndHorizontal();
			}
			EditorGUI.indentLevel = 0;


			EditorGUILayout.Space();

			gs = new GUIStyle();
			//gs.alignment = TextAnchor.MiddleCenter;
			gs.fontStyle = FontStyle.Bold;
			gs.fontSize = 14;
			EditorGUILayout.LabelField("When the dialog ends:", gs);

			d.onEnd_action = (MusicalDialog.ActionOnEnd)EditorGUILayout.EnumPopup("Do", d.onEnd_action);

			if (d.onEnd_action == MusicalDialog.ActionOnEnd.OpenAnotherDialog) {
				d.onEnd_dialog = (MusicalDialog)EditorGUILayout.ObjectField("Dialog", d.onEnd_dialog, typeof(MusicalDialog), true);
			}
			if (d.onEnd_action == MusicalDialog.ActionOnEnd.TriggerSomething) {
				d.onEnd_trigger = (Trigger)EditorGUILayout.ObjectField("Trigger", d.onEnd_trigger, typeof(Trigger), true);
			}


			EditorGUILayout.Space();

			GUI.enabled = Application.isPlaying;
			GUI.color = Color.green;
			if (GUILayout.Button("PLAY [Shift+P]" + (!Application.isPlaying ? " (Run the game to test)" : ""), GUILayout.Height(30))) {
				d.Play();
			}
			GUI.color = Color.white;
			GUI.enabled = true;

			/*
			f.sightRadius = EditorGUILayout.FloatField("Sight distance", f.sightRadius);
			if (f.sightRadius < 0.1f)
				f.sightRadius = 0.1f;
			f.stopAtDistance = EditorGUILayout.FloatField("Stop at", f.stopAtDistance);
			if (f.stopAtDistance < 0)
				f.stopAtDistance = 0;

			f.speed = EditorGUILayout.FloatField("Speed", f.speed);

			f.animationOnSeen = EditorTools.AnimationPopup("Animation On Seen", f.GetComponentInChildren<Animation>(), f.animationOnSeen);
			f.animationOnUnseen = EditorTools.AnimationPopup("Animation On Unseen", f.GetComponentInChildren<Animation>(), f.animationOnUnseen);
			*/

			if (EditorGUI.EndChangeCheck()) {
				EditorUtility.SetDirty(d);
			}

		}

		

		void OnEnable() {
			MusicalDialog d = target as MusicalDialog;
			foreach(MusicalDialogStyle s in d.styles) { s.SetNewDefaults(); }
			d.ParseDialog();
		}

	}
}