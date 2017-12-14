
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
	public class MenuMainMenu : MonoBehaviour {

		public CanvasGroup group;
		public string firstScene;

		public UnityEngine.UI.Button startButton;
		public UnityEngine.UI.Button restartButton;

		//bool _visible = true;
		bool _confirmRestart = false;

		// Use this for initialization
		void OnEnable () {
			Game.EnsureCoreAssets();
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
