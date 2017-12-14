
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
using System.Collections.Generic;

namespace OikosTools {
	public class SoundManager : MonoBehaviour {

		int MAX_SIMULTANEOUS_DIALOG_SOUNDS = 15;

		List<OneShotSound> oneshots = new List<OneShotSound>();
		int simultaneousSounds = 0;

		public static SoundManager instance;
		void Awake() {
			instance = this;
		}
		// Use this for initialization
		void Start () {
		
		}
		
		// Update is called once per frame
		void Update () {
			if (CameraController.instance != null) {
				transform.position = CameraController.instance.transform.position;
			} else {
				transform.position = Vector3.zero;
			}

			foreach(OneShotSound s in oneshots) {
				s.Update();
			}
		}

		void LateUpdate() {
			for(int i = 0; i < oneshots.Count; i++) {
				if (oneshots[i].markForRemoval) {
					oneshots.RemoveAt(i);
					i--;
				}
			}
		}

		public AudioSource PlayOneShot(AudioClip clip, Vector3 pos, float volume = 1, AudioSource baseSource = null, bool reversed = false) {
			
			if(simultaneousSounds >= MAX_SIMULTANEOUS_DIALOG_SOUNDS) {
				for(int i = 0; i < oneshots.Count - MAX_SIMULTANEOUS_DIALOG_SOUNDS - 1; i++) {
					OnOneshotFinish(oneshots[i]);
				}
			}

			GameObject tempGO;
		   if (baseSource == null) {
		   		tempGO = new GameObject(); // create the temp object
		   }
		   else {
		   		baseSource.playOnAwake = false;
		   		tempGO = GameObject.Instantiate(baseSource.gameObject, Vector3.zero, Quaternion.identity) as GameObject;
		   		tempGO.SetActive(true);
		   }

		   tempGO.name = "_Temp Sound " + clip.name;
		   tempGO.transform.SetParent(null);
		   tempGO.transform.position = pos; // set its position
		   
		   AudioSource aSource = tempGO.GetComponent<AudioSource>();
		   if (aSource == null)
		   		aSource = tempGO.AddComponent<AudioSource>(); // add an audio source

		   aSource.volume *= volume;
		   aSource.clip = clip; // define the clip
		   aSource.playOnAwake = false;
		   aSource.loop = false;
		   if (reversed) {
		   	// audio clip must be on Decompress On Load
		   	aSource.timeSamples = clip.samples - 1;
 			aSource.pitch = -1;
		   }
		   aSource.Play(); // start the sound
		   float lifetime = clip.length;
		   if (baseSource != null) {
		   	lifetime = Mathf.Max(12, clip.length * 3); // ummm hack the lifetime of the clip in case there's effects like reverb
		   }
			
			OneShotSound s = new OneShotSound(tempGO, lifetime);
			oneshots.Add(s);
			simultaneousSounds++;
		   
		   return aSource; // return the AudioSource reference
			
		}

		internal void OnOneshotFinish(OneShotSound Sound) {
			if (Sound.markForRemoval)
				return;

			if (Sound.target != null)
				Destroy(Sound.target);
			Sound.markForRemoval = true;
			simultaneousSounds--;
		}
		public AudioSource PlayClipOnPlayer(AudioClip clip, float volume = 1, AudioSource baseSource = null, bool reversed = false) { 
			AudioSource a = PlayOneShot(clip, CameraController.instance.gameCamera.transform.position, volume, baseSource, reversed); 
			a.transform.SetParent(CameraController.instance.gameCamera.transform);
			return a;
		}
	}
	[System.Serializable]
	public class OneShotSound {
		public GameObject target;
		public float lifetime = -1;
		public bool markForRemoval = false;

		float life = 0;

		public OneShotSound(GameObject Target, float Lifetime = 3) {
			target = Target;
			lifetime = Lifetime;
			life = 0;
		}

		public void Update() {
			life += Time.unscaledDeltaTime;
			if (life >= lifetime) {
				Destroy();
			}
		}
		void Destroy() {
			SoundManager.instance.OnOneshotFinish(this);
		}
	}
}