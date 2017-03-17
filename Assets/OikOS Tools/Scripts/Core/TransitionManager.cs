using UnityEngine;
using System.Collections;

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
			OnLevelWasLoaded();
			OnTransitionUpdate(0);
			OnFadeOutComplete();
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
			Screen.lockCursor = false;

			StartCoroutine("StartLoad");
		}

		private IEnumerator StartLoad() {
			_asyncOp = Application.LoadLevelAsync(_nextScene);
			yield return _asyncOp;
			OnLoadingComplete();
		}

		void OnLevelWasLoaded() {
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
