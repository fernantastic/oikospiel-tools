
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
	public class Game : MonoBehaviour {

		float wiggleFastSpeed { get { return Player.instance ? Player.instance.wiggleFastSpeed : 1; } }
		float wiggleSlowSpeed { get { return Player.instance ? Player.instance.wiggleSlowSpeed : 1; } }

		public static Game instance;

		public MenuPause pauseMenu;

		public static void EnsureCoreAssets() {
			Debug.Log("Initializing core assets");
			if (instance == null) {
				var go = Instantiate((GameObject)Resources.Load("_CORE"));
				Debug.Assert(go != null);
			}
		}

		void Awake() {
			instance = this;
		}
		void OnEnable () {
			SaveLoad.instance.Load();
		}

		void Start() {
			GenericValueSystem.instance.ChangeValue(GenericValueSystem.ValueType.MouseWiggleFast, 0);
			GenericValueSystem.instance.ChangeValue(GenericValueSystem.ValueType.MouseWiggleSlow, 0);
			SetGenericValues();
		}

		void Update() {
			if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.Return)) {
				Screen.fullScreen = !Screen.fullScreen;
			}
			if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.F4)) {
				Application.Quit();
			}

			SetGenericValues();
		}

		void SetGenericValues() {
			GenericValueSystem.instance.ChangeValue(GenericValueSystem.ValueType.CursorX, (float)Input.mousePosition.x/(float)Screen.width);
			GenericValueSystem.instance.ChangeValue(GenericValueSystem.ValueType.CursorY, (float)Input.mousePosition.y/(float)Screen.height);

			float dif = 0;
			float wfast = GenericValueSystem.instance.GetValue(GenericValueSystem.ValueType.MouseWiggleFast);
			dif = Mathf.Abs((Input.GetAxis("Mouse X") + Input.GetAxis("Mouse Y")) * 0.5f) * 0.3f * wiggleFastSpeed * Time.timeScale;
			if (Mathf.Approximately(dif, 0))
				wfast -= 0.15f * wiggleFastSpeed * Time.timeScale;
			else
				wfast += dif;
			GenericValueSystem.instance.ChangeValue(GenericValueSystem.ValueType.MouseWiggleFast, Mathf.Clamp01(wfast));

			float wslow = GenericValueSystem.instance.GetValue(GenericValueSystem.ValueType.MouseWiggleSlow);
			dif = Mathf.Abs((Input.GetAxis("Mouse X") + Input.GetAxis("Mouse Y")) * 0.5f) * 0.02f * wiggleSlowSpeed * Time.timeScale;
			if (Mathf.Approximately(dif, 0))
				wslow -= 0.01f * wiggleSlowSpeed * Time.timeScale;
			else
				wslow += dif;
			GenericValueSystem.instance.ChangeValue(GenericValueSystem.ValueType.MouseWiggleSlow, Mathf.Clamp01(wslow));
		}

		void OnDrawGizmosSelected() {
			if (Application.isPlaying) {
				Gizmos.color = new Color(1,0,0,0.25f);
				Gizmos.DrawWireCube(transform.position + Vector3.up * 5, Vector3.one);
				Gizmos.color = Color.red;
				Gizmos.DrawWireCube(transform.position + Vector3.up * 5, Vector3.one * GenericValueSystem.instance.GetValue(GenericValueSystem.ValueType.MouseWiggleFast));

				Gizmos.color = new Color(0,0,1,0.25f);
				Gizmos.DrawWireCube(transform.position + Vector3.up * 5 + Vector3.right * 2, Vector3.one);
				Gizmos.color = Color.blue;
				Gizmos.DrawWireCube(transform.position + Vector3.up * 5 + Vector3.right * 2, Vector3.one * GenericValueSystem.instance.GetValue(GenericValueSystem.ValueType.MouseWiggleSlow));
			}
		}
	}
}