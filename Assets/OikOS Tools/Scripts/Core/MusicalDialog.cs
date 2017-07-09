using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AudioSynthesis;
using AudioSynthesis.Util;
using AudioSynthesis.Midi;
using AudioSynthesis.Midi.Event;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;

namespace OikosTools {
	public class MusicalDialog : MonoBehaviour {

		public enum DialogMode { Sentence, TextFile }
		public enum InstrumentMode {
			Single,
			Custom,
			Random,
			Ordered
		}
		public enum ActionOnEnd {
			Close,
			OpenAnotherDialog,
			TriggerSomething
		}

		public string dialog = "I want to be_come a beau_ti_ful dog";
		public DialogMode dialogMode = DialogMode.Sentence;

		public Object textFile;
		public string _editor_textFile_path = "";

		public List<string> _storedDialogs = new List<string>();

		public List<List<string>> _storedSyllables = new List<List<string>>();

		public List<MusicalDialogMidiEvent> midiEvents = new List<MusicalDialogMidiEvent>();
		public List<MusicalDialogSyllable> syllables = new List<MusicalDialogSyllable>();

		public InstrumentMode instrumentMode = InstrumentMode.Single;
		public bool useMidi = true;
		
		public bool autoTalk = false;
		public float autoTalk_delay = 350f;
		public float autoTalk_endofparagraph_delay = 3000f;

		public Object midiFile;
		public string _editor_midiFile_path = ""; //store last used midi file
		public AudioClip baseClip;
		public AudioClip[] randomClips = new AudioClip[]{};
		public AudioClip[] orderedClips = new AudioClip[]{};
		public NotePitch baseNote = NotePitch.A4;
		public int pitchSemitones = 0;
		public float volume = 1;

		public bool audioEffects_enabled = false;
		public AudioSource audioEffects_source;

		public ActionOnEnd onEnd_action = ActionOnEnd.Close;
		public MusicalDialog onEnd_dialog;
		public Trigger onEnd_trigger;

		public MusicalDialogStyle[] styles;

		public int usableSyllables = 0;//int s = 0; foreach(MusicalDialogSyllable syl in syllables) { if(!string.IsNullOrEmpty(syl.text)) s++; } return s; } }

		bool cancelDialog { get { return Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Q); } }

		public static MusicalDialog current;

		
		MidiFile _midi;
		bool _playing = false;
		int _syllableIndex = 0;
		List<GameObject> _syllableObjects = new List<GameObject>();
		int _firstSyllableGlobalIndex = 0;
		Coroutine _coroutine;


		// Use this for initialization
		void Start () {

		}
		
		// Update is called once per frame
		void Update () {
			if (_playing) {



				if (Input.GetKeyDown(KeyCode.Q)) {
					StopCoroutine(_coroutine);
					Scene.current.dialogBlock.Hide();
					_playing = false;
					current = null;
					Scene.current.inputEnabled = true;
					if (Player.isActive) Player.instance.SwitchControlMode();

				}
			} else {
				#if UNITY_EDITOR
				if (Application.isEditor && Application.isPlaying && UnityEditor.Selection.activeGameObject == gameObject && (Input.GetKey(KeyCode.RightShift) || Input.GetKey(KeyCode.LeftShift)) && Input.GetKeyDown(KeyCode.P))
					Play();
					
				#endif
			}
		}

		public void Play() {
			//Debug.Log("MusicalDialog.Play()  _playing:" + _playing + " syllabls count: " + syllables.Count);
			if (_playing) return;
			if (syllables.Count == 0) return;
			_coroutine = StartCoroutine(PlayDialog());
		}

		IEnumerator Load() {
			//_midi = new MidiFile(new MidiSimpleFile(midiFile))
			yield return 0;
		}

		IEnumerator PlayDialog() {
			_playing = true;
			current = this;

			if (Scene.current.inputEnabled) {
				Scene.current.inputEnabled = false;
				Cursor.visible = true;
				Cursor.lockState = CursorLockMode.None;
			}

			ParseDialog();

			_syllableIndex = 0;
			_syllableObjects = new List<GameObject>();
			_firstSyllableGlobalIndex = 0;

			if (_storedDialogs.Count == 0)
				Debug.LogError("Tried to play a dialog but no text could be parsed");

			foreach(string currentDialog in _storedDialogs) {
				_firstSyllableGlobalIndex = _syllableIndex;

				// get lines
				//Debug.Log("Playing " + syllables.Count + " syllables with " + midiEvents.Count + " midiEvents.");
				int lines = currentDialog.Split('\n').Length;

				Scene.current.dialogBlock.Show(lines);


				var l = GetSyllables(currentDialog);
				int pauses = 0;
				int spaces = 0;
				for(int i = 0; i < l.Count; i++) {

					string currentSyllable = "";

					int previousPauses = pauses;
					int previousSpaces = spaces;

					if (previousSpaces > 0)
						currentSyllable += ' ';

					// parse text
					bool newLine = false;
					//int pauses = 1;
					pauses = 0;
					spaces = 0;
					foreach(char c in l[i]) {
						if (c==' ') {
							spaces++;
						} else if (c=='_') {
							pauses++;
						} else if (c=='\n') {
							pauses+=3;
							newLine = true;
						} else if (c=='.') {
							pauses+=2;
							currentSyllable += c;
						} else {
							currentSyllable += c;
						}
					}
					//Debug.Log("visible syllable '" + currentSyllable + "' pauses " + pauses + " spaces " + spaces);

					string regex_style = @"\[style\=([^\]]+?)\]";
					Match m = Regex.Match(currentSyllable, regex_style);
					if (m.Success && m.Groups.Count > 0) {
						//Debug.Log("match, group: " + m.Groups[1] + " value " + m.Groups[1].Value);
						string styleName = m.Groups[1].Value;
						bool switched = false;
						foreach(MusicalDialogStyle s in styles) {
							if (s.name.ToLower() == styleName.ToLower()) {
								Scene.current.dialogBlock.SwitchToStyle(s);
								switched = true;
								break;
							}
						}
						if (!switched) Debug.LogError("Could not find style '"+styleName+"'", this);

						currentSyllable = Regex.Replace(currentSyllable, regex_style, ""); // remove the tag
					}

					bool hasText = currentSyllable.Trim(' ').Length > 0;

					if (currentSyllable != " ") {
						bool wait = i > 0;
						if (!autoTalk && !hasText) wait = false;

						float t = (autoTalk_delay / 1000.0f) * (1+previousPauses+previousSpaces);

						/*
						// skip when holding
						if (skipUp && (Input.GetMouseButton(0) || Input.GetKey(KeyCode.Space))) {
							t *= 0.2f;
							skipUp = false;
						}
						*/

						if (!autoTalk)
							Scene.current.dialogBlock.waitingForInput = true;

						while (wait) {
							if (autoTalk) {
								t -= Time.deltaTime;
								if (t < 0) wait = false;
							} else if (Input.anyKeyDown || Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)) {
								wait = false;
								Scene.current.dialogBlock.waitingForInput = false;
							}
							yield return 0;
						}

						if (hasText) {
							_syllableObjects.Add(Scene.current.dialogBlock.AddLine(currentSyllable));
							
							PlaySyllable(_syllableIndex);

							if (syllables[_syllableIndex].activateTrigger != null) {
								syllables[_syllableIndex].activateTrigger.ForceTrigger();
							}

							_syllableIndex++;
						}
						
						yield return 0;
					}

					if (newLine) 
						Scene.current.dialogBlock.NextLine();


				}
				/*
				skipUp = Input.anyKey || Input.GetMouseButton(0) || Input.GetMouseButton(1);
				float tt = 0.5f;
				while (tt >=0) {
					bool pressing = Input.anyKey || Input.GetMouseButton(0) || Input.GetMouseButton(1);
					bool down = Input.anyKeyDown || Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1);
					if ((!skipUp && pressing) || down) {
						tt = -1;
					} else if (skipUp && pressing) {
						tt -= Time.deltaTime;
					}
					yield return 0;
				}
				*/

				if (autoTalk) {
					float t = (autoTalk_endofparagraph_delay / 1000.0f);
					while (t > 0) {
						t -= Time.deltaTime;
						if (Input.anyKeyDown || Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
							t = -1;
						yield return 0;
					}

				} else {
					Scene.current.dialogBlock.waitingForInput = true;

					while(!(Input.anyKeyDown || Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))) {
						yield return 0;
					}
				}
				Scene.current.dialogBlock.Clear();
				_syllableObjects.Clear();

			}
			
			Scene.current.dialogBlock.Hide();
			_playing = false;
			current = null;

			if (onEnd_action != ActionOnEnd.OpenAnotherDialog) {
				Scene.current.inputEnabled = true;
				if (Player.isActive) Player.instance.SwitchControlMode();
			}
			switch(onEnd_action) {
				case ActionOnEnd.OpenAnotherDialog:
				if (onEnd_dialog != null) onEnd_dialog.Play();
				break;
				case ActionOnEnd.TriggerSomething:
				if (onEnd_trigger != null) onEnd_trigger.ForceTrigger();
				break;

			}
		}

		public void PlaySyllable(int Index) {
			//Debug.Log("playing syllable index " + Index);	
			syllables[Index].SetNewDefaults();

			AudioClip clip = baseClip;
			if ((instrumentMode == InstrumentMode.Random || (instrumentMode == InstrumentMode.Custom && syllables[Index].useRandomClip)) && randomClips.Length > 0)
				clip = randomClips[Random.Range(0, randomClips.Length-1)];
			else if (instrumentMode == InstrumentMode.Custom && syllables[Index].clip != null)
				clip = syllables[Index].clip;
			else if (instrumentMode == InstrumentMode.Ordered && orderedClips.Count() > 0)
				clip = orderedClips[(int)Mathf.Repeat(Index, orderedClips.Count()	)];

			float syllableVolume = volume;
			if (instrumentMode == InstrumentMode.Custom)
				syllableVolume *= syllables[Index].localVolume;

			AudioSource baseAudioSource = audioEffects_enabled ? audioEffects_source : null;

			if (clip != null) {
				MusicalDialogMidiEvent e = midiEvents[(int)Mathf.PingPong(Index, midiEvents.Count-1)];

				if (useMidi && midiEvents.Count > 0) {
					foreach(int note in e.notes) {
						AudioSource ass = SoundManager.instance.PlayClipOnPlayer(clip, syllableVolume, baseAudioSource);
						int transposedNote = note - (int)baseNote;
						transposedNote += pitchSemitones;

						ass.name = "_TempAudio " + clip.name + " note " + (AudioSynthesis.Midi.NotePitch)transposedNote;
						ass.pitch = Mathf.Pow(2, (transposedNote)/12.0f);
					}
				} else {
					SoundManager.instance.PlayClipOnPlayer(clip, syllableVolume, baseAudioSource);
				}
				//Debug.Log("Playing note# " + Index + ": value " + note + /*" pitch " + Tables.SemitoneTable[note] +*/ " (source pitch: " + testAudio.pitch + ")");
			}
		}

		List<string> GetSyllables(string Dialog, bool TextOnly = false) {
			var l = new List<string>();

			Dialog += " "; // Hack to get the final syllable since the regex won't find it, TODO: fix regex so we don't need this

			MatchCollection matches = Regex.Matches(@Dialog, @"([^_ ]*?)[ _\n]+");

			//Debug.Log("found " + matches.Count + " syllables");
			foreach (Match match in matches)
			{
			    foreach (Capture capture in match.Captures)
			    {
			    	var s = capture.Value;
			    	if (TextOnly) {
			    		s = s.Trim('\n').Trim('_').Trim(' ');
			    		if (s.Length == 0)
			    			continue;
			    	}
			    	l.Add(s);
			    }
			}

			return l;
		}
		int GetTotalSyllables() {
			int n = 0;
			/*
			foreach(string s in _storedDialogs) { 
				n += GetSyllables(s).Count; 
			}
			*/
			foreach(List<string> l in _storedSyllables) {
				n += l.Count;
			}
			return n;
		}


		public void OnSyllableHover(GameObject SyllableObject) {
			int syllableIndex = _syllableObjects.IndexOf(SyllableObject);
			PlaySyllable(_firstSyllableGlobalIndex + syllableIndex);
		}

		public void ParseDialog() {
			if (dialogMode == DialogMode.Sentence) {
				_storedDialogs.Clear();
				_storedDialogs.Add(dialog);
			} else if (dialogMode == DialogMode.TextFile) {
				#if UNITY_EDITOR
				_storedDialogs.Clear();
				if (!string.IsNullOrEmpty(_editor_textFile_path)) {
					_storedDialogs.Add("");

					//Resources.Load(_editor_textFile_path);

					string[] ls = File.ReadAllLines(_editor_textFile_path);
					for(int i = 0; i < ls.Length; i++) {
						string s = ls[i];
						if (_storedDialogs.Count > 0 && !string.IsNullOrEmpty(_storedDialogs[_storedDialogs.Count-1]) && string.IsNullOrEmpty(s)) { // dialog break
							_storedDialogs.Add("");
						} else {
							_storedDialogs[_storedDialogs.Count-1] += (_storedDialogs[_storedDialogs.Count-1].Length > 0 ? "\n" : "") + s;
						}
					}
					// remove last if empty
					string lastDialog = _storedDialogs[_storedDialogs.Count-1];
					if (string.IsNullOrEmpty(lastDialog) || lastDialog.Trim(' ').Length == 0) _storedDialogs.RemoveAt(_storedDialogs.Count-1);
				}
				#endif
			}

			_storedSyllables.Clear();
			foreach(string dialog in _storedDialogs) { _storedSyllables.Add(GetSyllables(dialog, true)); }

			//([^_ ]*?)[ _\n]+

			usableSyllables = GetTotalSyllables();

			while(syllables.Count < usableSyllables) {
				syllables.Add(new MusicalDialogSyllable());
			}

			/*
			// empty dialog
			if (string.IsNullOrEmpty(resultingDialog)) {
				for(int i = 0; i < syllables.Count; i++) {
					syllables[i].text = "";
				}
				return;
			}

			List<string> texts = new List<string>();
			for (int i = 0; i < resultingDialog.Length; i++) {
				if (resultingDialog[i] == ' ' || resultingDialog[i] == '\n') {
					texts.Add(resultingDialog.Substring(n,i-n));
					n = i;
				} else if (resultingDialog[i] == '_') {
					texts.Add(resultingDialog.Substring(n,i-n)); //ignore 
					n = i+1;
				}
			}
			texts.Add(resultingDialog.Substring(n,resultingDialog.Length-n)); // add last syllable

			for(int i = 0; i < texts.Count; i++) {
				if (i >= syllables.Count)
					syllables.Add(new MusicalDialogSyllable(texts[i]));

				syllables[i].text = texts[i];;
			}
			if (syllables.Count > texts.Count) {
				for(int i = texts.Count; i < syllables.Count; i++) {
					syllables[i].text = "";
				}
			}
			*/
		}

		#if UNITY_EDITOR

		

		public void ParseMidiFile() {
			
			midiEvents.Clear();

			if (midiFile == null) return;
			if (string.IsNullOrEmpty(_editor_midiFile_path)) {
				Debug.LogError("Midi path is empty");
				return;
			}

			MidiFile mf = new MidiFile(new MidiSimpleFile(_editor_midiFile_path));
			mf.CombineTracks();

			foreach(MidiTrack t in mf.Tracks) {
				foreach(MidiEvent e in t.MidiEvents) {
					if ((MidiEventTypeEnum)e.Command == MidiEventTypeEnum.NoteOn) {
						//Debug.Log("Note dt " + e.DeltaTime + " at " + e.AbsoluteTime + ": " + e.Data1 + " (" + System.Enum.GetName(typeof(ControllerTypeEnum), e.Data1) + ") " + e.ToString());
						// check if last note uses the same time (NOTE: This assumes the notes come in order, TODO: make sure this is true)
						if (midiEvents.Count > 0 && midiEvents.Last().time == e.AbsoluteTime) {
							midiEvents.Last().notes.Add(e.Data1);
						} else {
							MusicalDialogMidiEvent musicalEvent = new MusicalDialogMidiEvent();
							musicalEvent.time = e.AbsoluteTime;
							musicalEvent.notes.Add(e.Data1);
							midiEvents.Add(musicalEvent);
						}

					}
				}
			}

		}

		#endif
	}
	[System.Serializable]
	public class MusicalDialogSyllable {

		[SerializeField()]
		int version = -1; // stored version to set defaults on new fields

		// FIELDS FOR VERSION <0
		[SerializeField()]
		public AudioClip clip;
		[SerializeField()]
		public bool useRandomClip = false;
		[SerializeField()]
		public Trigger activateTrigger;

		// FIELDS FOR VERSION 1
		[SerializeField()]
		public float localVolume = 1;
		
		// FIELDS FOR VERSION XX
		// Add new fields here, and set their defaults on SetNewDefaults() so they get properly updated on already created dialogs


		// Checks that the newly added fields have the proper default values 
		public void SetNewDefaults() {
			// upgrade to version 1 
			if (version < 1) {
				localVolume = 1;
				version = 1;
			}

			// upgrade to version XX
			// ...
			// Add defaults for new fields here, stack version checks when adding new defaults

		}

		public MusicalDialogSyllable(AudioClip Clip = null, bool UseRandomClip = false) {
			clip = Clip;
			useRandomClip = UseRandomClip;
			localVolume = 1;
		}
	}
	[System.Serializable]
	public class MusicalDialogMidiEvent {
		[SerializeField()]
		public List<int> notes = new List<int>(); // more than one note defines a chord
		[SerializeField()]
		public int time = -1;
	}

	[System.Serializable]
	public class MusicalDialogStyle {

		public enum DialogAlignment {
			Top = 1,
			Middle = 2,
			Bottom = 3
		}

		[System.Serializable]
		public class MusicalDialogTextPadding {
			public int left = 0;
			public int right = 0;
			public int top = 0;
			public int bottom = 0;
			public MusicalDialogTextPadding(int left, int right, int top, int bottom) {
				this.left = left;
				this.right = right;
				this.top = top;
				this.bottom = bottom;
			}
		}

		// FIELDS FOR VERSION 0
		[SerializeField()]
		public string name;
		[SerializeField()]
		public Font font;
		[SerializeField()]
		public Color fontColor = Color.white;
		[SerializeField()]
		public FontStyle fontStyle;
		[SerializeField()]
		public int fontSize;
		[SerializeField()]
		public float lineSpacing;
		[SerializeField()]
		public TextAlignment textAlignment;
		[SerializeField()]
		public Color backgroundColor;
		[SerializeField()]
		public UnityEngine.Sprite backgroundTexture;

		// FIELDS FOR VERSION 1
		[SerializeField()]
		public MusicalDialogStyle.DialogAlignment dialogAlignment;
		[SerializeField()]
		public float dialogMargin;

		// FIELDS FOR VERSION 2
		[SerializeField()]
		public MusicalDialogTextPadding textPadding;

		// FIELDS FOR VERSION XX
		// Add new fields here, and set their defaults on SetNewDefaults() so they get properly updated on already created dialogs


		[SerializeField(), HideInInspector]
		int version = -1; // stored version to set defaults on new fields


		public MusicalDialogStyle() {
			SetNewDefaults();
		}

		// Checks that the newly added fields have the proper default values 
		public void SetNewDefaults() {

			// upgrade to version 0
			if (version < 0) {
				//Debug.Log("setting defaults for version 0");
				name = "default";
				font = null;
				fontColor = Color.white;
				fontStyle = FontStyle.Normal;
				fontSize = 14;
				lineSpacing = 1;
				textAlignment = TextAlignment.Center;
				backgroundColor = new Color(141,0,79);
				backgroundTexture = null;

				version = 0;
			}
			// upgrade to version 1
			if (version < 1) {
				dialogAlignment = DialogAlignment.Bottom;
				dialogMargin = 10;

				version = 1;
			}
			if (version < 2) {
				textPadding = new MusicalDialogTextPadding(15,15,3,3);

				version = 2;
			}

			// upgrade to version XX
			// ...
			// Add defaults for new fields here, stack version checks when adding new defaults

		}
	}
}