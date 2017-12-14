
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
using UnityEngine.SceneManagement;

namespace OikosTools {
	[RequireComponent(typeof(Camera))]
	[RequireComponent(typeof(AudioSource))]
	[RequireComponent(typeof(WaveEffect))]
	public class TransitionManager : MonoBehaviour {

		public float waveIntensity = 20;
		public Canvas canvas;
		public UnityEngine.UI.RawImage transitionImage;
		public AnimationCurve curveTransitionImage = AnimationCurve.Linear(0,0,1,1);
		public AnimationCurve curveWaveEffect = AnimationCurve.Linear(0,0,1,1);
		public AnimationCurve curveVolume = AnimationCurve.Linear(0,0,1,1);
		public AudioClip[] transitionSounds;
		public Texture2D[] transitionTextures;
		public Texture2D[] waveTextures;


		public static float sceneVolume = 1;
		public static TransitionManager instance;

		string _nextScene = "";
		float _transitionState = 0;
		float _transitionDuration = 1;
		AsyncOperation _asyncOp;
		float _fromVolume = 1;
		float _toVolume = 1;

		void Awake() {
			instance = this;
			OnSceneLoaded();
			OnTransitionUpdate(0);
			OnFadeOutComplete();
		}
		void OnEnable() {
			SceneManager.sceneLoaded += OnSceneLoaded;
		}
		void OnDisable() {
			SceneManager.sceneLoaded -= OnSceneLoaded;

		}

		public void TransitionTo(string Scene, float Duration = 12, Texture2D TransitionTexture = null, Texture2D WaveTexture = null, AudioClip TransitionSound = null) {
			if (iTween.Count(gameObject) > 0)
				return;

			if (Game.instance.pauseMenu) Game.instance.pauseMenu.FadeOut();

			GetComponent<WaveEffect>().enabled = true;
			canvas.gameObject.SetActive(true);
			_nextScene = Scene;
			_fromVolume = AudioListener.volume;
			_toVolume = 1;

			_transitionDuration = Duration;
			AudioSource a = GetComponent<AudioSource>();
			a.clip = TransitionSound != null ? TransitionSound : transitionSounds[Random.Range(0,transitionSounds.Length-1)];
			a.Play();
			a.time = Random.value * a.time;

			GetComponent<WaveEffect>().displacement = WaveTexture != null ? WaveTexture : waveTextures[Random.Range(0,waveTextures.Length-1)];
			transitionImage.texture = TransitionTexture != null ? TransitionTexture : transitionTextures[Random.Range(0,transitionTextures.Length-1)];

			iTween.ValueTo(gameObject, iTween.Hash("from" , _transitionState, "to", 1, "time", _transitionDuration * .5f, "onupdate", "OnTransitionUpdate", "oncomplete", "OnFadeInComplete"));
		}

		void OnTransitionUpdate(float Ratio) {
			_transitionState = Ratio;
			foreach(UnityEngine.UI.Graphic g in gameObject.GetComponentsInChildren<UnityEngine.UI.Graphic>()) {
				Color c = g.color;
				c.a = curveTransitionImage.Evaluate(Ratio);
				g.color = c;
			}
			GetComponent<AudioSource>().volume = curveVolume.Evaluate(Ratio);
			GetComponent<WaveEffect>().intensity = curveWaveEffect.Evaluate(Ratio) * waveIntensity;
			AudioListener.volume = Mathf.Lerp(_fromVolume, _toVolume, Ratio);
		}

		void OnFadeInComplete() {
			Cursor.visible = true;
			Cursor.lockState = CursorLockMode.None;

			StartCoroutine("StartLoad");
		}

		private IEnumerator StartLoad() {
			_asyncOp = SceneManager.LoadSceneAsync(_nextScene);
			yield return _asyncOp;
			OnLoadingComplete();
		}

		void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode) {
			OnSceneLoaded();
		}
		void OnSceneLoaded() {
			//Debug.Log("OnLoadingComplete");
			foreach(Canvas c in GameObject.FindObjectsOfType<Canvas>() ) {
				c.renderMode = RenderMode.ScreenSpaceCamera;
				//Debug.Log("found canvas " + c);
				c.worldCamera = GetComponent<Camera>();
			}
			
			AudioListener[] al = GameObject.FindObjectsOfType<AudioListener>();
			if (al.Length > 1) {
				foreach(AudioListener a in al) {
					if (a.gameObject.name != "SoundManager") {
						Destroy (a);
					}
				}
			}
		}

		void OnLoadingComplete() {
			_fromVolume = Scene.current != null ? Scene.current.volume : 1;
			_toVolume = AudioListener.volume;
			iTween.ValueTo(gameObject, iTween.Hash("from" , _transitionState, "to", 0, "time", _transitionDuration * .5f, "delay", 0.5f, "onupdate", "OnTransitionUpdate", "oncomplete", "OnFadeOutComplete"));
		}

		void OnFadeOutComplete() {
			GetComponent<WaveEffect>().enabled = false;
			canvas.gameObject.SetActive(false);
			GetComponent<AudioSource>().Stop();
		}



	}
}
