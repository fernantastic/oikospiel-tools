using UnityEngine;
using System.Collections;

namespace OikosTools {
	public class LookAtPlayer : MonoBehaviour {

		public Vector3 offset = Vector3.zero;
		public float speed = 0.15f;

		Vector3 _lookAtPoint;

		void Start() {
			_lookAtPoint = Player.instance.transform.position;
		}

		// Update is called once per frame
		void Update () {
			_lookAtPoint = Vector3.MoveTowards(_lookAtPoint, Player.instance.transform.position, speed);

			transform.LookAt(_lookAtPoint);
			transform.Rotate(offset);
		}
	}
}