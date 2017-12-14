
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
/// <summary>
/// Chooses between several audio sources according to slider
/// </summary>
public class SoundChoose : MonoBehaviour {

	[SerializeField()]
	public GenericValueSystem.ValueType MainValueType = GenericValueSystem.ValueType.Unassigned;
	[SerializeField()]
	public AnimationCurve MainValueCurve = AnimationCurve.Linear(0,0,1,1);

	[HideInInspector()]
	public float multiplier = 1;

	public enum PlayMode {
		Continue,
		Reset,
		Randomize
	}

	public AudioClip[] clips;
	public bool reverseOrder;
	public PlayMode playMode = PlayMode.Reset;

	[HideInInspector()]
	public bool onlyInsideCollider = true;

	// new way
	AudioSource[] sources;
	public float volume = 1;
	bool[] _playing;
	bool _insideCollider = false;

	 void Start ()
	{
		if (clips.Length > 0 && transform.childCount == 0) {
			CreateSourcesFromClips();
		}
		if (sources == null || sources.Length == 0) {
			System.Collections.Generic.List<AudioSource> s = new System.Collections.Generic.List<AudioSource>();
			foreach(Transform tt in transform) {
				if (tt.GetComponent<AudioSource>())
					s.Add(tt.GetComponent<AudioSource>());
			}
			sources = s.ToArray();
		}
		
		_playing = new bool[sources.Length];
		int index = Mathf.FloorToInt(Mathf.Clamp(MainValueCurve.Evaluate(GenericValueSystem.instance.GetValue(MainValueType)) * multiplier,0,0.999f) * sources.Length);
		for (int i = 0; i < _playing.Length; i++) {
			_playing[i] = i == index;
			if (playMode == PlayMode.Continue && (sources[i].gameObject.activeSelf || sources[i].gameObject.activeInHierarchy))
				sources[i].Play();
		}

		if (GetComponent<Collider>() == null || !GetComponent<Collider>().isTrigger)
			onlyInsideCollider = false;

		if (MainValueType != GenericValueSystem.ValueType.Unassigned) 
			GenericValueSystem.instance.RegisterCallback(MainValueType, OnValueChange);
		
		if (Application.isEditor && (GetComponent<AudioEchoFilter>() || GetComponent<AudioLowPassFilter>() || GetComponent<AudioDistortionFilter>() || GetComponent<AudioReverbFilter>()))
			Debug.LogError("There's an effect next to a SoundChoose script. It won't work like this, you need to put the same effect inside each sound object.", this);
	}
	

	void OnValueChange (GenericValueSystem.ValueType Type, float Value) {
		if (onlyInsideCollider && !_insideCollider)
			return;
		if (sources != null && sources.Length > 0) {
			//int index = Mathf.FloorToInt(Mathf.Clamp(value_main,0,0.999f) * sources.Length);
			int index = System.Convert.ToInt32(MainValueCurve.Evaluate(Value) * multiplier * (sources.Length-1));
			if (reverseOrder) index = (sources.Length-1)-index;

			for (int i = 0; i < sources.Length; i++) {
				if (!sources[i] || !sources[i].gameObject.activeSelf || !sources[i].gameObject.activeInHierarchy)
					continue;
				bool changed = _playing[i] != (i == index);
				_playing[i] = i == index;
				//_soundManager.SetVolume(sources[i], volume * (_playing[i] ? 1 : (playMode == PlayMode.Continue ? 0 : 1)));
				sources[i].volume = volume * (_playing[i] ? 1 : (playMode == PlayMode.Continue ? 0 : 1));
				if (i == index && changed) {
					if (playMode == PlayMode.Reset) { 
						sources[i].timeSamples = 0; 
						sources[i].Play();
					}
					if (playMode == PlayMode.Randomize) { 
						sources[i].timeSamples = Mathf.FloorToInt(Random.value * sources[i].clip.samples);
						sources[i].Play();
					}
				}
			}
		}
	}

	void OnTriggerEnter() {
		_insideCollider = true;
	}
	void OnTriggerExit() {
		_insideCollider = false;
	}
	
	public void CreateSourcesFromClips() {
		foreach(Transform tt in transform) {
			if (Application.isEditor && !Application.isPlaying)
				DestroyImmediate(tt.gameObject);
			else
				Destroy (tt.gameObject);
		}
		sources = new AudioSource[clips.Length];
		for (int i = 0; i < clips.Length; i++) {
			GameObject go = new GameObject(clips[i].name);
			AudioSource a = go.AddComponent<AudioSource>();
			a.playOnAwake = false;
			a.clip = clips[i];
			a.loop = playMode == PlayMode.Continue;
			go.transform.parent = transform;
			sources[i] = a;
		}
		clips = new AudioClip[0];
		if (GetComponent<AudioSource>()) {
			if (Application.isEditor && !Application.isPlaying)
				DestroyImmediate(GetComponent<AudioSource>());
			else
				Destroy (GetComponent<AudioSource>());
		}
	}
	
	void OnDestroy()
	{
		if (MainValueType != GenericValueSystem.ValueType.Unassigned) GenericValueSystem.instance.UnRegisterCallbacks(OnValueChange);
	}


}
}