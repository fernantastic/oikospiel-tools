using UnityEngine;
using System.Collections;

namespace OikosTools {
	[RequireComponent(typeof(Rigidbody)), RequireComponent(typeof(CapsuleCollider))]
	public class Follow : MonoBehaviour {

		public enum TargetType {
			Player,
			Object
		}

		public Transform target;
		public TargetType targetType = TargetType.Player;

		public float sightRadius = 7;
		public float stopAtDistance = 1.15F;
		public float speed = 2;
		public string animationOnSeen = "walk";
		public string animationOnUnseen = "idle";

		bool _sawTarget = false;

		void Start () {
			if (targetType == TargetType.Player)
				target = Player.instance.transform;
		}

		void Update () {
			bool seen = Vector3.Distance(transform.position, target.position) < sightRadius;
			if (seen != _sawTarget) {
				Animation anim = GetComponentInChildren<Animation>();
				if (seen) {
					if (anim && anim[animationOnSeen] != null)
						anim.Play(animationOnSeen);
				} else {
					if (anim && anim[animationOnUnseen] != null)
						anim.Play(animationOnUnseen);
				}
				_sawTarget = seen;
			}
			if (_sawTarget) {
				if (Vector3.Distance(transform.position, target.position) > stopAtDistance) {
					transform.LookAt(target, Vector3.up);
					transform.Translate(Vector3.forward * speed * Time.deltaTime, Space.Self);
				}

			}
		}

		void OnDrawGizmosSelected() {
			Gizmos.color = _sawTarget ? Color.green : Color.gray;
			if (target != null)
				Gizmos.DrawLine(transform.position, target.position);
			//Gizmos.matrix = transform.localToWorldMatrix;
			Gizmos.color = Color.green;
			Gizmos.DrawWireSphere(transform.position, sightRadius);
			Gizmos.color = Color.blue;
			Gizmos.DrawWireSphere(transform.position, stopAtDistance);
		}
	}
}