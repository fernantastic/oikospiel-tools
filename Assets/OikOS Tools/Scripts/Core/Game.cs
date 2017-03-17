using UnityEngine;
using System.Collections;

namespace OikosTools {
	public class Game : MonoBehaviour {

		float wiggleFastSpeed { get { return Player.instance ? Player.instance.wiggleFastSpeed : 1; } }
		float wiggleSlowSpeed { get { return Player.instance ? Player.instance.wiggleSlowSpeed : 1; } }

		public static Game instance;

		public MenuPause pauseMenu;

		public static void EnsureCoreAssets() {
			if (GameObject.Find("_CORE") == null) { // TODO: find it in a better way
				Instantiate((GameObject)Resources.Load("_CORE"));
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