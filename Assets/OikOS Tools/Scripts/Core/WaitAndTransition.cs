using UnityEngine;
using System.Collections;

namespace OikosTools {
	public class WaitAndTransition : MonoBehaviour {

		public float wait = 5;
		public string nextScene = "";
		public float transitionDuration = 5;

		// Use this for initialization
		void Start () {
			StartCoroutine("Next");
		}

		IEnumerator Next() {
			yield return new WaitForSeconds(wait);
			TransitionManager.instance.TransitionTo(nextScene, transitionDuration);
		}
	}
}