using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;

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
					Screen.lockCursor = false;
				}
			}
		}

		public void FadeIn() {
			if (_visible)
				return;
			_visible = true;
			canvasObject.SetActive(_visible);

			Cursor.visible = true;
			Screen.lockCursor = false;

			AudioListener.volume = (Scene.current != null ? Scene.current.volume : 1) * 0.6f;

			//UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(firstItem);

			DOTween.Kill(group.alpha);
			DOTween.To (()=>group.alpha, x=>group.alpha=x, 1, 0.4f).OnComplete(OnFinish);
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
				OnFinish();
			} else {
				DOTween.Kill(group.alpha);
				DOTween.To (()=>group.alpha, x=>group.alpha=x, 0, 0.3f).OnComplete(OnFinish);
			}
		}

		public void OnFinish() {
			group.interactable = _visible;
			canvasObject.SetActive(_visible);
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
