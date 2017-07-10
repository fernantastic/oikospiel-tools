using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace OikosTools {
	public class MenuPause : MonoBehaviour {
		public GameObject canvasObject;
		public CanvasGroup group;

		public Button button_continue;
		public Button button_restart;
		public Button button_previous;
		public Button button_exit;

		internal bool visible { get { return _visible; } }
		bool _visible = false;
		float _fadeT = -1;
		float _fadeFrom = 0;
		float _fadeTo = 0;

		// Use this for initialization
		void OnEnable () {
			Initialize();
		}

		void Initialize() {
			FadeOut(true);

			var c = canvasObject.GetComponent<Canvas>();
			c.renderMode = RenderMode.ScreenSpaceCamera;
			c.worldCamera = TransitionManager.instance.GetComponent<Camera>();
		}

		void Update() {
			if (_fadeT >= 0) {
				_fadeT += Time.deltaTime / 0.3f;
				group.alpha = Mathf.Lerp(_fadeFrom, _fadeTo, Mathf.Clamp01(_fadeT));
				if (_fadeT > 1) {
					OnFadeComplete();
				}
			}
			if (Scene.current) {
				if (Input.GetKeyDown(KeyCode.Escape)) {
					Debug.Log ("Fading pause menu in _visible="+_visible);
					if (_visible)
						FadeOut();
					else
						FadeIn();
				}
				if (_visible) {
					Cursor.visible = true;
					Cursor.lockState = CursorLockMode.None;
				}
			}
		}

		public void FadeIn() {
			if (_visible)
				return;
			_visible = true;
			canvasObject.SetActive(_visible);

			Cursor.visible = true;
			Cursor.lockState = CursorLockMode.None;

			AudioListener.volume = (Scene.current != null ? Scene.current.volume : 1) * 0.6f;

			//UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(firstItem);
			_fadeT = 0;
			_fadeFrom = group.alpha;
			_fadeTo = 1;
		}

		public void FadeOut(bool Immediate=false) {
			if (!Immediate && !_visible)
				return;
			_visible = false;

			if (Scene.current && Scene.current.inputEnabled && Player.isActive) {
				Player.instance.SwitchControlMode();
			}

			//UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);

			AudioListener.volume = Scene.current != null ? Scene.current.volume : 1;
			if (Immediate) {
				group.alpha = 0;
				OnFadeComplete();
			} else {
				_fadeT = 0;
				_fadeFrom = group.alpha;
				_fadeTo = 0;
			}
		}

		public void OnFadeComplete() {
			group.interactable = _visible;
			canvasObject.SetActive(_visible);
			_fadeT = -1;
		}

		public void OnButtonHit(Button button) {
			if (button == button_continue)
				FadeOut();
			if (button == button_restart)
				TransitionManager.instance.TransitionTo(SaveLoad.instance.data.lastScene);
			if (button == button_previous)
				TransitionManager.instance.TransitionTo(SaveLoad.instance.data.lastScene);
			if (button == button_exit)
				TransitionManager.instance.TransitionTo("_MainMenu");
		}

	}
}
