using UnityEngine;
using System.Collections;
namespace OikosTools {
	public class MenuMainMenu : MonoBehaviour {

		public CanvasGroup group;
		public string firstScene;

		public UnityEngine.UI.Button startButton;
		public UnityEngine.UI.Button restartButton;

		//bool _visible = true;
		bool _confirmRestart = false;

		// Use this for initialization
		void OnEnable () {
			if (Application.isEditor && GameObject.Find("_CORE") == null)
				Application.LoadLevelAdditive("_CORE");
		}

		void Start() {
			bool newGame = SaveLoad.instance.data.lastScene.Length == 0;
			restartButton.gameObject.SetActive(!newGame);
			startButton.GetComponentInChildren<UnityEngine.UI.Text>().text = newGame ? "Begin" : "Continue";

			Game.instance.pauseMenu.FadeOut(true);

			UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(startButton.gameObject);
		}

		void Update() {

		}

		public void OnHitBegin() {
			UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
			TransitionManager.instance.TransitionTo(SaveLoad.instance.data.lastScene.Length > 0 ? SaveLoad.instance.data.lastScene : firstScene);
		}
		public void OnHitRestart() {
			UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
			if (!_confirmRestart) {
				restartButton.GetComponentInChildren<UnityEngine.UI.Text>().text = "Lose your progress and restart?";
				_confirmRestart = true;
			} else {
				SaveLoad.instance.data.lastScene = firstScene;
				SaveLoad.instance.Save();
				TransitionManager.instance.TransitionTo(firstScene);
			}
		}

		public void OnHitExit() {
			//
			Application.Quit();
		}
	}
}
